using System.Net.Http;

namespace Qntip.Net;

internal sealed class IpInfo
{
    public string Ip { get; set; } = "?";
    public string Country { get; set; } = "?";
    public string Region { get; set; } = "?";
    public string City { get; set; } = "?";
    public string Isp { get; set; } = "?";
}

internal static class IpChecker
{
    public static async Task<IpInfo?> FetchAsync(
        HttpClient http, CancellationToken ct, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            var info = await TryFetchOnceAsync(http, ct);
            if (info is not null) return info;

            if (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                try { await Task.Delay(delay, ct); }
                catch (OperationCanceledException) { return null; }
            }
        }
        return null;
    }

    private static async Task<IpInfo?> TryFetchOnceAsync(HttpClient http, CancellationToken ct)
    {
        // 1) ip-api.com (HTTP, Tor altinda)
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            var url = "http://ip-api.com/json/?fields=status,country,regionName,city,isp,org,query";
            var json = await http.GetStringAsync(url, timeoutCts.Token);

            if (json.Contains("\"status\":\"success\""))
            {
                return new IpInfo
                {
                    Ip = GetStr(json, "query") ?? "?",
                    Country = GetStr(json, "country") ?? "?",
                    Region = GetStr(json, "regionName") ?? "?",
                    City = GetStr(json, "city") ?? "?",
                    Isp = GetStr(json, "isp") ?? GetStr(json, "org") ?? "?",
                };
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch { }

        // 2) ipinfo.io (HTTPS fallback)
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            var json = await http.GetStringAsync("https://ipinfo.io/json", timeoutCts.Token);
            var ip = GetStr(json, "ip");
            if (!string.IsNullOrWhiteSpace(ip))
            {
                return new IpInfo
                {
                    Ip = ip,
                    Country = GetStr(json, "country") ?? "?",
                    Region = GetStr(json, "region") ?? "?",
                    City = GetStr(json, "city") ?? "?",
                    Isp = GetStr(json, "org") ?? "?",
                };
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
        catch { }

        return null;
    }

    private static string? GetStr(string json, string key)
    {
        var pattern = $"\"{key}\"";
        var idx = json.IndexOf(pattern, StringComparison.Ordinal);
        if (idx < 0) return null;

        var colon = json.IndexOf(':', idx);
        if (colon < 0) return null;

        var start = json.IndexOf('"', colon + 1);
        if (start < 0) return null;

        var end = json.IndexOf('"', start + 1);
        if (end < 0) return null;

        return json.Substring(start + 1, end - start - 1);
    }
}