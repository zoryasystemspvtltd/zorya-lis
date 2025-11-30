using LIS.DtoModel;
using Microsoft.VisualBasic;
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
        protected string sInputMsg = "";
        public string FullMessage { get; private set; }
        protected int index;
        protected string[] output;
        //NetworkStream stream;
        public TCPIPASTMCommand(TCPIPSettings settings)
        {
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method started.");
            this._settings = settings;
            Logger.Logger.LogInstance.LogDebug("LIS.Com.Businesslogic TCPIPASTMCommand Constructor method completed.");
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

                    string data = parserBuffer.ToString();
                    Logger.Logger.LogInstance.LogInfo($"Read: {data}");                
                    await ProcessFrameAsync(data, stream).ConfigureAwait(false);

                    // Keep leftover 
                    parserBuffer.Clear();
                   // parserBuffer.Append(data);
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

        /// ENQ or (char)5 -enquiry
        /// ACK or (char)6 -acknowledge
        /// STX or (char)2- start of text
        /// ETX or (char)3- end of text
        /// ETB or (char)17- end of text
        /// EOT or (char)4 - end of transmission
        /// NAK or (char)21 - negative acknowledge
        /// DLE or (char)10 - data link escape 
        /// CR	or (char)13 carriage return
        private async Task ProcessFrameAsync(string message, NetworkStream stream)
        {
            try
            {
                if (message != string.Empty)
                {
                    var InpBuffer = message.ToCharArray();
                    switch (InpBuffer[0])
                    {
                        case (char)5:        // Check for <ENQ>
                            {
                                await WriteToPort("" + (char)6, stream);
                                break;
                            }

                        case (char)6:      // Check for <ACK>
                            {
                                if (index < 4)
                                {
                                    await WriteToPort((char)2 + Add_CheckSum(output[index + 1]) + (char)13, stream);
                                    index += 1;

                                }
                                else
                                {  //(char)4 means end of transmission
                                    await WriteToPort("" + (char)4, stream);
                                    index = 0;
                                    for (int i = 0; i <= 4; i++)
                                        output[i] = string.Empty;
                                }

                                break;
                            }

                        case (char)4:   // Check For the <EOT>
                            {
                                //Logger.Logger.LogInstance.LogInfo("SerialCommand Read: '{0}'", sInputMsg);
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
                                        await WriteToPort("" + (char)6, stream);
                                    }
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                this.FullMessage = ex.Message;
                sInputMsg = "";
                throw;
            }
        }
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

        protected async Task WriteToPort(string text, NetworkStream stream)
        {
            var dsrBytes = Encoding.ASCII.GetBytes(text);
            await stream.WriteAsync(dsrBytes, 0, dsrBytes.Length).ConfigureAwait(false);
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