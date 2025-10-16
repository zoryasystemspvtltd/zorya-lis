using LIS.DtoModel;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LIS.Com.Businesslogic
{
    public class TCPIPASTMCommand
    {
        private TCPIPSettings settings;
        protected Thread reportingASTMThread;
        protected Socket soc;
        protected Stream sm;
        protected StreamWriter sw;
        protected StreamReader sr;

        protected TcpListener serverASTM;
        protected string[] data;
        protected int index;
        public bool IsReady { get; private set; }

        public string Message { get; private set; }

        public bool IsRunning { get; private set; }
        public string FullMessage { get; private set; }
        protected System.Timers.Timer timer;
        protected string sInputMsg = "";

        public TCPIPASTMCommand(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method started.");
            IsReady = false;
            this.settings = settings;
            if (this.settings.AutoConnect)
            {
                ConnectToTCPIP();
            }

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method completed.");
        }

        public void ConnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand ConnectToTCPIP method started.");
            try
            {
                var ipAddress = IPAddress.Parse(settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, settings.PortNo);
                serverASTM = new TcpListener(localEndPoint);
                serverASTM.Start();
                reportingASTMThread = new Thread(new ThreadStart(TCPIPListenASTMData));
                reportingASTMThread.Start();
                IsReady = true;

                Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand ConnectToTCPIP method completed.");
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            finally
            {
                if (reportingASTMThread != null)
                {
                    reportingASTMThread.Abort();//properly abort the client
                    Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                }
                if (serverASTM != null)
                {
                    serverASTM.Stop();//properly stop the listner
                    Logger.Logger.LogInstance.LogDebug("Server Stopped.");
                }

            }
        }

        public void DisconnectToTCPIP()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPASTMCommand DisconnectToTCPIP method started.");
            try
            {
                reportingASTMThread.Abort();
                if (serverASTM != null)
                {
                    soc.Dispose();
                    serverASTM.Stop();
                    serverASTM = null;
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
            soc = serverASTM.AcceptSocket();
            sm = new NetworkStream(soc);
            sr = new StreamReader(sm);
            sw = new StreamWriter(sm)
            {
                AutoFlush = true // enable automatic flushing
            };

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
                        var InpBuffer = rawmsg.ToCharArray();
                        int failCount = 0;
                        switch (InpBuffer[0])
                        {
                            case (char)5:        // Check for <ENQ>
                                {
                                    WriteToPort("" + (char)6);
                                    break;
                                }

                            case (char)6:      // Check for <ACK>
                                {
                                    failCount = 0;
                                    switch (index)
                                    {
                                        case 0:
                                            //(char)2 means start of text
                                            WriteToPort((char)2 + Add_CheckSum(data[index + 1]) + Constants.vbCrLf);
                                            index = 1;
                                            break;
                                        case 1:
                                            //(char)2 means start of text
                                            WriteToPort((char)2 + Add_CheckSum(data[index + 1]) + Constants.vbCrLf);
                                            index = 2;
                                            break;
                                        case 2:
                                            //(char)2 means start of text
                                            WriteToPort((char)2 + Add_CheckSum(data[index + 1]) + Constants.vbCrLf);
                                            index = 3;
                                            break;
                                        case 3:
                                            //(char)2 means start of text
                                            WriteToPort((char)2 + Add_CheckSum(data[index + 1]) + Constants.vbCrLf);
                                            index = 4;
                                            break;
                                        case 4:
                                            //(char)2 means start of text
                                            WriteToPort((char)2 + Add_CheckSum(data[index + 1]) + Constants.vbCrLf);
                                            index = 5;
                                            break;
                                        default:
                                            //(char)4 means end of transmission
                                            WriteToPort("" + (char)4);
                                            index = 0;
                                            this.IsRunning = false;
                                            break;
                                    }
                                    break;
                                }

                            //When the EVOLIS receives a <NAK> for a frame rejected by a host it resends the frame.
                            //Frames are invalidated when:
                            //1. Any character errors are detected(ie.parity error, framing error)
                            //2. The frame checksum does not match the checksum computed on the received frame.
                            //2. The frame number is not the same as the last accepted frame or one number higher.
                            case (char)21:       // Check for <NAK>
                                { 
                                    if (failCount < 3)
                                    {
                                        if (index > 0)
                                        {
                                            WriteToPort((char)2 + Add_CheckSum(data[index]) + Constants.vbCrLf);
                                        }
                                        else
                                        {
                                            WriteToPort(data[index]);
                                        }
                                    }
                                    else
                                    {
                                        this.IsRunning = false;
                                    }

                                    failCount++;
                                    break;
                                }
                            case (char)4:   // Check For the <EOT>
                                {
                                    Logger.Logger.LogInstance.LogInfo("SerialCommand Read: '{0}'", sInputMsg);
                                    await CreateMessage(sInputMsg);
                                    break;
                                }

                            default:
                                {
                                    for (int i = 0; i <= InpBuffer.Length - 1; i++)
                                    {
                                        sInputMsg += InpBuffer[i];

                                        if (InpBuffer[i] == Strings.Chr(10))
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
            sw.Write(text);
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