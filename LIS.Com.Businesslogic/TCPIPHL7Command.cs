using LIS.DtoModel;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class TCPIPHL7Command
    {
        private TCPIPSettings _settings;
        protected Thread reportingThread;
        protected Socket soc;
        protected Stream sm;
        protected StreamWriter sw;
        protected StreamReader sr;
        protected TcpListener server;
        public bool IsReady { get; private set; }
        public string FullMessage { get; private set; }
        protected System.Timers.Timer timer;
        protected StringBuilder sInputMsg = new StringBuilder();
        private CancellationTokenSource disconnectTokenSource;

        public TCPIPHL7Command(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method started.");
            this._settings = settings;
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method completed.");
        }

        public void StartListenerAsync(CancellationToken externalToken)
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand ConnectToTCPIP method started.");
            try
            {
                var ipAddress = IPAddress.Parse(_settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _settings.PortNo);
                server = new TcpListener(localEndPoint);
                server.Start();
                disconnectTokenSource = new CancellationTokenSource();
                reportingThread = new Thread(() => TCP_ListenData(disconnectTokenSource.Token));
                reportingThread.Start();
                IsReady = true;
                Logger.Logger.LogInstance.LogDebug("TCPIPCommand ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        private void TCP_ListenData(CancellationToken token)
        {
            string messageControlId = "";
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand TCP_ListenData method started.");
            try
            {
                soc = server.AcceptSocket();
                sm = new NetworkStream(soc);
                sr = new StreamReader(sm);
                sw = new StreamWriter(sm) { AutoFlush = true };

                bool orderRequest = false;
                char[] charArray = new char[10240];

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var readByteCount = sr.Read(charArray, 0, charArray.Length);
                        if (readByteCount > 0)
                        {
                            var rawmsg = new string(charArray, 0, readByteCount);
                            Logger.Logger.LogInstance.LogInfo("COM Read: '{0}'", rawmsg);
                            var inputmsg = rawmsg.Split((char)28);
                            var blocks = inputmsg[0].Split((char)13);
                            
                            foreach (var block in blocks)
                            {
                                var input = block.Split('|');
                                switch (input[0])
                                {
                                    case "MSH":
                                    case "MSH":
                                        sInputMsg.Clear();
                                        orderRequest = input[8] == "QRY^Q02";
                                        messageControlId = input[9];
                                        if (!orderRequest)
                                        {
                                            sInputMsg.Append(block + (char)13);
                                        }
                                        break;
                                    case "QRD":
                                        var sampleId = input.Length > 8 ? input[8] : string.Empty;
                                        if (orderRequest)
                                        {
                                            var response = SendOrderData(sampleId, messageControlId).Result;
                                            if (response != null && !string.IsNullOrEmpty(response.QRYResponse))
                                            {
                                                WriteResponse(response.QRYResponse, sw);
                                            }

                                            if (response != null && !string.IsNullOrEmpty(response.DSRResponse))
                                            {
                                                WriteResponse(response.DSRResponse, sw);
                                            }
                                        }
                                        break;
                                    case "OBR":
                                        sInputMsg.Append(block + (char)13);
                                        break;
                                    case "OBX":
                                        sInputMsg.Append(block + (char)13);
                                        break;
                                }
                            }
                            if (sInputMsg.Length > 150)
                            {
                                ResultProcess(sInputMsg.ToString(), messageControlId).Wait();
                                sInputMsg.Clear();
                                var response = @"MSH|^~\&|||||" + DateTime.Now.ToString("yyyyMMddhhmmss") +
                                        "||ACK^R01|" + messageControlId + "|P|2.3.1||||2||ASCII||" + (char)13 +
                                        $"MSA|AA|{messageControlId}|Message accepted|||0|{(char)13}";
                                WriteResponse(response, sw);
                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Logger.Logger.LogInstance.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
            }
            finally
            {
                // Cleanup
                sw?.Dispose();
                sr?.Dispose();
                sm?.Dispose();
                soc?.Dispose();
            }
        }

        private void WriteResponse(string response,StreamWriter sw)
        {
            var res = AddHeaderAndFooterToHL7Msg(response);
            Logger.Logger.LogInstance.LogInfo("COM Write: '{0}'", res);
            char[] datachar = res.ToCharArray();
            sw.Write(datachar, 0, datachar.Length);
        }
        public string AddHeaderAndFooterToHL7Msg(string RawMessage)
        {
            char BeginFormat = (char)11;
            char EndFormat1 = (char)28;
            char EndFormat2 = (char)13;

            string NwkMessage = RawMessage.PadLeft(RawMessage.Length + 1, BeginFormat);
            string NwkMessage1 = NwkMessage.PadRight(NwkMessage.Length + 1, EndFormat1);
            string NwkMessage2 = NwkMessage1.PadRight(NwkMessage1.Length + 1, EndFormat2);

            return NwkMessage2;
        }

        public void DisconnectToTCPIPAsync()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand DisconnectToTCPIP method started.");
            try
            {
                disconnectTokenSource?.Cancel();
                if (server != null)
                {
                    server.Stop();
                    server = null;
                }
                if (reportingThread != null && reportingThread.IsAlive)
                {
                    reportingThread.Join(1000);
                    if (reportingThread.IsAlive)
                    {
                        reportingThread.Interrupt();
                    }
                    reportingThread = null;
                }
                IsReady = false;
                Logger.Logger.LogInstance.LogInfo($"LIS disconnected!.");
                Logger.Logger.LogInstance.LogDebug("TCPIPCommand DisconnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        virtual public Task<OrderHL7Response> SendOrderData(string sampleNo, string messageControlId)
        {
            throw new NotImplementedException();
        }

        virtual public string SendResponse(string sampleNo, string messageControlId)
        {
            throw new NotImplementedException();
        }

        virtual public Task ResultProcess(string message, string messageControlId)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderHL7Response
    {
        public string QRYResponse { get; set; }
        public string DSRResponse { get; set; }
    }

    public class ResultItem
    {
        public string TestCode { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string AbnormalFlags { get; set; }
    }
}
