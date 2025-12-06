using LIS.DtoModel;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace LIS.Com.Businesslogic
{
    public class TCPIPASTMCommand
    {
        private TCPIPSettings settings;
        protected Thread TCPServerASTMThread;
        NetworkStream stream;
        protected TcpListener TCPServerASTM;
        protected string[] output;
        protected int index;
        public bool IsReady { get; private set; }

        public string Message { get; private set; }

        public bool IsRunning { get; private set; }
        public string FullMessage { get; private set; }
        protected System.Timers.Timer timer;
        protected string sInputMsg = "";
        public bool IsConnected { get; set; }
        public TCPIPASTMCommand(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method started.");
            IsReady = false;
            this.settings = settings;

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method completed.");
        }

        public void StartListener()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand ConnectToTCPIP method started.");
            try
            {
                IsConnected = true;
                var ipAddress = IPAddress.Parse(settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, settings.PortNo);
                TCPServerASTM = new TcpListener(localEndPoint);
                TCPServerASTM.Start();
                TCPServerASTMThread = new Thread(new ThreadStart(TCPIPListenASTMData));
                TCPServerASTMThread.Name = "SERVER" + settings.PortNo;
                TCPServerASTMThread.Start();
                IsReady = true;

                Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                // TCPServerASTMThread?.Abort();//properly abort the thread
                TCPServerASTM?.Stop();//properly stop the listner
                Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        public void DisconnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand DisconnectToTCPIP method started.");
            try
            {
                IsConnected = false;
                if (TCPServerASTMThread != null)
                {
                    //TCPServerASTMThread.Abort();
                    TCPServerASTM.Stop();
                }
                IsReady = false;
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand DisconnectToTCPIP method completed.");
        }

        /// ENQ or (char)5 -enquiry
        /// ACK or (char)6 -acknowledge
        /// STX or (char)2- start of text
        /// ETX or (char)3- end of text
        /// ETB or (char)17- end of text
        /// EOT or (char)4 - end of transmission
        /// NAK or (char)21 - negative acknowledge
        /// DLE or (char)10 - data link escape 
        /// CR	or (char)13 carriage return
        private async void TCPIPListenASTMData()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand TCPIPListenASTMData method started.");
            TcpClient client = null;
            while (true)
            {
                try
                {
                    if (IsConnected)
                    {
                        // Do not call Start repeatedly; listener started in StartListener
                        try
                        {
                            client = TCPServerASTM.AcceptTcpClient();
                        }
                        catch (SocketException sockEx)
                        {
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
                                    Logger.Logger.LogInstance.LogInfo("LIS disconnected!." );
                                    break;
                                }

                                string message = "";

                                // Get a stream object for reading and writing
                                stream = client.GetStream();

                                // Use blocking Read with a timeout instead of busy-waiting on DataAvailable
                                stream.ReadTimeout = 1000; // 1 second timeout
                                var buffer = new byte[4096];
                                int bytesRead = 0;
                                try
                                {
                                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                                }
                                catch (IOException ioEx)
                                {
                                    var se = ioEx.InnerException as SocketException;
                                    if (se != null && se.SocketErrorCode == SocketError.TimedOut)
                                    {
                                        // read timed out, loop to check IsConnected
                                        continue;
                                    }
                                    Logger.Logger.LogInstance.LogException(ioEx);
                                    break;
                                }

                                if (bytesRead == 0)
                                {
                                    // remote closed connection
                                    break;
                                }

                                message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                                if (message != string.Empty)
                                {
                                    var InpBuffer = message.ToCharArray();
                                    switch (InpBuffer[0])
                                    {
                                        case (char)5:        // Check for <ENQ>
                                            {
                                                WriteToPort("" + (char)6);
                                                break;
                                            }

                                        case (char)6:      // Check for <ACK>
                                            {
                                                if (index < 4)
                                                {
                                                    WriteToPort((char)2 + Add_CheckSum(output[index + 1]) + (char)13);
                                                    index += 1;

                                                }
                                                else
                                                {  //(char)4 means end of transmission
                                                    WriteToPort("" + (char)4);
                                                    index = 0;
                                                    for (int i = 0; i <= 4; i++)
                                                        output[i] = string.Empty;
                                                }

                                                break;
                                            }

                                        case (char)4:   // Check For the <EOT>
                                            {
                                                Logger.Logger.LogInstance.LogInfo("SerialCommand Read: '{0}'", sInputMsg);
                                                try
                                                {
                                                    await CreateMessage(sInputMsg);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Logger.LogInstance.LogException(ex);
                                                }
                                                break;
                                            }

                                        default:
                                            {
                                                for (int i = 0; i <= InpBuffer.Length - 1; i++)
                                                {
                                                    sInputMsg += InpBuffer[i];

                                                    if (InpBuffer[i] == '\n')
                                                    {
                                                        WriteToPort("" + (char)6);
                                                    }
                                                }

                                                break;
                                            }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Logger.LogInstance.LogException(ex);
                                break;
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

                    Logger.Logger.LogInstance.LogInfo("LIS disconnected!." );
                    break;
                }
            }
        }
        /// <summary>
        ///Many serial protocols use checksum (additional bytes added at the end of the data string) to check
        ///the data integrity, as errors might occur during data transmission.        
        ///ETX (end of text) or ETB (end transmission block).
        /// The checksum is encoded as two characters sent after the <ETB> or <ETX>
        ///character.The checksum includes the first character after<STX>(the frame
        ///number) up to and including<ETB> or<ETX>.It is computed by adding the
        ///binary values of the characters, keeping the least significant eight bits of the result.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Add_CheckSum(string input)
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand Add_CheckSum: '{0}'", input);
            string output = "";
            if (input != null)
            {
                int chk = 0;
                for (int i = 0; i <= input.Length - 1; i++)
                    chk += Strings.Asc(input[i]);
                if ((chk % 256) < 16)
                    output = input + "0" + Conversion.Hex(chk % 256);
                else
                    output = input + Conversion.Hex(chk % 256);

                Logger.Logger.LogInstance.LogDebug("Add_CheckSum Return: '{0}'", output);
            }
            return output;
        }

        protected void WriteToPort(string text)
        {
            ASCIIEncoding encd = new ASCIIEncoding();
            var dataBytes = encd.GetBytes(text);
            stream.Write(dataBytes, 0, dataBytes.Length);
            Logger.Logger.LogInstance.LogInfo("TCPIPASTMCommand Write: '{0}'", text);
        }

        virtual public Task SendOrderData(string sampleNo)
        {
            throw new NotImplementedException();
        }
        virtual public Task ParseMessage(string message, ArrayList sampleIdLst)
        {
            throw new NotImplementedException();
        }

        virtual public Task CreateMessage(string message)
        {
            throw new NotImplementedException();
        }

        virtual public Task Identify(string message)
        {
            throw new NotImplementedException();
        }
    }

}