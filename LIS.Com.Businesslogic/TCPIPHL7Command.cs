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

        public string FullMessage { get; private set; }

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

            IPAddress ipAddress;
            if (!IPAddress.TryParse(_settings.IPAddress, out ipAddress))
            {
                Logger.Logger.LogInstance.LogWarning($"Invalid IP '{_settings.IPAddress}' in settings. Falling back to IPAddress.Any.");
                ipAddress = IPAddress.Any;
            }

            _listener = new TcpListener(new IPEndPoint(ipAddress, _settings.PortNo));
            _listener.Start();

            Logger.Logger.LogInstance.LogInfo($"TCP listener started at {_settings.IPAddress}:{_settings.PortNo}");

            // Accept loop
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

                    // Create a per-client CTS so we can cancel per-client handler on disconnect
                    var clientCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    var clientKey = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();

                    // Start handler and store it for graceful shutdown
                    var handlerTask = Task.Run(() => HandleClientAsync(client, clientCts.Token, clientKey), clientCts.Token);

                    _clients.TryAdd(clientKey, (client, handlerTask, clientCts));

                    Logger.Logger.LogInstance.LogInfo($"Accepted connection from {clientKey}. Handler started.");
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

        // Per-client handler
        private async Task HandleClientAsync(TcpClient client, CancellationToken token, string clientKey)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString();
            NetworkStream stream = null;
            var buffer = new byte[16 * 1024];
            var parserBuffer = new StringBuilder();
            DateTime lastReceived = DateTime.UtcNow;
            TimeSpan idleThreshold = TimeSpan.FromMinutes(10); // increased idle threshold; tune as needed
            TimeSpan probeTimeout = TimeSpan.FromSeconds(5);

            // per-client input builder (no cross-talk)
            var clientInputBuilder = new StringBuilder();

            try
            {
                stream = client.GetStream();
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                while (!token.IsCancellationRequested && client.Connected)
                {
                    Task<int> readTask = null;
                    try
                    {
                        readTask = stream.ReadAsync(buffer, 0, buffer.Length, token);
                        var completed = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(30), token)).ConfigureAwait(false);

                        if (completed != readTask)
                        {
                            // no data in 30s - check idle policy
                            if (DateTime.UtcNow - lastReceived > idleThreshold)
                            {
                                Logger.Logger.LogInstance.LogInfo($"Connection idle threshold reached for {endpoint}. Sending probe before close.");

                                // send ENQ (0x05) probe - non-blocking attempt
                                try
                                {
                                    var enq = new byte[] { 0x05 };
                                    await stream.WriteAsync(enq, 0, 1, token).ConfigureAwait(false);
                                    await stream.FlushAsync(token).ConfigureAwait(false);
                                }
                                catch (Exception ex) { Logger.Logger.LogInstance.LogException(ex); }

                                // wait up to probeTimeout for lastReceived to update (polling)
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
                                    Logger.Logger.LogInstance.LogInfo($"No activity after probe - closing connection: {endpoint}");
                                    break; // exit loop -> cleanup -> allow reconnect
                                }
                                else
                                {
                                    // reset lastReceived and continue read loop
                                    lastReceived = DateTime.UtcNow;
                                    continue;
                                }
                            }
                            continue;
                        }

                        int bytesRead = await readTask.ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            // client closed gracefully
                            Logger.Logger.LogInstance.LogInfo($"Remote closed connection: {endpoint}");
                            break;
                        }

                        lastReceived = DateTime.UtcNow;

                        var chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        parserBuffer.Append(chunk);

                        // Log raw read at debug level (trim long messages)
                        string dataPreview = parserBuffer.Length > 2000 ? parserBuffer.ToString(0, 2000) + "..." : parserBuffer.ToString();
                        Logger.Logger.LogInstance.LogInfo($"Read: {dataPreview}");

                        // Process complete frames if any (detect char 28 FS)
                        string data = parserBuffer.ToString();
                        int fsIndex;
                        while ((fsIndex = data.IndexOf((char)28)) >= 0)
                        {
                            var frame = data.Substring(0, fsIndex); // frame without FS
                            data = data.Substring(fsIndex + 1);     // remove processed frame

                            // Normalize and clean control chars
                            var cleaned = frame.Replace(((char)11).ToString(), "")   // remove VT
                                               .Replace(((char)13).ToString(), "\r");

                            // Process frame (await)
                            try
                            {
                                await ProcessFrameAsync(cleaned, stream, token, clientInputBuilder).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Logger.Logger.LogInstance.LogException(ex);
                                // If processing fails, persist raw frame if needed, then continue.
                            }
                        }

                        // Keep leftover
                        parserBuffer.Clear();
                        parserBuffer.Append(data);
                    }
                    catch (IOException ioEx)
                    {
                        var se = ioEx.InnerException as SocketException;
                        if (se != null && se.SocketErrorCode == SocketError.TimedOut)
                        {
                            // just a read timeout - continue
                            continue;
                        }
                        Logger.Logger.LogInstance.LogException(ioEx);
                        break;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        // shutting down
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
                // cleanup client, remove from dictionary, dispose its CTS (if present)
                try
                {
                    Logger.Logger.LogInstance.LogInfo($"Handler exiting and closing client: {endpoint}");
                    try { stream?.Close(); } catch { }
                    try { client.Close(); } catch { }
                    try { client.Dispose(); } catch { }
                }
                catch { }

                // remove from _clients
                _clients.TryRemove(clientKey, out var entry);

                // dispose per-client CTS if present
                try { entry.Cts?.Dispose(); } catch { }
            }
        }

        /// <summary>
        /// Process a single cleaned frame. Uses a per-client builder so multiple clients do not interfere.
        /// </summary>
        private async Task ProcessFrameAsync(string frame, NetworkStream stream, CancellationToken token, StringBuilder clientInputBuilder)
        {
            // We will handle QRD (order queries) and accumulate OBR/OBX for results.
            try
            {
                // Split by CR to get HL7 segments
                var segments = frame.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

                // For results accumulation
                string currentObrPlacerOrderNo = null;
                string currentSampleId = null;
                var currentResults = new List<ResultItem>();

                // Check for MSH to extract control id
                string messageControlId = string.Empty;
                for (int i = 0; i < segments.Length; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var seg = segments[i].Trim();
                    if (string.IsNullOrEmpty(seg)) continue;

                    var parts = seg.Split('|');
                    if (parts.Length == 0) continue;

                    var segId = parts[0].Trim();

                    switch (segId)
                    {
                        case "MSH":
                        case "\u000BMSH":
                            if (parts.Length > 9) messageControlId = parts[9];
                            break;

                        case "QRD":
                            {
                                // QRD is a query; extract sample id (often in QRD-8)
                                var sampleId = parts.Length > 8 ? parts[8] : string.Empty;
                                if (!string.IsNullOrWhiteSpace(sampleId))
                                {
                                    Logger.Logger.LogInstance.LogInfo($"QRD received for sample {sampleId}, messageControlId={messageControlId}");

                                    OrderHL7Response response = null;
                                    try
                                    {
                                        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
                                        {
                                            cts.CancelAfter(TimeSpan.FromSeconds(10)); // tune as needed
                                            response = await SendOrderData(sampleId, messageControlId).ConfigureAwait(false);
                                        }
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        Logger.Logger.LogInstance.LogWarning($"SendOrderData timed out for sample {sampleId}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Logger.LogInstance.LogException(ex);
                                    }

                                    // If response contains QRYResponse (payload to send), send it (framed)
                                    if (response != null && !string.IsNullOrEmpty(response.QRYResponse))
                                    {
                                        Logger.Logger.LogInstance.LogInfo($"Sending QRYResponse for {sampleId}");
                                        await SafeWriteAsync(stream, response.QRYResponse, token).ConfigureAwait(false);
                                    }

                                    // Stream DSR and DSP asynchronously to avoid blocking read loop.
                                    if (response != null && !string.IsNullOrEmpty(response.DSRResponse))
                                    {
                                        var dsrPayload = response.DSRResponse;
                                        // Fire-and-forget streaming of DSR because it may be large; but observe exceptions
                                        _ = Task.Run(async () =>
                                        {
                                            try
                                            {
                                                // small delay if needed by vendor
                                                await Task.Delay(150).ConfigureAwait(false);
                                                await SafeWriteAsync(stream, dsrPayload, CancellationToken.None).ConfigureAwait(false);
                                                Logger.Logger.LogInstance.LogInfo($"Completed streaming DSR for {sampleId}");
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Logger.LogInstance.LogException(ex);
                                            }
                                        });
                                    }
                                }
                                else
                                {
                                    Logger.Logger.LogInstance.LogWarning("QRD received without sample id - skipping.");
                                }
                            }
                            break;

                        case "OBR":
                            {
                                // finalize previous OBR if present
                                if (currentObrPlacerOrderNo != null && currentResults.Count > 0)
                                {
                                    try
                                    {
                                        var resp = await ResultProcess(BuildObrObxMessage(currentObrPlacerOrderNo, currentResults)).ConfigureAwait(false);
                                        if (!string.IsNullOrEmpty(resp))
                                        {
                                            await SafeWriteAsync(stream, resp, token).ConfigureAwait(false);
                                            Logger.Logger.LogInstance.LogInfo($"ResultProcess response written for OBR {currentObrPlacerOrderNo}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Logger.LogInstance.LogException(ex);
                                    }
                                    finally
                                    {
                                        currentResults.Clear();
                                    }
                                }

                                // start new OBR context
                                var partsObr = seg.Split('|');
                                currentObrPlacerOrderNo = partsObr.Length > 2 ? partsObr[2] : null;
                                currentSampleId = partsObr.Length > 3 ? partsObr[3] : currentObrPlacerOrderNo;
                            }
                            break;

                        case "OBX":
                            {
                                var obxParts = seg.Split('|');
                                var ri = new ResultItem
                                {
                                    TestCode = obxParts.Length > 3 ? obxParts[3] : string.Empty,
                                    Value = obxParts.Length > 5 ? obxParts[5] : string.Empty,
                                    Units = obxParts.Length > 6 ? obxParts[6] : string.Empty,
                                    ReferenceRange = obxParts.Length > 7 ? obxParts[7] : string.Empty,
                                    AbnormalFlags = obxParts.Length > 8 ? obxParts[8] : string.Empty
                                };
                                currentResults.Add(ri);
                            }
                            break;

                        default:
                            // store other segments for possible later use
                            clientInputBuilder.Append(seg + (char)13);
                            break;
                    } // switch
                } // for

                // flush any remaining OBR results at end-of-frame
                if (currentObrPlacerOrderNo != null && currentResults.Count > 0)
                {
                    try
                    {
                        var resp = await ResultProcess(BuildObrObxMessage(currentObrPlacerOrderNo, currentResults)).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(resp))
                        {
                            await SafeWriteAsync(stream, resp, token).ConfigureAwait(false);
                            Logger.Logger.LogInstance.LogInfo($"ResultProcess response written for final OBR {currentObrPlacerOrderNo}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.LogInstance.LogException(ex);
                    }
                    finally
                    {
                        currentResults.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                Logger.Logger.LogInstance.LogException(ex);
                throw;
            }
        }

        // Helper to build a minimal combined string to pass to ResultProcess
        private string BuildObrObxMessage(string obrPlacerOrderNo, List<ResultItem> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"OBR||{obrPlacerOrderNo}|");
            foreach (var r in results)
            {
                sb.AppendLine($"OBX|||{r.TestCode}||{r.Value}|{r.Units}|{r.ReferenceRange}|{r.AbnormalFlags}|");
            }
            return sb.ToString();
        }

        // Frame HL7 message with VT (0x0B) .. FS (0x1C) CR (0x0D)
        private static byte[] FrameMessage(string hl7Message)
        {
            if (string.IsNullOrEmpty(hl7Message)) return Array.Empty<byte>();

            var sb = new StringBuilder();
            sb.Append((char)11);         // VT
            sb.Append(hl7Message);
            sb.Append((char)28);         // FS
            sb.Append((char)13);         // CR
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        // Safe write that frames the message (if not already framed)
        private async Task SafeWriteAsync(NetworkStream stream, string hl7Message, CancellationToken token)
        {
            if (stream == null || string.IsNullOrEmpty(hl7Message)) return;

            // If the message already appears to contain a trailing FS+CR, send as-is; else frame it.
            bool alreadyFramed = hl7Message.IndexOf((char)28) >= 0;
            byte[] bytes;
            if (alreadyFramed)
            {
                bytes = Encoding.ASCII.GetBytes(hl7Message);
            }
            else
            {
                bytes = FrameMessage(hl7Message);
            }

            try
            {
                await stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                await stream.FlushAsync(token).ConfigureAwait(false);

                string logged = bytes.Length > 200 ? Encoding.ASCII.GetString(bytes, 0, 200) + "..." : Encoding.ASCII.GetString(bytes);
                Logger.Logger.LogInstance.LogInfo($"Write (framed): {logged}");
            }
            catch (Exception ex)
            {
                Logger.Logger.LogInstance.LogException(ex);
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
                                client.Client.Shutdown(SocketShutdown.Both);
                            }
                            catch (SocketException se)
                            {
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

        // virtuals to be implemented by derived classes
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

    // lightweight DTO for result accumulation
    public class ResultItem
    {
        public string TestCode { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string AbnormalFlags { get; set; }
    }
}
