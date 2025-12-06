using LIS.DtoModel;
using System;
using System.Collections;
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
    public class TCPIPASTMCommand
    {
        private TCPIPSettings _settings;
        private CancellationTokenSource _cts;
        private TcpListener _listener;
        private readonly ConcurrentDictionary<string, (TcpClient Client, Task HandlerTask, CancellationTokenSource Cts)> _clients
            = new ConcurrentDictionary<string, (TcpClient, Task, CancellationTokenSource)>();
        private readonly object _shutdownLock = new object();
        private bool _isShutdown = false;

        // Backwards-compatible fields kept for existing derived code, but handlers use per-client builders
        protected string[] output = new string[5];
        protected int index;
        public bool IsReady { get; private set; }
        public bool IsRunning { get; private set; }
        public string Message { get; private set; }
        public string FullMessage { get; private set; }
        public bool IsConnected { get; private set; }

        // Note: this field retained for compatibility with code that expects a single stream,
        // handlers will also use a per-client stream variable.
        protected NetworkStream stream;

        public TCPIPASTMCommand(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method started.");
            _settings = settings;
            IsReady = false;
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method completed.");
        }

        // New async listener modeled after TCPIPHL7Command
        public async Task StartListenerAsync(CancellationToken externalToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            var token = _cts.Token;

            IPAddress ipAddress;
            if (!IPAddress.TryParse(_settings.IPAddress, out ipAddress))
            {
                Logger.Logger.LogInstance.LogWarning($"Invalid IP '{_settings.IPAddress}' in settings. Falling back to IPAddress.Any.");
                ipAddress = IPAddress.Any;
            }

            _listener = new TcpListener(new IPEndPoint(ipAddress, _settings.PortNo));
            _listener.Start();
            IsConnected = true;
            IsReady = true;

            Logger.Logger.LogInstance.LogInfo($"TCP ASTM listener started at {_settings.IPAddress}:{_settings.PortNo}");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    // configure socket keepalive and NoDelay (Nagle off)
                    try
                    {
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        client.Client.NoDelay = true;
                    }
                    catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                    var clientCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    var clientKey = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();

                    var handlerTask = Task.Run(() => HandleClientAsync(client, clientCts.Token, clientKey), clientCts.Token);

                    _clients.TryAdd(clientKey, (client, handlerTask, clientCts));

                    Logger.Logger.LogInstance.LogInfo($"Accepted ASTM connection from {clientKey}. Handler started.");
                }
                catch (ObjectDisposedException) { break; } // listener stopped
                catch (Exception ex)
                {
                    this.FullMessage = ex.Message;
                    Logger.Logger.LogInstance.LogException(ex);
                    try { await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false); } catch { }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token, string clientKey)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString();
            NetworkStream clientStream = null;
            var buffer = new byte[8 * 1024];
            DateTime lastReceived = DateTime.UtcNow;
            TimeSpan idleThreshold = TimeSpan.FromMinutes(10);
            TimeSpan probeTimeout = TimeSpan.FromSeconds(5);

            // per-client builders to avoid cross-talk
            var clientInputBuilder = new StringBuilder();
            var clientMessageBuilder = new StringBuilder();

            try
            {
                clientStream = client.GetStream();

                // for backward compatibility some code may use protected 'stream' field;
                // set it to this client's stream while the handler runs.
                try { stream = clientStream; } catch { }

                // Ensure keepalive
                try { client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); } catch { }

                while (!token.IsCancellationRequested && client.Connected)
                {
                    Task<int> readTask = null;
                    try
                    {
                        readTask = clientStream.ReadAsync(buffer, 0, buffer.Length, token);
                        var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(30), token)).ConfigureAwait(false);

                        if (completed != readTask)
                        {
                            // no data in 30s - check idle policy
                            if (DateTime.UtcNow - lastReceived > idleThreshold)
                            {
                                Logger.Logger.LogInstance.LogInfo($"ASTM connection idle threshold reached for {endpoint}. Sending ENQ probe before close.");

                                // send ENQ (0x05) probe - non-blocking attempt
                                try
                                {
                                    var enq = new byte[] { 0x05 };
                                    await clientStream.WriteAsync(enq, 0, 1, token).ConfigureAwait(false);
                                    await clientStream.FlushAsync(token).ConfigureAwait(false);
                                }
                                catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                                var probeDeadline = DateTime.UtcNow + probeTimeout;
                                bool activitySeen = false;
                                while (DateTime.UtcNow < probeDeadline && !token.IsCancellationRequested)
                                {
                                    if (DateTime.UtcNow - lastReceived < TimeSpan.FromSeconds(1))
                                    {
                                        activitySeen = true;
                                        break;
                                    }
                                    try { await Task.Delay(200, token).ConfigureAwait(false); } catch { break; }
                                }

                                if (!activitySeen)
                                {
                                    Logger.Logger.LogInstance.LogInfo($"No activity after probe - closing ASTM connection: {endpoint}");
                                    break;
                                }
                                else
                                {
                                    lastReceived = DateTime.UtcNow;
                                    continue;
                                }
                            }
                            continue;
                        }

                        int bytesRead = await readTask.ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            // remote closed
                            Logger.Logger.LogInstance.LogInfo($"ASTM remote closed connection: {endpoint}");
                            break;
                        }

                        lastReceived = DateTime.UtcNow;

                        var chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        // process chunk char-by-char to handle control characters and block protocol
                        for (int i = 0; i < chunk.Length; i++)
                        {
                            char ch = chunk[i];

                            switch (ch)
                            {
                                case (char)5: // ENQ -> respond with ACK
                                    {
                                        await SafeWriteAsync(clientStream, ((char)6).ToString(), token).ConfigureAwait(false);
                                        break;
                                    }

                                case (char)6: // ACK -> send next frame or EOT
                                    {
                                        if (index < 4 && !string.IsNullOrEmpty(output[index + 1]))
                                        {
                                            // STX (0x02) + data-with-checksum + CR
                                            var payload = ((char)2) + Add_CheckSum(output[index + 1]) + (char)13;
                                            await SafeWriteAsync(clientStream, payload, token).ConfigureAwait(false);
                                            index += 1;
                                        }
                                        else
                                        {
                                            await SafeWriteAsync(clientStream, ((char)4).ToString(), token).ConfigureAwait(false); // EOT
                                            index = 0;
                                            for (int k = 0; k <= 4; k++)
                                                output[k] = string.Empty;
                                        }
                                        break;
                                    }

                                case (char)4: // EOT -> process accumulated message
                                    {
                                        var message = clientMessageBuilder.ToString();
                                        Logger.Logger.LogInstance.LogInfo($"ASTM Received complete message: {message}");
                                        try
                                        {
                                            await CreateMessage(message).ConfigureAwait(false);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Logger.LogInstance.LogException(ex);
                                        }
                                        clientMessageBuilder.Clear();
                                        break;
                                    }

                                case (char)2: // STX - start of text; reset current block builder
                                    {
                                        clientInputBuilder.Clear();
                                        // Some protocols include block number as first char after STX - keep as-is
                                        clientInputBuilder.Append(ch);
                                        break;
                                    }

                                case (char)3: // ETX - end of text; typically followed by checksum
                                case (char)17: // ETB
                                    {
                                        clientInputBuilder.Append(ch);
                                        // After ETX/ETB there will be two hex checksum chars then CR. We will attempt to capture them from the stream if present.
                                        // If not present (split across reads), the remaining logic will catch and process when present.
                                        // At a minimum, append current block to message builder.
                                        clientMessageBuilder.Append(clientInputBuilder.ToString());
                                        clientInputBuilder.Clear();
                                        break;
                                    }

                                default:
                                    {
                                        clientInputBuilder.Append(ch);
                                        clientMessageBuilder.Append(ch);

                                        // If newline seen, reply with ACK per original implementation
                                        if (ch == '\n')
                                        {
                                            await SafeWriteAsync(clientStream, ((char)6).ToString(), token).ConfigureAwait(false);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    catch (IOException ioEx)
                    {
                        var se = ioEx.InnerException as SocketException;
                        if (se != null && se.SocketErrorCode == SocketError.TimedOut)
                        {
                            // read timed out - continue to loop
                            continue;
                        }
                        Logger.Logger.LogInstance.LogException(ioEx);
                        break;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.LogInstance.LogException(ex);
                        break;
                    }
                } // while
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
            }
            finally
            {
                try
                {
                    Logger.Logger.LogInstance.LogInfo($"ASTM handler exiting and closing client: {endpoint}");
                    try { clientStream?.Close(); } catch { }
                    try { client.Close(); } catch { }
                    try { client.Dispose(); } catch { }
                }
                catch { }

                _clients.TryRemove(clientKey, out var entry);
                try { entry.Cts?.Dispose(); } catch { }

                // Clear compatibility stream if it points to this client's stream
                try
                {
                    if (stream == clientStream)
                        stream = null;
                }
                catch { }
            }
        }

        // Compute checksum same semantics as original VB implementation:
        // sum of bytes from block-number (first char after STX) through ETX/ETB,
        // keep low-order 8 bits and present hex as uppercase with leading zero if needed.
        public string Add_CheckSum(string input)
        {
            Logger.Logger.LogInstance.LogDebug($"TCPIPASTMCommand Add_CheckSum: '{input}'");
            if (input == null) return string.Empty;

            int chk = 0;
            for (int i = 0; i < input.Length; i++)
            {
                chk += (byte)input[i];
            }

            int low = chk % 256;
            string hex = low.ToString("X");
            if (hex.Length == 1) hex = "0" + hex;

            var result = input + hex;
            Logger.Logger.LogInstance.LogDebug($"Add_CheckSum Return: '{result}'");
            return result;
        }

        // Safe write for ASTM (sends raw text)
        protected void WriteToPort(string text)
        {
            // kept for compatibility; uses the last assigned 'stream' (set while handling a client).
            if (stream == null || string.IsNullOrEmpty(text)) return;

            try
            {
                var dataBytes = Encoding.ASCII.GetBytes(text);
                stream.Write(dataBytes, 0, dataBytes.Length);
                Logger.Logger.LogInstance.LogInfo($"TCPIPASTMCommand Write: '{text}'");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        private async Task SafeWriteAsync(NetworkStream streamToUse, string text, CancellationToken token)
        {
            if (streamToUse == null || string.IsNullOrEmpty(text)) return;

            try
            {
                var bytes = Encoding.ASCII.GetBytes(text);
                await streamToUse.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                await streamToUse.FlushAsync(token).ConfigureAwait(false);

                var logged = bytes.Length > 200 ? Encoding.ASCII.GetString(bytes, 0, 200) + "..." : Encoding.ASCII.GetString(bytes);
                Logger.Logger.LogInstance.LogInfo($"ASTM Write: {logged}");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
            }
        }

        // Graceful shutdown mirroring TCPIPHL7Command
        public async Task DisconnectToTCPIPAsync(TimeSpan? gracefulWait = null)
        {
            var waitTimeout = gracefulWait ?? TimeSpan.FromSeconds(10);

            lock (_shutdownLock)
            {
                if (_isShutdown) return;
                _isShutdown = true;
            }

            Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP (ASTM): initiating shutdown.");

            try
            {
                if (_listener != null)
                {
                    Logger.Logger.LogInstance.LogInfo("Stopping TcpListener (ASTM)...");
                    try { _cts?.Cancel(); } catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                    try { _listener.Stop(); } catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }
                }
            }
            catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

            var handlerTasks = new System.Collections.Generic.List<Task>();
            foreach (var kvp in _clients.ToArray())
            {
                var key = kvp.Key;
                var tuple = kvp.Value;
                try
                {
                    Logger.Logger.LogInstance.LogInfo($"DisconnectToTCPIP (ASTM): closing client {key}");
                    try { tuple.Cts?.Cancel(); } catch { }

                    try
                    {
                        var client = tuple.Client;
                        if (client != null && client.Connected)
                        {
                            try { client.Client.Shutdown(SocketShutdown.Both); } catch (SocketException se) { Logger.Logger.LogInstance.LogException(se); }
                        }
                    }
                    catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                    if (tuple.HandlerTask != null) handlerTasks.Add(tuple.HandlerTask);
                }
                catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }
            }

            try
            {
                if (handlerTasks.Count > 0)
                {
                    var whenAll = Task.WhenAll(handlerTasks);
                    var finished = await Task.WhenAny(whenAll, Task.Delay(waitTimeout)).ConfigureAwait(false);
                    if (finished != whenAll)
                    {
                        Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP (ASTM): timeout waiting for handler tasks to finish; forcing closure.");
                    }
                }
            }
            catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

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
                catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                _clients.TryRemove(key, out _);
            }

            try { _cts?.Dispose(); } catch { }
            _cts = null;
            try { _listener = null; } catch { }

            IsConnected = false;
            IsReady = false;

            Logger.Logger.LogInstance.LogInfo("DisconnectToTCPIP (ASTM): shutdown completed.");
        }

        // virtuals to be implemented by derived classes (preserve original signatures)
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