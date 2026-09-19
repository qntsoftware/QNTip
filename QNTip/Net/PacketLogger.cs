using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace Qntip.Net;

internal sealed class PacketLogger
{
    private int _counter;
    private readonly Stopwatch _session = Stopwatch.StartNew();

    public void Separator(string title)
    {
        var width = 78;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  " + new string('─', width));
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"  ║ {title}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  " + new string('─', width));
        Console.ResetColor();
    }

    public async Task LogRequestAsync(HttpRequestMessage req)
    {
        var id = Interlocked.Increment(ref _counter);
        var host = req.RequestUri?.Host ?? "?";
        var port = req.RequestUri?.Port ?? (req.RequestUri?.Scheme == "https" ? 443 : 80);
        var path = req.RequestUri?.PathAndQuery ?? "/";
        var method = req.Method.Method;
        var scheme = req.RequestUri?.Scheme?.ToUpperInvariant() ?? "?";

        long bodySize = 0;
        if (req.Content is not null)
        {
            bodySize = req.Content.Headers.ContentLength ?? -1;
            if (bodySize < 0)
            {
                var bytes = await req.Content.ReadAsByteArrayAsync();
                bodySize = bytes.Length;
            }
        }

        var ts = DateTime.Now.ToString("HH:mm:ss.fff");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [{ts}] ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"#{id:D4} ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"→ {method,-4} ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{scheme}://{host}:{port}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(path);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [giden: {bodySize} B]");
        Console.WriteLine();
        Console.ResetColor();
    }

    public Task LogResponseAsync(HttpResponseMessage resp, TimeSpan elapsed, long receivedBytes)
    {
        var statusColor = (int)resp.StatusCode switch
        {
            >= 200 and < 300 => ConsoleColor.Green,
            >= 300 and < 400 => ConsoleColor.Cyan,
            >= 400 and < 500 => ConsoleColor.Yellow,
            >= 500 => ConsoleColor.Red,
            _ => ConsoleColor.White
        };

        var ts = DateTime.Now.ToString("HH:mm:ss.fff");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [{ts}]      ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("← ");
        Console.ForegroundColor = statusColor;
        Console.Write($"{(int)resp.StatusCode} {resp.StatusCode,-20} ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[gelen: {receivedBytes} B]  ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"{elapsed.TotalMilliseconds:F0} ms");
        Console.WriteLine();
        Console.ResetColor();

        return Task.CompletedTask;
    }

    public void ShowSessionStats(int requestCount)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ── Toplam istek: {requestCount}  |  Oturum süresi: {_session.Elapsed.TotalSeconds:F1} sn ──");
        Console.ResetColor();
    }
}