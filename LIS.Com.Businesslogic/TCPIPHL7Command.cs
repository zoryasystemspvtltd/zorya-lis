using LIS.DtoModel;
using System;
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
        protected Thread TCPServerHL7Thread;
        protected TcpListener TCPserverHL7;
        NetworkStream stream;
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
                TCPserverHL7 = new TcpListener(localEndPoint);
                TCPserverHL7.Start();
                TCPServerHL7Thread = new Thread(new ThreadStart(TCPListenHL7Data));
                TCPServerHL7Thread.Name = "SERVER";
                TCPServerHL7Thread.Start();

                IsReady = true;
                Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                TCPServerHL7Thread?.Abort();//properly abort the client 
                TCPserverHL7?.Stop();//properly stop the listner
                Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        public void DisconnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command DisconnectToTCPIP method started.");
            try
            {
                if (TCPServerHL7Thread != null)
                {
                    TCPServerHL7Thread.Abort();
                    TCPserverHL7.Stop();
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
            while (true)
            {
                try
                {

                    TCPserverHL7.Start();
                    TcpClient client = TCPserverHL7.AcceptTcpClient();

                    while (true)
                    {
                        try
                        {
                            string messageControlId = "";
                            bool orderRequest = false;
                            string message = "";
                            int read = 0;

                            // Get a stream object for reading and writing
                            stream = client.GetStream();

                            // Loop to receive all the data sent by the client.
                            while (stream.DataAvailable)
                            {
                                read = stream.ReadByte();
                                message += Convert.ToChar(read);
                            }
                            if (message != string.Empty)
                            {
                                Logger.Logger.LogInstance.LogInfo("TCP/IP Read: '{0}'", message);
                                //Remove <SB> character from raw message
                                message = message.Replace("<SB>", "");

                                var inputmsg = message.Split((char)28);
                                var blocks = inputmsg[0].Split((char)13);

                                foreach (var block in blocks)
                                {
                                    var input = block.Split('|');
                                    switch (input[0].Trim())
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
                                                ASCIIEncoding encd = new ASCIIEncoding();
                                                var response = await SendOrderData(sampleNo, messageControlId);

                                                //Send First order Response
                                                var dataBytes = encd.GetBytes(response.QRYResponse);
                                                stream.Write(dataBytes, 0, dataBytes.Length);
                                                Logger.Logger.LogInstance.LogInfo("TCP/IP Write: '{0}'", response.QRYResponse);
                                                if (response.DSRResponse != null)
                                                {
                                                    //Send Order Info
                                                    var dsrBytes = encd.GetBytes(response.DSRResponse);
                                                    stream.Write(dsrBytes, 0, dsrBytes.Length);
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
                                if (sInputMsg.Length > 100)
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
                if (field[0].Trim() == "OBR")
                {
                    string sampleNo = field[3];
                    //bool flag = IsValidSampleNo(sampleNo);
                    string response = @"MSH|^~\&|||||||ACK|" + (char)13 + $"MSA|AR|{messageControlId}|{(char)13}";

                    if (resultMesgSegments.Length > 2)
                    {
                        response = await ProccessMessage(sampleNo, message, messageControlId);
                        WriteMessage(response.ToString());
                    }
                    else
                    {
                        WriteMessage(response.ToString());
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


        private void WriteMessage(string response)
        {
            ASCIIEncoding encd = new ASCIIEncoding();
            var finalresponse = (char)11 + response + (char)28 + (char)13;
            var dsrBytes = encd.GetBytes(finalresponse);
            stream.Write(dsrBytes, 0, dsrBytes.Length);
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