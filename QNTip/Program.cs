using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Qntip.Crypto;
using Qntip.Localization;
using Qntip.Modes;
using Qntip.Net;
using Qntip.Tor;
using Qntip.UI;

namespace Qntip;

internal static class Program
{
    private const int ProxyPort = 8080;
    private static readonly PacketLogger Logger = new();

    private delegate bool ConsoleCtrlHandler(uint ctrlType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler? handler, bool add);

    private static ConsoleCtrlHandler? _ctrlHandler;

    private const uint CTRL_C_EVENT = 0;
    private const uint CTRL_BREAK_EVENT = 1;
    private const uint CTRL_CLOSE_EVENT = 2;
    private const uint CTRL_LOGOFF_EVENT = 5;
    private const uint CTRL_SHUTDOWN_EVENT = 6;

    private static async Task<int> Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            SafePrintError(e.ExceptionObject as Exception);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            SafePrintError(e.Exception);
            e.SetObserved();
        };

        try
        {
            return await RunAsync();
        }
        catch (Exception ex)
        {
            SafePrintError(ex);
            return 1;
        }
    }

    private static async Task<int> RunAsync()
    {
        Console.Clear();

        Console.WriteLine();
        Console.WriteLine();
        Buttons.Draw();
        Banner.Show();

        if (!IsAdministrator())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] Bu program yonetici olarak calistirilmalidir.");
            Console.WriteLine("    Cikip sag tik -> 'Yonetici olarak calistir' deneyin.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Kapatmak icin bir tusa basin...");
            Console.ReadKey();
            return 1;
        }

        var torPath = await TorManager.EnsureTorAsync();
        if (torPath is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] Tor hicbir sekilde bulunamadi ve indirilemedi.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Kapatmak icin bir tusa basin...");
            Console.ReadKey();
            return 1;
        }

        using var torManager = new TorManager(torPath);
        try
        {
            await torManager.StartAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] Tor baslatilamadi: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Kapatmak icin bir tusa basin...");
            Console.ReadKey();
            return 1;
        }

        var control = new TorControlClient(torManager.DataDir);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Islem modunu secin / Select operation mode:");
        Console.WriteLine("  Şifreli paket modu internet akışınızı yavaşlatacaktır. ama anonimlik konusunda kesinlikle tercih edilmeli");
        Console.WriteLine("   [1] Sifreli Paket Modu  (Proxy + TLS 1.3 + Tor)");
        Console.WriteLine("   [2] Sifresiz Paket Modu (Proxy + Tor)");
        Console.ResetColor();
        Console.Write("  Secim / Choice [1/2]: ");
        var modeInput = Console.ReadLine()?.Trim();
        var mode = modeInput == "1" ? OperationMode.Encrypted : OperationMode.Plain;

        Console.Write("  IP degisim araligi / interval (sn/sec) [60]: ");
        var intervalInput = Console.ReadLine();
        var interval = int.TryParse(intervalInput, out var i) && i > 0 ? i : 60;

        Console.Write("  Dongu sayisi / cycles (0 = sonsuz/infinite): ");
        var cyclesInput = Console.ReadLine();
        var cycles = int.TryParse(cyclesInput, out var c) && c >= 0 ? c : 0;

        var aesKey = mode == OperationMode.Encrypted ? AesGcmCrypto.GenerateKey() : null;

        using var http = TorHttpClient.Create(
            TorManager.SocksPort,
            forceTls13: mode == OperationMode.Encrypted,
            logger: Logger);

        var proxy = new HttpProxyServer(
            ProxyPort,
            TorManager.SocksPort,
            encryptedMode: mode == OperationMode.Encrypted);

        try
        {
            await proxy.StartAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[!] Proxy baslatilamadi (port {ProxyPort} dolu olabilir): {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Kapatmak icin bir tusa basin...");
            Console.ReadKey();
            return 1;
        }

        InstallCloseHandler(proxy, torManager);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        _ = Task.Run(() => KeyboardListenerAsync(cts));
        _ = Task.Run(() => HealthMonitorAsync(torManager, cts.Token));

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  " + Loc.T("ctrl_c"));
        Console.ResetColor();
        Console.WriteLine();

        var loop = 0;
        try
        {
            Logger.Separator(Loc.T("starting"));
            await FetchAndShowAsync(http, mode, aesKey, cts.Token);

            while (!cts.IsCancellationRequested)
            {
                Buttons.Draw();

                loop++;
                if (cycles > 0 && loop > cycles) break;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  [{loop}] {string.Format(Loc.T("next_ip_in"), interval)}: ");
                Console.ResetColor();

                for (var s = interval; s > 0 && !cts.IsCancellationRequested; s--)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"{s,3} ");
                    Console.ResetColor();
                    try { await Task.Delay(1000, cts.Token); }
                    catch (OperationCanceledException) { break; }
                }
                Console.WriteLine();

                if (cts.IsCancellationRequested) break;

                Logger.Separator(string.Format(Loc.T("loop_sep"), loop));

                await ChangeIdentityWithRetryAsync(control, cts.Token);

                await Task.Delay(2000, cts.Token);
                await FetchAndShowAsync(http, mode, aesKey, cts.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            Console.WriteLine();
            Logger.ShowSessionStats(loop);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + Loc.T("shutting"));
            Console.ResetColor();

            try { await proxy.StopAsync(); } catch { }
            try { await torManager.StopAsync(); } catch { }

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("  [DEBUG] Devam icin bir tusa basin...");
            Console.ReadKey();
#endif
        }

        return 0;
    }

    [SupportedOSPlatform("windows")]
    private static void InstallCloseHandler(HttpProxyServer proxy, TorManager torManager)
    {
        _ctrlHandler = ctrlType =>
        {
            if (ctrlType == CTRL_C_EVENT || ctrlType == CTRL_BREAK_EVENT)
                return false;

            try
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║  [!] QNTip KAPATILIYOR                               ║");
                Console.WriteLine("  ║      Tor servisi ve proxy durduruluyor...            ║");
                Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
                Console.ResetColor();
            }
            catch { }

            try
            {
                proxy.StopAsync().Wait(TimeSpan.FromSeconds(2));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] Proxy durduruldu.");
                Console.ResetColor();
            }
            catch { }

            try
            {
                torManager.StopAsync().Wait(TimeSpan.FromSeconds(3));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] Tor durduruldu. IP'ler temizlendi.");
                Console.ResetColor();
            }
            catch { }

            try
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  [+] Guvenli cikis tamamlandi. Gule gule!");
                Console.ResetColor();
                Console.WriteLine();
                Thread.Sleep(500);
            }
            catch { }

            Environment.Exit(0);
            return true;
        };

        SetConsoleCtrlHandler(_ctrlHandler, true);
    }

    private static async Task KeyboardListenerAsync(CancellationTokenSource cts)
    {
        while (!cts.IsCancellationRequested)
        {
            try
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    var ch = char.ToUpperInvariant(key.KeyChar);

                    if (ch == 'S')
                    {
                        cts.Cancel();
                        return;
                    }

                    if (Loc.Lang == "TR" && ch == 'E')
                    {
                        Loc.SetLang("ENG");
                        Buttons.Draw();
                    }
                    else if (Loc.Lang == "ENG" && ch == 'T')
                    {
                        Loc.SetLang("TR");
                        Buttons.Draw();
                    }
                }

                await Task.Delay(80, cts.Token);
            }
            catch (OperationCanceledException) { return; }
            catch { }
        }
    }

    private static async Task HealthMonitorAsync(TorManager tor, CancellationToken ct)
    {
        int consecutiveFails = 0;
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
            catch { return; }

            try
            {
                if (await tor.IsAliveAsync())
                {
                    if (consecutiveFails > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n  [i] Tor yeniden saglikli.");
                        Console.ResetColor();
                    }
                    consecutiveFails = 0;
                    continue;
                }

                consecutiveFails++;
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\n  [!] Tor yanit vermiyor ({consecutiveFails}/2)...");
                Console.ResetColor();

                if (consecutiveFails >= 2)
                {
                    try
                    {
                        await tor.RestartAsync();
                        consecutiveFails = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  [!] Tor restart basarisiz: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Health check: {ex.Message}");
            }
        }
    }

    private static async Task ChangeIdentityWithRetryAsync(
        TorControlClient control, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await control.ChangeIdentityAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [+] " + Loc.T("circuit_ok"));
                Console.ResetColor();
                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"  [!] {Loc.T("newnym_fail")} (deneme {attempt}/3): {ex.Message}");
                Console.ResetColor();

                if (attempt < 3)
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  [!] NEWNYM 3 kez basarisiz oldu.");
        Console.ResetColor();
    }

    private static async Task FetchAndShowAsync(
        HttpClient http, OperationMode mode, byte[]? aesKey, CancellationToken ct)
    {
        try
        {
            var info = await IpChecker.FetchAsync(http, ct);

            if (info is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [!] {Loc.T("err_fetch")}");
                Console.ResetColor();
                return;
            }

            PrintIpBox(info);

            if (mode == OperationMode.Encrypted && aesKey is not null)
            {
                var payload = Encoding.UTF8.GetBytes($"{info.Ip}|{info.Country}|{info.Region}|{info.City}");
                var encrypted = AesGcmCrypto.Encrypt(payload, aesKey);
                var hash = Convert.ToHexString(SHA256.HashData(encrypted))[..16];

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"  [*] AES-256-GCM: {encrypted.Length} B  |  SHA-256: {hash}...");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [!] {Loc.T("err_generic")}: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void PrintIpBox(IpInfo info)
    {
        const int width = 66;
        var border = new string('═', width);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ╔" + border + "╗");
        Console.ForegroundColor = ConsoleColor.Green;

        var title = "🌍  " + Loc.T("ip_title");
        var titleLen = title.Length;
        var pad = (width - titleLen) / 2;
        if (pad < 0) pad = 0;
        Console.WriteLine("  ║" + new string(' ', pad) + title + new string(' ', Math.Max(0, width - titleLen - pad)) + "║");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ╠" + border + "╣");

        PrintBoxLine($"  {Loc.T("ip_label"),-10}: {info.Ip,-50}", width, ConsoleColor.Yellow);
        PrintBoxLine($"  {Loc.T("country"),-10}: {info.Country,-50}", width, ConsoleColor.White);
        PrintBoxLine($"  {Loc.T("region"),-10}: {info.Region,-50}", width, ConsoleColor.White);
        PrintBoxLine($"  {Loc.T("city"),-10}: {info.City,-50}", width, ConsoleColor.White);
        PrintBoxLine($"  {Loc.T("isp"),-10}: {info.Isp,-50}", width, ConsoleColor.White);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ╚" + border + "╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void PrintBoxLine(string text, int width, ConsoleColor color)
    {
        if (text.Length > width) text = text[..width];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  ║");
        Console.ForegroundColor = color;
        Console.Write(text.PadRight(width));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("║");
        Console.ResetColor();
    }

    [SupportedOSPlatform("windows")]
    private static bool IsAdministrator()
    {
        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch { return true; }
    }

    private static void SafePrintError(Exception? ex)
    {
        try
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("  [!] BEKLENMEYEN HATA");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            if (ex is not null)
            {
                Console.WriteLine($"  Tip   : {ex.GetType().Name}");
                Console.WriteLine($"  Mesaj : {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("  Stack:");
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Kapatmak icin bir tusa basin...");
            Console.ReadKey();
        } 
        catch { }
    }
}