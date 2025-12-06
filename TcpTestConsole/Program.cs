using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimulateCL_1200i
{
    
    /// <summary>
    /// Multi-analyzer simulator (Mindray CL-1200i + Sysmex XN350) for .NET Core 3.1.
    /// Usage:
    /// dotnet run -- [host] [mindrayPort] [sysmexPort] [mindrayIntervalSec] [sysmexIntervalSec] [startMsgId]
    /// Example:
    /// dotnet run -- 127.0.0.1 5000 5001 5 7 100
    /// </summary>
    class Program
    {
        // Framing chars
        const byte VT = 0x0B;  // <VT> (11)
        const byte FS = 0x1C;  // <FS> (28)
        const byte CR = 0x0D;  // <CR> (13)

        static async Task<int> Main(string[] args)
        {
            string host = args.Length > 0 ? args[0] : "127.0.0.1";
            int mindrayPort = args.Length > 1 && int.TryParse(args[1], out var p1) ? p1 : 5000;
            int sysmexPort = args.Length > 2 && int.TryParse(args[2], out var p2) ? p2 : 5001;
            int mindrayInterval = args.Length > 3 && int.TryParse(args[3], out var i1) ? i1 : 5;
            int sysmexInterval = args.Length > 4 && int.TryParse(args[4], out var i2) ? i2 : 7;
            int startMsgId = args.Length > 5 && int.TryParse(args[5], out var s) ? s : 1;

            Console.WriteLine("Multi-Analyzer Simulator (Mindray + Sysmex)");
            Console.WriteLine($"Host: {host}, MindrayPort: {mindrayPort}, SysmexPort: {sysmexPort}");
            Console.WriteLine($"Intervals: Mindray {mindrayInterval}s, Sysmex {sysmexInterval}s. StartMsgId: {startMsgId}");
            Console.WriteLine("Ctrl+C to stop.");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Stopping...");
                e.Cancel = true;
                cts.Cancel();
            };

            // Start both simulators concurrently
            var mindrayTask = RunAnalyzerSimulator("Mindray-CL1200", host, mindrayPort, mindrayInterval, startMsgId, AnalyzerType.Mindray, cts.Token);
            var sysmexTask = RunAnalyzerSimulator("Sysmex-XN350", host, sysmexPort, sysmexInterval, startMsgId + 1000, AnalyzerType.Sysmex, cts.Token);

            await Task.WhenAll(mindrayTask, sysmexTask);
            Console.WriteLine("Simulators stopped.");
            return 0;
        }

        enum AnalyzerType { Mindray, Sysmex }

        static async Task RunAnalyzerSimulator(string name, string host, int port, int intervalSec, int startMsgId, AnalyzerType type, CancellationToken ct)
        {
            int msgId = startMsgId;

            while (!ct.IsCancellationRequested)
            {
                TcpClient client = null;
                try
                {
                    client = new TcpClient();
                    Console.WriteLine($"[{name}] Connecting to {host}:{port}...");
                    // .NET Core 3.1: no ConnectAsync overload with token. Use registration that closes socket on cancellation.
                    using (ct.Register(() => { try { client?.Close(); } catch { } }))
                    {
                        await client.ConnectAsync(host, port);
                    }
                    Console.WriteLine($"[{name}] Connected.");

                    using var stream = client.GetStream();

                    // Start reader loop
                    var readerTask = Task.Run(() => ReaderLoopAsync(name, stream, ct), ct);

                    // Send periodic QRD (or other) messages
                    while (!ct.IsCancellationRequested && client.Connected)
                    {
                        string sampleId = $"{(type == AnalyzerType.Mindray ? "M" : "S")}{msgId}";
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                        string message = type == AnalyzerType.Mindray
                            ? BuildMindrayQrd(sampleId, timestamp, msgId)
                            : BuildSysmexQrd(sampleId, timestamp, msgId);

                        byte[] framed = FrameMessage(message);

                        Console.WriteLine($"[{name}] --> Sending QRD (msgId={msgId}, sample={sampleId})");
                        await stream.WriteAsync(framed, 0, framed.Length, ct).ConfigureAwait(false);
                        await stream.FlushAsync(ct).ConfigureAwait(false);

                        // Wait for the configured interval (or cancellation)
                        try { await Task.Delay(TimeSpan.FromSeconds(intervalSec), ct).ConfigureAwait(false); } catch (TaskCanceledException) { break; }

                        msgId++;
                    }

                    // Wait for reader to finish shortly then close
                    try { await Task.WhenAny(readerTask, Task.Delay(500, ct)).ConfigureAwait(false); } catch { }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{name}] ERROR: {ex.GetType().Name} - {ex.Message}");
                }
                finally
                {
                    try { client?.Close(); } catch { }
                }

                // On disconnect, backoff and then retry
                if (!ct.IsCancellationRequested)
                {
                    try { await Task.Delay(2000, ct).ConfigureAwait(false); } catch { break; }
                    Console.WriteLine($"[{name}] Reconnecting after backoff...");
                }
            }

            Console.WriteLine($"[{name}] Simulator stopping.");
        }

        // Build Mindray-like QRD (similar to logs you supplied)
        static string BuildMindrayQrd(string sampleId, string timestampUtc, int msgId)
        {
            var sb = new StringBuilder();
            sb.Append("MSH|^~\\&|||||");
            sb.Append(timestampUtc);
            sb.Append("||QRY^Q02|");
            sb.Append(msgId);
            sb.Append("|P|2.3.1||||||ASCII|||");
            sb.Append('\r');
            sb.Append("QRD|");
            sb.Append(timestampUtc);
            sb.Append("|R|D|22|||RD|");
            sb.Append(sampleId);
            sb.Append("|OTH|||T|");
            sb.Append('\r');
            sb.Append("QRF||||||RCT|COR|ALL||");
            sb.Append('\r');
            return sb.ToString();
        }

        // Build Sysmex-like QRD (slightly different fields for variety)
        static string BuildSysmexQrd(string sampleId, string timestampUtc, int msgId)
        {
            var sb = new StringBuilder();
            sb.Append("MSH|^~\\&|||||");
            sb.Append(timestampUtc);
            sb.Append("||QRY^Q02|");
            sb.Append(msgId);
            sb.Append("|P|2.3.1||||||ASCII|||");
            sb.Append('\r');
            sb.Append("QRD|");
            sb.Append(timestampUtc);
            sb.Append("|R|D|10|||RD|");
            sb.Append(sampleId);
            sb.Append("|OTH|||T|");
            sb.Append('\r');
            sb.Append("QRF||||||RCT|COR|ALL||");
            sb.Append('\r');
            return sb.ToString();
        }

        static byte[] FrameMessage(string hl7)
        {
            if (hl7 == null) hl7 = string.Empty;
            var payload = Encoding.ASCII.GetBytes(hl7);
            var framed = new byte[payload.Length + 3];
            framed[0] = VT;
            Buffer.BlockCopy(payload, 0, framed, 1, payload.Length);
            framed[framed.Length - 2] = FS;
            framed[framed.Length - 1] = CR;
            return framed;
        }

        static async Task ReaderLoopAsync(string name, NetworkStream stream, CancellationToken ct)
        {
            var readBuffer = new byte[8192];
            var sb = new StringBuilder();
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    int read = 0;
                    try
                    {
                        read = await stream.ReadAsync(readBuffer, 0, readBuffer.Length, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ObjectDisposedException) { break; }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{name} Reader] Exception: {ex.Message}");
                        break;
                    }

                    if (read == 0)
                    {
                        Console.WriteLine($"[{name} Reader] Connection closed by remote.");
                        break;
                    }

                    var chunk = Encoding.ASCII.GetString(readBuffer, 0, read);
                    sb.Append(chunk);

                    string data = sb.ToString();
                    int fs;
                    while ((fs = data.IndexOf((char)FS)) >= 0)
                    {
                        var frameWithVT = data.Substring(0, fs); // includes leading VT often
                        data = data.Substring(fs + 1);
                        var cleaned = frameWithVT.TrimStart((char)VT).TrimEnd((char)CR, '\n');
                        Console.WriteLine();
                        Console.WriteLine($"[{name}] <-- Received framed response:");
                        Console.WriteLine("-------------------------------------------------");
                        Console.WriteLine(cleaned.Replace("\r", Environment.NewLine));
                        Console.WriteLine("-------------------------------------------------");
                    }

                    sb.Clear();
                    sb.Append(data);
                }
            }
            finally
            {
                // no-op
            }
        }
    }


}
