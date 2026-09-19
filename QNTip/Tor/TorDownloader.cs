using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http;

namespace Qntip.Tor;

internal static class TorDownloader
{
    private static readonly string[] Versions =
    {
        "14.5.1",
        "14.0.10",
        "13.5.10",
    };

    private const string BaseUrl = "https://dist.torproject.org/torbrowser";

    public static string InstallDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QNTip", "tor-bundle");

    public static string ExpectedTorExe => Path.Combine(InstallDir, "tor", "tor.exe");

    public static async Task<string?> EnsureAsync()
    {
        if (File.Exists(ExpectedTorExe))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[i] Onbellekteki Tor kullanilacak: {ExpectedTorExe}");
            Console.ResetColor();
            return ExpectedTorExe;
        }

        Directory.CreateDirectory(InstallDir);

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("QNTip/1.0");

        foreach (var version in Versions)
        {
            var url = $"{BaseUrl}/{version}/tor-expert-bundle-windows-x86_64-{version}.tar.gz";
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[+] Tor indiriliyor: v{version}");
                Console.ResetColor();
                Console.WriteLine($"    URL: {url}");

                var tmpFile = Path.Combine(Path.GetTempPath(), $"qntip_tor_{version}.tar.gz");

                await DownloadWithProgressAsync(http, url, tmpFile);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[+] Arsiv cikariliyor...");
                Console.ResetColor();

                ExtractTarGz(tmpFile, InstallDir);

                try { File.Delete(tmpFile); } catch { }

                if (File.Exists(ExpectedTorExe))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[+] Tor hazir: {ExpectedTorExe}");
                    Console.ResetColor();
                    Console.WriteLine();
                    return ExpectedTorExe;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"[!] v{version} indirildi ama tor.exe bulunamadi, sonraki surum deneniyor...");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[!] v{version} basarisiz: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[!] Hicbir surum indirilemedi. Lutfen internet baglantini kontrol et");
        Console.WriteLine("    veya Tor Browser'i manuel kur.");
        Console.ResetColor();
        return null;
    }

    private static async Task DownloadWithProgressAsync(
        HttpClient http, string url, string destPath)
    {
        using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();

        var total = resp.Content.Headers.ContentLength ?? -1;
        long received = 0;
        var buffer = new byte[81920];

        await using var src = await resp.Content.ReadAsStreamAsync();
        await using var dst = File.Create(destPath);

        int read;
        var lastReport = DateTime.UtcNow;
        while ((read = await src.ReadAsync(buffer)) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, read));
            received += read;

            if ((DateTime.UtcNow - lastReport).TotalMilliseconds > 250)
            {
                lastReport = DateTime.UtcNow;
                PrintProgress(received, total);
            }
        }
        PrintProgress(received, total);
        Console.WriteLine();
    }

    private static void PrintProgress(long received, long total)
    {
        if (total > 0)
        {
            var pct = (double)received / total * 100;
            Console.Write($"\r    [{pct,5:F1}%] {FormatBytes(received)} / {FormatBytes(total)}   ");
        }
        else
        {
            Console.Write($"\r    {FormatBytes(received)} indirildi...   ");
        }
    }

    private static string FormatBytes(long b) =>
        b < 1024 ? $"{b} B" :
        b < 1024 * 1024 ? $"{b / 1024.0:F1} KB" :
        b < 1024L * 1024 * 1024 ? $"{b / 1024.0 / 1024.0:F1} MB" :
        $"{b / 1024.0 / 1024.0 / 1024.0:F2} GB";

    private static void ExtractTarGz(string tarGzPath, string destDir)
    {
        using var fileStream = File.OpenRead(tarGzPath);
        using var gzStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var tarReader = new TarReader(gzStream);

        TarEntry? entry;
        while ((entry = tarReader.GetNextEntry()) != null)
        {
            var name = entry.Name.Replace('/', Path.DirectorySeparatorChar);

            var fullPath = Path.GetFullPath(Path.Combine(destDir, name));
            var destRoot = Path.GetFullPath(destDir);
            if (!fullPath.StartsWith(destRoot, StringComparison.OrdinalIgnoreCase))
                continue;

            if (entry.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(fullPath);
                continue;
            }

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            entry.ExtractToFile(fullPath, overwrite: true);
        }
    }
}