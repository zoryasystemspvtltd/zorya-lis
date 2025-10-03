using LIS.DtoModel;
using Microsoft.VisualBasic;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class TCPIPCommand
    {
        private TCPIPSettings settings;
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

        public TCPIPCommand(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPCommand Constructor method started.");
            IsReady = false;
            this.settings = settings;
            if (this.settings.AutoConnect)
            {
                ConnectToTCPIP();
            }

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPCommand Constructor method completed.");
        }

        public void ConnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand ConnectToTCPIP method started.");
            try
            {
                var ipAddress = IPAddress.Parse(settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, settings.PortNo);
                server = new TcpListener(localEndPoint);
                server.Start();
                reportingThread = new Thread(new ThreadStart(TCP_ListenData));
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

        public void DisconnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand DisconnectToTCPIP method started.");
            try
            {
                reportingThread.Abort();
                if (server != null)
                {
                    soc.Dispose();
                    server.Stop();
                    server = null;
                }
                IsReady = false;
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand DisconnectToTCPIP method completed.");
        }

        private async void TCP_ListenData()
        {
            string messageControlId = "";
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand TCP_ListenData method started.");
            soc = server.AcceptSocket();
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
                                    orderRequest = input[8] == "ORM^O01";
                                    messageControlId = input[9];
                                    if (!orderRequest)
                                    {
                                        sInputMsg.Append(block + (char)13);
                                    }
                                    break;
                                case "ORC":
                                    string sampleNo = input[3];                                    
                                    if (orderRequest)
                                    {
                                        // Send the response                                  
                                        string finalResponse = await SendOrderData(sampleNo, messageControlId);                                      
                                       
                                        sw.Write(finalResponse);
                                        Logger.Logger.LogInstance.LogInfo("COM Write: '{0}'", finalResponse);

                                        finalResponse = "";
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
                    string response = @"MSH|^~\&|LIS||||||ACK|" + (char)13 + $"MSA|AA|{messageControlId}|{(char)13}";

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
            var strlist = sampleNo.Split('.');
            if (strlist.Length == 1 && sampleNo.Length == 8)
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
            Logger.Logger.LogInstance.LogInfo("COM Write: '{0}'", finalresponse);
        }

        virtual public Task<string> SendOrderData(string sampleNo, string messageControlId)
        {
            throw new NotImplementedException();
        }
        virtual public Task<string> ProccessMessage(string sampleNo, string message, string messageControlId)
        {
            throw new NotImplementedException();
        }
    }
}