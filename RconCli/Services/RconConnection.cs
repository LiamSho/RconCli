using System.Net;
using CoreRCON;

namespace RconCli.Services;

public class RconConnection : IDisposable
{
    private readonly RCON _rcon;

    private bool _disposed;
    private bool _isConnected;

    public RconConnection(IPAddress ip, ushort port, string password)
    {
        _rcon = new RCON(ip, port, password);

        _rcon.OnDisconnected += OnDisconnected;
    }

    public async Task<string> SendCommand(string command, uint timeout = 10)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await EnsureConnectedAsync();

        return await _rcon.SendCommandAsync(command, TimeSpan.FromSeconds(timeout));
    }

    private async Task EnsureConnectedAsync()
    {
        if (_isConnected)
        {
            return;
        }

        await _rcon.ConnectAsync();
        _isConnected = true;
    }

    private void OnDisconnected()
    {
        _isConnected = false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _disposed = true;

        try
        {
            _rcon.Dispose();
        }
        catch (Exception)
        {
            // Ignored
        }

        GC.ReRegisterForFinalize(this);
    }
}
