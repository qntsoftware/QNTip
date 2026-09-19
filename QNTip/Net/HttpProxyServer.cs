using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Qntip.Net;

internal sealed class HttpProxyServer : IDisposable
{
    private readonly int _listenPort;
    private readonly int _torSocksPort;
    private readonly bool _encryptedMode;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private long _bytesIn;
    private long _bytesOut;
    private int _activeConns;
    private int _totalRequests;

    public HttpProxyServer(int listenPort, int torSocksPort, bool encryptedMode)
    {
        _listenPort = listenPort;
        _torSocksPort = torSocksPort;
        _encryptedMode = encryptedMode;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _listener = new TcpListener(IPAddress.Loopback, _listenPort);
        _listener.Start();

        PrintInfo();

        _ = Task.Run(() => StatsLoopAsync(_cts.Token));
        _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    private void PrintInfo()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("  ║  🚀  PROXY AKTIF                                          ║");
        Console.WriteLine("  ╠══════════════════════════════════════════════════════════╣");
        Console.WriteLine($"  ║  Dinleniyor    : 127.0.0.1:{_listenPort,-34}║");
        Console.WriteLine($"  ║  Yonlendirme   : Tor SOCKS5 127.0.0.1:{_torSocksPort,-19}║");
        Console.WriteLine($"  ║  Mod           : {(_encryptedMode ? "SIFRELI (TLS+Tor)" : "SIFRESIZ (Tor)"),-37}║");
        Console.WriteLine("  ╠══════════════════════════════════════════════════════════╣");
        Console.WriteLine("  ║  Firefox proxy ayarini şöyle yap:                        ║");
        Console.WriteLine("  ║    SOCKS5  127.0.0.1:8080                                ║");
        Console.WriteLine("  ║    veya HTTP  127.0.0.1:8080                             ║");
        Console.WriteLine("  ╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(ct);
                _ = Task.Run(() => HandleClientAsync(client, ct), ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [!] Accept hatasi: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        Interlocked.Increment(ref _activeConns);
        Interlocked.Increment(ref _totalRequests);
        var clientId = Guid.NewGuid().ToString("N")[..6];

        try
        {
            client.NoDelay = true;
            using var clientStream = client.GetStream();

            // Ilk satir icin 10 sn timeout
            using var readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            readCts.CancelAfter(TimeSpan.FromSeconds(10));

            var firstLine = await ReadLineAsync(clientStream, readCts.Token);
            if (string.IsNullOrWhiteSpace(firstLine)) return;

            var parts = firstLine.Split(' ');
            if (parts.Length < 3) return;

            var method = parts[0].ToUpperInvariant();
            var target = parts[1];

            string host; int port;

            if (method == "CONNECT")
            {
                var hp = target.Split(':');
                host = hp[0];
                port = hp.Length > 1 ? int.Parse(hp[1]) : 443;
            }
            else
            {
                if (!Uri.TryCreate(target, UriKind.Absolute, out var uri))
                {
                    await WriteAsync(clientStream, "HTTP/1.1 400 Bad Request\r\n\r\n", ct);
                    return;
                }
                host = uri.Host;
                port = uri.Port;
            }

            var headers = new StringBuilder();
            while (true)
            {
                var line = await ReadLineAsync(clientStream, ct);
                if (string.IsNullOrEmpty(line)) break;
                headers.AppendLine(line);
            }

            LogRequest(clientId, method, host, port);

            var torSocket = await Socks5Client.ConnectAsync(
                "127.0.0.1", _torSocksPort, host, port, ct);
            var torStream = new NetworkStream(torSocket, ownsSocket: true);

            if (method == "CONNECT")
            {
                await WriteAsync(clientStream, "HTTP/1.1 200 Connection Established\r\n\r\n", ct);
                await PumpBidirectionalAsync(clientStream, torStream, ct);
            }
            else
            {
                var cleaned = CleanHeaders(headers.ToString());
                var rebuilt = $"{method} {target} HTTP/1.1\r\n{cleaned}\r\n";
                var bytes = Encoding.ASCII.GetBytes(rebuilt);

                await torStream.WriteAsync(bytes, ct);
                Interlocked.Add(ref _bytesOut, bytes.Length);

                await PumpBidirectionalAsync(clientStream, torStream, ct);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  [!] [{clientId}] Hata: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            try { client.Close(); } catch { }
            Interlocked.Decrement(ref _activeConns);
        }
    }

    private string CleanHeaders(string headers)
    {
        var drop = new[] { "Proxy-Connection", "Proxy-Authorization" };
        var lines = headers.Split("\r\n");
        var kept = lines.Where(l =>
            !string.IsNullOrWhiteSpace(l) &&
            !drop.Any(d => l.StartsWith(d, StringComparison.OrdinalIgnoreCase)));
        return string.Join("\r\n", kept);
    }

    private async Task PumpBidirectionalAsync(
        NetworkStream a, NetworkStream b, CancellationToken ct)
    {
        var up = PumpAsync(a, b, true, ct);
        var down = PumpAsync(b, a, false, ct);
        await Task.WhenAny(up, down);
    }

    private async Task PumpAsync(
        NetworkStream src, NetworkStream dst, bool isOut, CancellationToken ct)
    {
        var buffer = new byte[8192];
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Idle timeout - 5 dk veri gelmezse kapat
                using var idleCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                idleCts.CancelAfter(TimeSpan.FromMinutes(5));

                var n = await src.ReadAsync(buffer, idleCts.Token);
                if (n == 0) break;
                await dst.WriteAsync(buffer.AsMemory(0, n), ct);
                await dst.FlushAsync(ct);

                if (isOut) Interlocked.Add(ref _bytesOut, n);
                else Interlocked.Add(ref _bytesIn, n);
            }
        }
        catch (OperationCanceledException) { }
        catch { }
    }

    private static async Task<string?> ReadLineAsync(NetworkStream stream, CancellationToken ct)
    {
        var sb = new StringBuilder();
        var one = new byte[1];
        while (true)
        {
            var n = await stream.ReadAsync(one, ct);
            if (n == 0) return sb.Length > 0 ? sb.ToString() : null;
            var c = (char)one[0];
            if (c == '\n') return sb.ToString().TrimEnd('\r');
            if (c == '\r') continue;
            sb.Append(c);
            if (sb.Length > 8192) break;
        }
        return sb.ToString();
    }

    private static async Task WriteAsync(NetworkStream s, string text, CancellationToken ct)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        await s.WriteAsync(bytes, ct);
        await s.FlushAsync(ct);
    }

    private void LogRequest(string id, string method, string host, int port)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [{ts}] ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"#{id} ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"→ {method,-7} ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{host}:{port}");
        Console.WriteLine();
        Console.ResetColor();
    }

    private async Task StatsLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(30_000, ct); } catch { break; }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(
                $"  [i] Aktif: {_activeConns}  |  Toplam istek: {_totalRequests}  |  Yukari: {FormatBytes(_bytesOut)}  |  Asagi: {FormatBytes(_bytesIn)}");
            Console.ResetColor();
        }
    }

    private static string FormatBytes(long b) =>
        b < 1024 ? $"{b} B" :
        b < 1024 * 1024 ? $"{b / 1024.0:F1} KB" :
        $"{b / 1024.0 / 1024.0:F1} MB";

    public Task StopAsync()
    {
        try
        {
            _cts?.Cancel();
            _listener?.Stop();
        }
        catch { }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
    }
}