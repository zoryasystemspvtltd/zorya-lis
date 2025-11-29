using LIS.DtoModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private TCPIPSettings _settings;
        private CancellationTokenSource _cts;
        private TcpListener _listener;
        private readonly ConcurrentDictionary<string, (TcpClient Client, Task HandlerTask, CancellationTokenSource Cts)> _clients
            = new ConcurrentDictionary<string, (TcpClient, Task, CancellationTokenSource)>();
        private readonly object _shutdownLock = new object();
        private bool _isShutdown = false;
        protected StringBuilder sInputMsg = new StringBuilder();
        public string FullMessage { get; private set; }
        // Call from ConnectToTCPIP instead of creating Thread 
        public TCPIPHL7Command(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method started.");
            this._settings = settings;
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPHL7Command Constructor method completed.");
        }
        public async Task StartListenerAsync(CancellationToken externalToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            var token = _cts.Token;

            var ipAddress = IPAddress.Parse(_settings.IPAddress);
            _listener = new TcpListener(new IPEndPoint(ipAddress, _settings.PortNo));
            _listener.Start();
            // Accept loop
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    // Optional: configure socket keepalive 

                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    _ = Task.Run(() => HandleClientAsync(client, token), token); // fire-and-forget 

                }

                catch (ObjectDisposedException) { break; } // listener stopped 
                catch (Exception ex)
                {
                    this.FullMessage = ex.Message;
                    Logger.Logger.LogInstance.LogException(ex);
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
            }
        }
        // Per-client handler 
        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString();
            var stream = client.GetStream();
            var buffer = new byte[10240];
            var parserBuffer = new StringBuilder();
            var lastReceived = DateTime.UtcNow;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Read with timeout awareness 
                    var readTask = stream.ReadAsync(buffer, 0, buffer.Length, token);
                    var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(30), token)).ConfigureAwait(false);
                    if (completed != readTask)
                    {
                        // no data in 30s - check idle policy 
                        if (DateTime.UtcNow - lastReceived > TimeSpan.FromMinutes(5))
                        {
                            Logger.Logger.LogInstance.LogInfo($"Connection idle - closing: {endpoint}");
                            break; // exit loop -> cleanup -> allow reconnect 
                        }
                        continue;
                    }

                    int bytesRead = 0;
                    try
                    {
                        bytesRead = readTask.Result;
                        if (bytesRead == 0)
                        {
                            // client closed gracefully 
                            Logger.Logger.LogInstance.LogInfo($"Remote closed connection: {endpoint}");
                            break;
                        }
                    }
                    catch (IOException ioEx)
                    {
                        this.FullMessage = ioEx.Message;
                        Logger.Logger.LogInstance.LogException(ioEx);
                    }

                    lastReceived = DateTime.UtcNow;
                    var chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    parserBuffer.Append(chunk);

                    // Process complete frames if any (detect char 28 FS / char 13 CR, or ASTM framing) 
                    // Example: split by FS (char 28) into frames 

                    string data = parserBuffer.ToString();
                    Logger.Logger.LogInstance.LogInfo($"Read: {data}");
                    int fsIndex;
                    while ((fsIndex = data.IndexOf((char)28)) >= 0)
                    {
                        var frame = data.Substring(0, fsIndex); // frame without FS 
                        // Remove processed frame from buffer 
                        data = data.Substring(fsIndex + 1);
                        // Now process frame (trim VT/CR etc) 
                        var cleaned = frame.Replace(((char)11).ToString(), "")   // remove VT 
                                           .Replace(((char)13).ToString(), "\r");

                        // Process by splitting on CR and examine segments like MSH/QRD/OBR/OBX 
                        await ProcessFrameAsync(cleaned, stream).ConfigureAwait(false);
                    }
                    // Keep leftover 
                    parserBuffer.Clear();
                    parserBuffer.Append(data);
                }
            }

            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
            }
            finally
            {
                try { stream.Close(); client.Close(); }
                catch { }
            }
        }

        private async Task ProcessFrameAsync(string frame, NetworkStream stream)
        {
            try
            {
                bool orderRequest = false;
                string messageControlId = "";
                var segments = frame.Split('\r');
                foreach (var seg in segments)
                {
                    var parts = seg.Split('|');
                    if (parts.Length == 0) continue;
                    var segId = parts[0].Trim();
                    switch (segId)
                    {
                        case "MSH":
                        case "MSH":
                            messageControlId = parts[9];
                            orderRequest = parts[8] == "QRY^Q02";
                            if (!orderRequest)
                            {
                                sInputMsg.Append(seg + (char)13);
                            }
                            break;
                        case "QRD":
                            string sampleNo = parts.Length > 8 ? parts[8] : string.Empty;
                            if (!string.IsNullOrEmpty(sampleNo))
                            {
                                var response = await SendOrderData(sampleNo, messageControlId).ConfigureAwait(false);
                                if (response != null)
                                {
                                    Logger.Logger.LogInstance.LogInfo($"Write: {response.QRYResponse}");
                                    var dataBytes = Encoding.ASCII.GetBytes(response.QRYResponse);
                                    await stream.WriteAsync(dataBytes, 0, dataBytes.Length).ConfigureAwait(false);

                                    if (!string.IsNullOrEmpty(response.DSRResponse))
                                    {
                                        Logger.Logger.LogInstance.LogInfo($"Write: {response.DSRResponse}");
                                        var dsrBytes = Encoding.ASCII.GetBytes(response.DSRResponse);
                                        await stream.WriteAsync(dsrBytes, 0, dsrBytes.Length).ConfigureAwait(false);
                                    }
                                }
                            }
                            break;
                        case "OBR":
                            sInputMsg.Append(seg + (char)13);
                            break;
                        case "OBX":
                            sInputMsg.Append(seg + (char)13);
                            break;
                    }
                }
                if (sInputMsg.Length > 100)
                {
                    var response = Task.Run(async () => await ResultProcess(sInputMsg.ToString())).Result;
                    sInputMsg.Clear(); //Clear the insput message
                    Logger.Logger.LogInstance.LogInfo($"Write: {response.ToString()}");
                    var dsrBytes = Encoding.ASCII.GetBytes(response.ToString());
                    await stream.WriteAsync(dsrBytes, 0, dsrBytes.Length).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                sInputMsg.Clear();
                throw;
            }
        }

        // Call this to disconnect and stop the listener
        public async Task DisconnectToTCPIPAsync(TimeSpan? gracefulWait = null)
        {
            // default wait for handlers to exit
            var waitTimeout = gracefulWait ?? TimeSpan.FromSeconds(10);

            lock (_shutdownLock)
            {
                if (_isShutdown) return; // idempotent
                _isShutdown = true;
            }

            Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP: initiating shutdown.");

            // 1) Stop accepting new clients
            try
            {
                if (_listener != null)
                {
                    Logger.Logger.LogInstance.LogInfo("Stopping TcpListener...");
                    try
                    {
                        // Cancel accept loop first if you use a token for accept loop
                        _cts?.Cancel();
                    }
                    catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                    try
                    {
                        _listener.Stop();
                    }
                    catch (Exception ex)
                    {
                        // Sometimes Stop may throw if listener already stopped; log and continue
                        Logger.Logger.LogInstance.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
            }

            // 2) Cancel all client read loops and request graceful close
            List<Task> handlerTasks = new List<Task>();
            foreach (var kvp in _clients.ToArray())
            {
                var key = kvp.Key;
                var tuple = kvp.Value;
                try
                {
                    Logger.Logger.LogInstance.LogInfo($"DisconnectToTCPIP: closing client {key}");

                    // Cancel per-client token so their read loops can exit gracefully
                    try { tuple.Cts?.Cancel(); } catch (Exception) { }

                    // Attempt graceful shutdown on socket
                    try
                    {
                        var client = tuple.Client;
                        if (client != null && client.Connected)
                        {
                            try
                            {
                                // Graceful shutdown
                                client.Client.Shutdown(SocketShutdown.Both);
                            }
                            catch (SocketException se)
                            {
                                // ignore if remote already closed, but log
                                Logger.Logger.LogInstance.LogException(se);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.LogInstance.LogException(ex);
                    }

                    // collect handler task to await later
                    if (tuple.HandlerTask != null)
                    {
                        handlerTasks.Add(tuple.HandlerTask);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }
            }

            // 3) Give handlers some time to finish gracefully
            try
            {
                if (handlerTasks.Count > 0)
                {
                    var whenAll = Task.WhenAll(handlerTasks);
                    var finished = await Task.WhenAny(whenAll, Task.Delay(waitTimeout)).ConfigureAwait(false);
                    if (finished != whenAll)
                    {
                        Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP: timeout waiting for handler tasks to finish; forcing closure.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
            }

            // 4) Close and dispose remaining clients and streams forcefully
            foreach (var kvp in _clients.ToArray())
            {
                var key = kvp.Key;
                var tuple = kvp.Value;
                try
                {
                    try { tuple.Client?.GetStream()?.Close(); } catch { }
                    try { tuple.Client?.Close(); } catch { }
                    try { tuple.Client?.Dispose(); } catch { }
                    try { tuple.Cts?.Dispose(); } catch { }
                }
                catch (Exception ex)
                {
                    Logger.Logger.LogInstance.LogException(ex);
                }

                // remove from collection
                _clients.TryRemove(key, out _);
            }

            // 5) Dispose listener CTS
            try { _cts?.Dispose(); } catch { }
            _cts = null;

            // 6) Finally nullify or dispose listener reference
            try { _listener = null; } catch { }

            Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP: shutdown completed.");
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