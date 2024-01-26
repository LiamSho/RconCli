using System.Net;
using CoreRCON;
using RconCli.Abstract;

namespace RconCli.Services;

public class CoreRconConnection : IRconConnection
{
    private readonly IPAddress _ip;
    private readonly ushort _port;
    private readonly string _password;

    private RCON? _rcon;

    private bool _disposed;
    private bool _isConnected;

    private uint _timeout;
    private bool _multiPacketResponse;

    public CoreRconConnection(IPAddress ip, ushort port, string password, uint timeout = 10, bool multiPacketResponse = false)
    {
        _ip = ip;
        _port = port;
        _password = password;

        _timeout = timeout;
        _multiPacketResponse = multiPacketResponse;

        CreateClient();
    }

    public async Task<string> SendCommandAsync(string command)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_rcon);

        await EnsureConnectedAsync();

        return await _rcon.SendCommandAsync(command);
    }

    public async Task ConnectAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await EnsureConnectedAsync();
    }

    public Task SetTimeout(uint timeout)
    {
        _timeout = timeout;

        return Task.CompletedTask;
    }

    public async Task SetMultiPacketResponse(bool enabled)
    {
        _multiPacketResponse = enabled;
        CreateClient();

        await EnsureConnectedAsync();
    }

    private void CreateClient()
    {
        _isConnected = false;

        _rcon = new RCON(_ip, _port, _password, _timeout, _multiPacketResponse);
    }

    private async Task EnsureConnectedAsync()
    {
        if (_isConnected)
        {
            return;
        }

        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(_rcon);

        await _rcon.ConnectAsync();
        _isConnected = true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _disposed = true;

        try
        {
            _rcon?.Dispose();
        }
        catch (Exception)
        {
            // Ignored
        }

        GC.ReRegisterForFinalize(this);
    }
}
