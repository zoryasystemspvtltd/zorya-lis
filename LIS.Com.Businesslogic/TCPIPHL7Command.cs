using LIS.DtoModel;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
        public bool IsConnected { get; set; }

        public TCPIPHL7Command(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method started.");
            IsReady = false;
            this.settings = settings;

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method completed.");
        }

        public void StartListener()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command ConnectToTCPIP method started.");
            try
            {
                IsConnected = true;
                var ipAddress = IPAddress.Parse(settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, settings.PortNo);
                TCPserverHL7 = new TcpListener(localEndPoint);
                TCPserverHL7.Start();

                TCPServerHL7Thread = new Thread(new ThreadStart(TCPListenHL7Data));
                TCPServerHL7Thread.Name = "SERVER" + settings.PortNo;
                TCPServerHL7Thread.Start();
               
                IsReady = true;
                Logger.Logger.LogInstance.LogDebug("TCPIPHL7Command ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                //TCPServerHL7Thread?.Abort();//properly abort the client 
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
                IsConnected = false;
                if (TCPServerHL7Thread != null)
                {
                    //TCPServerHL7Thread.s
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

        private void TCPListenHL7Data()
        {

            TcpClient client = null;
            while (true)
            {
                try
                {
                    if (IsConnected)
                    {
                        // Do NOT call Start repeatedly here. Listener was started in StartListener.

                        try
                        {
                            // AcceptTcpClient blocks until a client connects or the listener is stopped.
                            client = TCPserverHL7.AcceptTcpClient();
                        }
                        catch (SocketException sockEx)
                        {
                            // Listener was likely stopped - break if disconnected
                            Logger.Logger.LogInstance.LogException(sockEx);
                            if (!IsConnected)
                                break;
                            continue;
                        }


                        while (IsConnected && client != null && client.Connected)
                        {
                            try
                            {
                                if (!IsConnected)
                                {
                                    Logger.Logger.LogInstance.LogInfo("LIS disconnected!.");
                                    break;
                                }

                                string messageControlId = "";
                                bool orderRequest = false;
                                string message = "";

                                // Get a stream object for reading and writing
                                stream = client.GetStream();

                                // Use blocking Read with a short timeout instead of busy-waiting on DataAvailable
                                stream.ReadTimeout = 1000; // 1 second timeout so we can check IsConnected periodically
                                var buffer = new byte[4096];

                                int bytesRead = 0;
                                try
                                {
                                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                                }
                                catch (IOException ioEx)
                                {
                                    // If the read timed out, loop back to check IsConnected
                                    var se = ioEx.InnerException as SocketException;
                                    if (se != null && se.SocketErrorCode == SocketError.TimedOut)
                                    {
                                        continue;
                                    }
                                    // Other IO issues - log and break client loop
                                    Logger.Logger.LogInstance.LogException(ioEx);
                                    break;
                                }

                                if (bytesRead == 0)
                                {
                                    // Remote closed connection
                                    break;
                                }

                                message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                                if (!string.IsNullOrEmpty(message))
                                {
                                    Logger.Logger.LogInstance.LogInfo("TCP/IP Read: '{0}'", message);
                                    //Remove <SB> character from raw message
                                    //Remove <VT> character from raw message
                                    message = message.Replace("<VT>", "");
                                    message = message.Replace("<SB>", "");

                                    var inputmsg = message.Split((char)28);
                                    var blocks = inputmsg[0].Split((char)13);

                                    foreach (var block in blocks)
                                    {
                                        var input = block.Split('|');
                                        switch (input[0].Trim())
                                        {
                                            case "MSH":
                                            case "\u000BMSH":
                                                orderRequest = input.Length > 8 && input[8] == "QRY^Q02";
                                                messageControlId = input.Length > 9 ? input[9] : string.Empty;
                                                if (!orderRequest)
                                                {
                                                    sInputMsg.Append(block + (char)13);
                                                }
                                                break;
                                            case "QRD":
                                                string sampleNo = input.Length > 8 ? input[8] : string.Empty;
                                                if (orderRequest)
                                                {
                                                    var response = Task.Run(async () => await SendOrderData(sampleNo, messageControlId)).Result;

                                                    //Send First order Response
                                                    var dataBytes = Encoding.ASCII.GetBytes(response.QRYResponse);
                                                    stream.Write(dataBytes, 0, dataBytes.Length);
                                                    Logger.Logger.LogInstance.LogInfo("TCP/IP Write: '{0}'", response.QRYResponse);
                                                    if (response.DSRResponse != null)
                                                    {
                                                        //Send Order Info
                                                        var dsrBytes = Encoding.ASCII.GetBytes(response.DSRResponse);
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
                                        var response = Task.Run(async () => await ResultProcess(sInputMsg.ToString())).Result;
                                        //await ResultProcess();
                                        sInputMsg.Clear(); //Clear the insput message
                                        WriteMessage(response.ToString());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Logger.LogInstance.LogException(ex);
                            }
                        }
                        if (client != null)
                            client.Close();
                    }
                }
                catch (SocketException ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
                if (!IsConnected)
                {
                    if (client != null)
                        client.Close();

                    Logger.Logger.LogInstance.LogInfo("LIS disconnected!.");
                    break;
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

        /// <summary>
        /// CR	or (char)13 carriage return
        /// VT	or (char)11 start block
        /// FS	or (char)28 start block
        /// </summary>
        /// <param name="response"></param>
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

        virtual public Task<string> ResultProcess(string message)
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