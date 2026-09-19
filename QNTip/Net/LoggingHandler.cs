using System.Diagnostics;
using System.Net.Http;

namespace Qntip.Net;

internal sealed class LoggingHandler : DelegatingHandler
{
    private readonly PacketLogger _logger;

    public LoggingHandler(PacketLogger logger, HttpMessageHandler inner)
        : base(inner)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        await _logger.LogRequestAsync(request);

        var sw = Stopwatch.StartNew();
        var response = await base.SendAsync(request, ct);

        long size = response.Content.Headers.ContentLength ?? -1;
        if (size < 0)
        {
            try
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                size = bytes.Length;
                response.Content = new ByteArrayContent(bytes);
            }
            catch { size = 0; }
        }

        sw.Stop();
        await _logger.LogResponseAsync(response, sw.Elapsed, size);
        return response;
    }
}