using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace Qntip.Net;

internal static class TorHttpClient
{
    public static HttpClient Create(int socksPort, bool forceTls13, PacketLogger? logger = null)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (ctx, ct) =>
            {
                var socket = await Socks5Client.ConnectAsync(
                    "127.0.0.1", socksPort,
                    ctx.DnsEndPoint.Host, ctx.DnsEndPoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            },
            PooledConnectionLifetime = TimeSpan.FromSeconds(30),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(15),
            ConnectTimeout = TimeSpan.FromSeconds(20),
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = forceTls13
                    ? SslProtocols.Tls13
                    : SslProtocols.Tls12 | SslProtocols.Tls13,
            },
        };

        HttpMessageHandler finalHandler = logger is null
            ? handler
            : new LoggingHandler(logger, handler);

        return new HttpClient(finalHandler) { Timeout = TimeSpan.FromSeconds(60) };
    }
}