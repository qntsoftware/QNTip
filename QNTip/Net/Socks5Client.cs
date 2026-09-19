using System.Net;
using System.Net.Sockets;

namespace Qntip.Net;

internal static class Socks5Client
{
    public static async Task<Socket> ConnectAsync(
        string proxyHost, int proxyPort,
        string targetHost, int targetPort,
        CancellationToken ct = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(new DnsEndPoint(proxyHost, proxyPort), ct);

        var stream = new NetworkStream(socket, ownsSocket: false);

        await stream.WriteAsync(new byte[] { 0x05, 0x01, 0x00 }, ct);
        var greet = new byte[2];
        await ReadExactAsync(stream, greet, ct);
        if (greet[0] != 0x05 || greet[1] != 0x00)
            throw new InvalidOperationException("SOCKS5 handshake failed.");

        var hostBytes = System.Text.Encoding.ASCII.GetBytes(targetHost);
        var req = new byte[4 + 1 + hostBytes.Length + 2];
        req[0] = 0x05; req[1] = 0x01; req[2] = 0x00; req[3] = 0x03;
        req[4] = (byte)hostBytes.Length;
        Buffer.BlockCopy(hostBytes, 0, req, 5, hostBytes.Length);
        req[^2] = (byte)(targetPort >> 8);
        req[^1] = (byte)(targetPort & 0xFF);
        await stream.WriteAsync(req, ct);

        var header = new byte[4];
        await ReadExactAsync(stream, header, ct);
        if (header[1] != 0x00)
            throw new InvalidOperationException($"SOCKS5 CONNECT failed: {header[1]}");

        int skip = header[3] switch
        {
            0x01 => 4,
            0x04 => 16,
            0x03 => (await ReadOneAsync(stream, ct)),
            _ => throw new InvalidOperationException("Unknown ATYP")
        };

        var tail = new byte[skip + 2];
        await ReadExactAsync(stream, tail, ct);

        return socket;
    }

    private static async Task<byte> ReadOneAsync(Stream s, CancellationToken ct)
    {
        var b = new byte[1];
        await ReadExactAsync(s, b, ct);
        return b[0];
    }

    private static async Task ReadExactAsync(Stream s, byte[] buf, CancellationToken ct)
    {
        var offset = 0;
        while (offset < buf.Length)
        {
            var read = await s.ReadAsync(buf.AsMemory(offset), ct);
            if (read == 0) throw new EndOfStreamException();
            offset += read;
        }
    }
}