using System.Diagnostics;
using System.Net.Sockets;

namespace Qntip.Tor;

internal sealed class TorManager : IDisposable
{
    public const int SocksPort = 9050;
    public const int ControlPort = 9051;

    private readonly string _torPath;
    private readonly string _dataDir;
    private readonly string _torrcPath;
    private Process? _process;
    private bool _weStarted;

    public string DataDir => _dataDir;

    public TorManager(string torPath)
    {
        _torPath = torPath;
        _dataDir = Path.Combine(Path.GetTempPath(), "QNTip", "tordata");
        _torrcPath = Path.Combine(Path.GetTempPath(), "QNTip", "torrc");
    }

    public static async Task<string?> EnsureTorAsync()
    {
        var found = FindTor();
        if (found is not null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[i] Tor bulundu: {found}");
            Console.ResetColor();
            return found;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[!] tor.exe bulunamadi. Otomatik indirme baslatiliyor...");
        Console.ResetColor();
        Console.WriteLine();

        return await TorDownloader.EnsureAsync();
    }

    public static string? FindTor()
    {
        var env = Environment.GetEnvironmentVariable("TOR_EXE");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env)) return env;

        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            try
            {
                var p = Path.Combine(dir, "tor.exe");
                if (File.Exists(p)) return p;
            }
            catch { }
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var downloads = Path.Combine(userProfile, "Downloads");
        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var progFilesX = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        var candidates = new[]
        {
            Path.Combine(desktop,   @"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(desktop,   @"tor browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(desktop,   @"Tor\tor.exe"),

            Path.Combine(downloads, @"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(downloads, @"tor browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(downloads, @"Tor\tor.exe"),

            Path.Combine(localApp,  @"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(appData,   @"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),

            Path.Combine(progFiles, @"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(progFilesX,@"Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(progFiles, @"Tor\tor.exe"),
            Path.Combine(progFilesX,@"Tor\tor.exe"),

            Path.Combine(userProfile, @"OneDrive\Desktop\Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
            Path.Combine(userProfile, @"OneDrive\Masaüstü\Tor Browser\Browser\TorBrowser\Tor\tor.exe"),
        };

        foreach (var c in candidates)
        {
            if (File.Exists(c)) return c;
        }

        try
        {
            var searchRoots = new[] { desktop, downloads };
            foreach (var root in searchRoots)
            {
                if (!Directory.Exists(root)) continue;
                var found = Directory.GetFiles(root, "tor.exe", SearchOption.AllDirectories);
                if (found.Length > 0) return found[0];
            }
        }
        catch { }

        return null;
    }

    public async Task StartAsync()
    {

        if (_process is not null && _process.HasExited)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[i] Eski Tor process sonlanmis, yeniden baslatilacak.");
            Console.ResetColor();
            _process = null;
            _weStarted = false;
        }

        if (await IsPortListening(SocksPort))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[i] Zaten calisan Tor tespit edildi (9050). Ona baglaniliyor.");
            Console.ResetColor();
            _weStarted = false;
            return;
        }

        Directory.CreateDirectory(_dataDir);
        await File.WriteAllTextAsync(_torrcPath, $"""
            SocksPort {SocksPort} NoIsolateDestAddr NoIsolateSOCKSAUTH NoIsolateDestPort
            ControlPort {ControlPort}
            DataDirectory {_dataDir.Replace("\\", "/")}
            CookieAuthentication 1
            MaxCircuitDirtiness 10
            NewCircuitPeriod 30
            KeepalivePeriod 60
            """);

        var psi = new ProcessStartInfo
        {
            FileName = _torPath,
            Arguments = $"-f \"{_torrcPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        Console.WriteLine("[+] Tor baslatiliyor...");
        _process = Process.Start(psi) ?? throw new InvalidOperationException("tor.exe baslatilamadi.");
        _weStarted = true;

        _ = Task.Run(() => DrainOutput(_process));

        if (!await WaitForPort(SocksPort, TimeSpan.FromSeconds(60)))
            throw new TimeoutException("Tor SOCKS portu acilmadi.");
        if (!await WaitForPort(ControlPort, TimeSpan.FromSeconds(15)))
            throw new TimeoutException("Tor Control portu acilmadi.");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[+] Tor hazir.");
        Console.ResetColor();
    }

    private static async Task DrainOutput(Process p)
    {
        try
        {
            var reader = p.StandardError;
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line is null) break;
                Debug.WriteLine($"[tor] {line}");
            }
        }
        catch { }
    }

    private static async Task<bool> IsPortListening(int port)
    {
        try
        {
            using var c = new TcpClient();
            await c.ConnectAsync("127.0.0.1", port);
            return true;
        }
        catch { return false; }
    }

    private static async Task<bool> WaitForPort(int port, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (await IsPortListening(port)) return true;
            await Task.Delay(300);
        }
        return false;
    }

    public async Task<bool> IsAliveAsync()
    {
        if (_weStarted && (_process is null || _process.HasExited))
            return false;

        return await IsPortListening(SocksPort);
    }

    public async Task RestartAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[!] Tor yeniden baslatiliyor...");
        Console.ResetColor();

        await StopAsync();
        await Task.Delay(2000);

        _process = null;
        _weStarted = false;

        await StartAsync();
    }

    public async Task StopAsync()
    {
        if (_process is not null && !_process.HasExited)
        {
            try
            {
                _process.Kill(entireProcessTree: true);
                await Task.Run(() => _process.WaitForExit(5000));
            }
            catch { }
        }

        for (int i = 0; i < 20; i++)
        {
            if (!await IsPortListening(SocksPort)) break;
            await Task.Delay(250);
        }

        _weStarted = false;
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();
}