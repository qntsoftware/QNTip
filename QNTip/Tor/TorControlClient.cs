using System.Net.Sockets;
using System.Text;

namespace Qntip.Tor;

internal sealed class TorControlClient
{
    private readonly string _cookiePath;
    private readonly int _port;

    public TorControlClient(string dataDir, int port = TorManager.ControlPort)
    {
        _cookiePath = Path.Combine(dataDir, "control_auth_cookie");
        _port = port;
    }

    public async Task ChangeIdentityAsync()
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync("127.0.0.1", _port);

        using var stream = tcp.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII);
        using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

        if (File.Exists(_cookiePath))
        {
            var cookie = await File.ReadAllBytesAsync(_cookiePath);
            var hex = Convert.ToHexString(cookie).ToLowerInvariant();
            await writer.WriteLineAsync($"AUTHENTICATE {hex}");
        }
        else
        {
            await writer.WriteLineAsync("AUTHENTICATE");
        }

        var auth = await reader.ReadLineAsync() ?? "";
        if (!auth.StartsWith("250"))
            throw new InvalidOperationException($"Tor auth basarisiz: {auth}");

        await writer.WriteLineAsync("SIGNAL NEWNYM");
        var resp = await reader.ReadLineAsync() ?? "";
        if (!resp.StartsWith("250"))
            throw new InvalidOperationException($"NEWNYM basarisiz: {resp}");

        await writer.WriteLineAsync("QUIT");
        await reader.ReadLineAsync();
    }
}