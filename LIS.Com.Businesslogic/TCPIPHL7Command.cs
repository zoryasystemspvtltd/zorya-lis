using LIS.DtoModel;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class TCPIPHL7Command
    {
        private TCPIPSettings settings;
        protected Thread reportingHL7Thread;
        protected Socket soc;
        protected Stream sm;
        protected StreamWriter sw;
        protected StreamReader sr;

        protected TcpListener serverHL7;
        public bool IsReady { get; private set; }
        public string FullMessage { get; private set; }
        protected System.Timers.Timer timer;
        protected StringBuilder sInputMsg = new StringBuilder();

        public TCPIPHL7Command(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method started.");
            IsReady = false;
            this.settings = settings;
            if (this.settings.AutoConnect)
            {
                ConnectToTCPIP();
            }

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method completed.");
        }

        public void ConnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command ConnectToTCPIP method started.");
            try
            {
                var ipAddress = IPAddress.Parse(settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, settings.PortNo);
                serverHL7 = new TcpListener(localEndPoint);
                serverHL7.Start();
                reportingHL7Thread = new Thread(new ThreadStart(TCPListenHL7Data));
                reportingHL7Thread.Start();

                IsReady = true;

                Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            finally
            {
                if (reportingHL7Thread != null)
                {
                    reportingHL7Thread.Abort();//properly abort the client
                    Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                }
                if (serverHL7 != null)
                {
                    serverHL7.Stop();//properly stop the listner
                    Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                }

            }
        }

        public void DisconnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command DisconnectToTCPIP method started.");
            try
            {
                reportingHL7Thread.Abort();
                if (serverHL7 != null)
                {
                    soc.Dispose();
                    serverHL7.Stop();
                    serverHL7 = null;
                }
                IsReady = false;
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command DisconnectToTCPIP method completed.");
        }

        private async void TCPListenHL7Data()
        {
            string messageControlId = "";
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command TCPListenHL7Data method started.");
            soc = serverHL7.AcceptSocket();
            sm = new NetworkStream(soc);
            sr = new StreamReader(sm);
            sw = new StreamWriter(sm)
            {
                AutoFlush = true // enable automatic flushing
            };

            bool orderRequest = false;
            char[] charArray = new char[10240];
            while (true)
            {
                try
                {
                    var readByteCount = sr.Read(charArray, 0, charArray.Length);
                    if (readByteCount > 0)
                    {
                        var rawmsg = new string(charArray, 0, readByteCount);

                        Logger.Logger.LogInstance.LogInfo("TCP/IP Read: '{0}'", rawmsg);
                        var inputmsg = rawmsg.Split((char)28);
                        var blocks = inputmsg[0].Split((char)13);

                        foreach (var block in blocks)
                        {
                            var input = block.Split('|');
                            switch (input[0])
                            {
                                case "MSH":
                                case "MSH":
                                    orderRequest = input[8] == "QRY^Q02";
                                    messageControlId = input[9];
                                    if (!orderRequest)
                                    {
                                        sInputMsg.Append(block + (char)13);
                                    }
                                    break;
                                case "QRD":
                                    string sampleNo = input[8];
                                    if (orderRequest)
                                    {
                                        var response = await SendOrderData(sampleNo, messageControlId);

                                        //Send First order Response
                                        sw.Write(response.QRYResponse);
                                        Logger.Logger.LogInstance.LogInfo("TCP/IP Write: '{0}'", response.QRYResponse);
                                        if (response.DSRResponse != null)
                                        {
                                            //Send Order Info
                                            sw.Write(response.DSRResponse);
                                            Logger.Logger.LogInstance.LogInfo("TCP/IP Write: '{0}'", response.DSRResponse);
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
                        if (sInputMsg.Length > 1000)
                        {
                            await ResultProcess();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
            }
        }

        private async Task ResultProcess()
        {
            string messageControlId = "";
            Logger.Logger.LogInstance.LogDebug("Result process method excuted.");
            string message = sInputMsg.ToString();
            sInputMsg.Clear(); //Clear the insput message

            string[] resultMesgSegments = message.TrimEnd((char)13).Split((char)13); // vbCr<CR>
            if (resultMesgSegments.Length > 1)
            {
                string[] firstrow = resultMesgSegments[0].Split('|');
                if (firstrow[0] == "MSH" || firstrow[0] == "MSH")
                    messageControlId = firstrow[9];

                string[] field = resultMesgSegments[1].Split('|');
                if (field[0] == "OBR")
                {
                    string sampleNo = field[3];
                    bool flag = IsValidSampleNo(sampleNo);
                    string response = @"MSH|^~\&|||||||ACK|" + (char)13 + $"MSA|AR|{messageControlId}|{(char)13}";

                    if (flag && resultMesgSegments.Length > 2)
                    {
                        response = await ProccessMessage(sampleNo, message, messageControlId);
                        WriteMessage(response.ToString(), sw);
                    }
                    else
                    {
                        WriteMessage(response.ToString(), sw);
                    }
                }
            }
        }

        public bool IsValidSampleNo(string sampleNo)
        {
            if (sampleNo.Length > 4)
            {
                Logger.Logger.LogInstance.LogDebug("Sample No '{0}' is valid.", sampleNo);
                return true;
            }
            else
            {
                Logger.Logger.LogInstance.LogDebug("Sample No '{0}' is not valid.", sampleNo);
                return false;
            }
        }


        private void WriteMessage(string response, StreamWriter sw)
        {
            var finalresponse = (char)11 + response + (char)28 + (char)13;
            sw.Write(finalresponse);
            Logger.Logger.LogInstance.LogInfo("TCP/IP Write: '{0}'", finalresponse);
        }

        virtual public Task<OrderHL7Response> SendOrderData(string sampleNo, string messageControlId)
        {
            throw new NotImplementedException();
        }
        virtual public string SendResponse(string sampleNo, string messageControlId)
        {
            throw new NotImplementedException();
        }
        virtual public Task<string> ProccessMessage(string sampleNo, string message, string messageControlId)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderHL7Response
    {
        public string QRYResponse { get; set; }
        public string DSRResponse { get; set; }
    }
}