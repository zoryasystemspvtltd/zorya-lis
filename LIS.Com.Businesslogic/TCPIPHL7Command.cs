using LIS.DtoModel;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
        public bool AnalyzerActive { get; private set; } = false;
        public string FullMessage { get; private set; }
        protected System.Timers.Timer timer;
        private CancellationTokenSource disconnectTokenSource;
        private readonly object _lockObject = new object();
        private volatile bool _connectionEstablished = false;

        public TCPIPHL7Command(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method started.");
            this._settings = settings;

            // Initialize heartbeat timer (1 minute = 60000ms)
            timer = new System.Timers.Timer(60000);
            timer.Elapsed += OnHeartbeatTimerElapsed;
            timer.AutoReset = true;

            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method completed.");
        }

        public void StartListenerAsync()
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand ConnectToTCPIP method started.");
            try
            {
                if (string.IsNullOrWhiteSpace(_settings?.IPAddress) || _settings.PortNo <= 0)
                    throw new ArgumentException("Invalid TCP settings");

                var ipAddress = IPAddress.Parse(_settings.IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _settings.PortNo);
                server = new TcpListener(localEndPoint);
                server.Start();
                disconnectTokenSource = new CancellationTokenSource();
                reportingThread = new Thread(() => TCP_ListenData(disconnectTokenSource.Token));
                reportingThread.IsBackground = true;
                reportingThread.Start();
                IsReady = true;
                Logger.Logger.LogInstance.LogDebug("TCPIPCommand ConnectToTCPIP method completed.");
                
                Thread.SpinWait(5000);
                SendHeartbit();
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        private void TCP_ListenData(CancellationToken token)
        {
            Logger.Logger.LogInstance.LogDebug("TCPIPCommand TCP_ListenData method started.");
            try
            {
                // Wait for client connection with timeout
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                if (!server.Pending()) // 5 second timeout
                {
                    if (stopwatch.ElapsedMilliseconds > 5000) // 5 second timeout
                    {
                        Logger.Logger.LogInstance.LogWarning("No client connected within 5 seconds");
                        return;
                    }
                    Thread.Sleep(100);
                }

                soc = server.AcceptSocket();
                if (soc == null)
                {
                    Logger.Logger.LogInstance.LogError("Failed to accept socket");
                    return;
                }

                sm = new NetworkStream(soc, true);
                if (sm == null)
                {
                    Logger.Logger.LogInstance.LogError("Failed to create NetworkStream");
                    return;
                }

                sr = new StreamReader(sm, Encoding.ASCII);
                sw = new StreamWriter(sm, Encoding.ASCII) { AutoFlush = true };

                if (sr == null || sw == null)
                {
                    Logger.Logger.LogInstance.LogError("Failed to create StreamReader/Writer");
                    return;
                }

                _connectionEstablished = true;
                Logger.Logger.LogInstance.LogInfo("TCP connection established successfully");

                // Start heartbeat timer
                lock (_lockObject)
                {
                    timer.Start();
                }

                ProcessIncomingMessages(token);
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
            }
        }


        private void ProcessIncomingMessages(CancellationToken token)
        {
            string messageControlId = "";
            char[] charArray = new char[10240];
            var sInputMsg = new StringBuilder();
            var messageBuffer = new StringBuilder();

            while (!token.IsCancellationRequested && _connectionEstablished)
            {
                try
                {
                    if (sr == null || !soc.Connected) break;

                    int readByteCount = sr.Read(charArray, 0, charArray.Length);
                    if (readByteCount == 0)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    string rawmsg = new string(charArray, 0, readByteCount);
                    Logger.Logger.LogInstance.LogInfo("COM Read: '{0}'", rawmsg);

                    messageBuffer.Append(rawmsg);
                    ProcessBufferedMessages(messageBuffer, ref sInputMsg, ref messageControlId);
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Logger.Logger.LogInstance.LogException(ex);
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void ProcessBufferedMessages(StringBuilder messageBuffer, ref StringBuilder sInputMsg, ref string messageControlId)
        {
            string bufferContent = messageBuffer.ToString();
            int fsIndex = bufferContent.IndexOf((char)28);

            while (fsIndex >= 0)
            {
                try
                {
                    string completeMsg = bufferContent.Substring(0, fsIndex + 1);
                    messageBuffer.Remove(0, fsIndex + 1);

                    string hl7Content = completeMsg.Length > 2 ? completeMsg.Substring(1, completeMsg.Length - 2) : "";
                    if (string.IsNullOrEmpty(hl7Content)) continue;

                    var blocks = hl7Content.Split((char)13);
                    bool orderRequest = false;

                    foreach (var block in blocks)
                    {
                        if (string.IsNullOrWhiteSpace(block)) continue;

                        var input = block.Split('|');
                        if (input.Length == 0) continue;

                        string segmentType = input[0].TrimStart('\u000B', '|');

                        switch (segmentType.Trim())
                        {
                            case "MSH":
                                sInputMsg.Clear();
                                orderRequest = input.Length > 8 && input[8] == "QRY^Q02";
                                messageControlId = input.Length > 9 ? input[9] : "";
                                if (!orderRequest)
                                {
                                    sInputMsg.Append(block + (char)13);
                                }
                                break;
                            case "QRD":
                                if (orderRequest && input.Length > 8)
                                {
                                    var sampleId = input[8];
                                    var response = SendOrderData(sampleId, messageControlId).Result;
                                    if (response != null)
                                    {
                                        if (!string.IsNullOrEmpty(response.QRYResponse))
                                            WriteResponseSafe(response.QRYResponse);
                                        if (!string.IsNullOrEmpty(response.DSRResponse))
                                            WriteResponseSafe(response.DSRResponse);
                                    }
                                }
                                break;
                            case "OBR":
                            case "OBX":
                                sInputMsg.Append(block + (char)13);
                                break;
                        }
                    }

                    if (sInputMsg.Length > 150)
                    {
                        ResultProcess(sInputMsg.ToString(), messageControlId).Wait();
                        sInputMsg.Clear();
                        string ackResponse = $@"MSH|^~\&|||||{DateTime.Now:yyyyMMddHHmmss}||ACK^R01|{messageControlId}|P|2.3.1||||2||ASCII{(char)13}MSA|AA|{messageControlId}|Message accepted|||0{(char)13}";
                        WriteResponseSafe(ackResponse);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }

                bufferContent = messageBuffer.ToString();
                fsIndex = bufferContent.IndexOf((char)28);
            }
        }

        private void WriteResponseSafe(string response)
        {
            lock (_lockObject)
            {
                if (_connectionEstablished && sw != null)
                {
                    try
                    {
                        WriteResponse(response, sw);
                    }
                    catch (ObjectDisposedException)
                    {
                        Logger.Logger.LogInstance.LogWarning("Attempted to write to a closed writer.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.LogInstance.LogException(ex);
                    }
                }
                else
                {
                    Logger.Logger.LogInstance.LogWarning("Write ignored: connection not established or writer is null.");
                }
            }
        }


        // Heartbeat method - sends HL7 ACK every minute to check analyzer
        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                if (!_connectionEstablished || sw == null || soc == null || !soc.Connected)
                {
                    Logger.Logger.LogInstance.LogInfo("Analyzer disconnected!");
                    AnalyzerActive = false;
                    timer.Stop();
                    return;
                }
            }

            try
            {
                SendHeartbit();
            }
            catch (Exception ex)
            {
                AnalyzerActive = false;
                Logger.Logger.LogInstance.LogError("Heartbeat failed: {0}", ex.Message);
            }
        }


        private void SendHeartbit()
        {
            string heartbeatControlId = "HB" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string heartbeatMsg = $@"MSH|^~\&|LIS|LAB|ANALYZER|RECEIVER|{DateTime.Now:yyyyMMddHHmmss}||ACK|P|2.3.1||||2||ASCII{(char)13}
MSA|AA|{heartbeatControlId}|Analyzer heartbeat check{(char)13}";

            Logger.Logger.LogInstance.LogInfo("Sending heartbeat ACK (ID: {0})", heartbeatControlId);
            WriteResponseSafe(heartbeatMsg);
        }

        private void WriteResponse(string response, StreamWriter sw)
        {
            var res = AddHeaderAndFooterToHL7Msg(response);
            Logger.Logger.LogInstance.LogInfo("COM Write: '{0}'", res);
            char[] datachar = res.ToCharArray();
            sw.Write(datachar, 0, datachar.Length);
        }

        public string AddHeaderAndFooterToHL7Msg(string RawMessage)
        {
            char BeginFormat = (char)11;  // VT
            char EndFormat1 = (char)28;   // FS
            char EndFormat2 = (char)13;   // CR

            string NwkMessage = BeginFormat + RawMessage + EndFormat1 + EndFormat2;
            return NwkMessage;
        }

        private void CleanupConnection()
        {
            lock (_lockObject)
            {
                timer?.Stop();
                _connectionEstablished = false;

                sw?.Dispose();
                sr?.Dispose();
                sm?.Dispose();
                soc?.Dispose();

                sw = null;
                sr = null;
                sm = null;
                soc = null;
            }
        }


        public void DisconnectToTCPIPAsync()
        {
            try
            {
                disconnectTokenSource?.Cancel();
                CleanupConnection();

                server?.Stop();
                server = null;

                reportingThread?.Join(2000);
                reportingThread = null;

                IsReady = false;
                AnalyzerActive = false;
                Logger.Logger.LogInstance.LogInfo("LIS disconnected. Analyzer: {0}", AnalyzerActive);
            }
            catch (Exception ex)
            {
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
