using System.Net;
using RconCli.Abstract;
using RconSharp;

namespace RconCli.Services;

public class RconSharpConnection : IRconConnection
{
    private readonly IPAddress _ip;
    private readonly ushort _port;
    private readonly string _password;

    private RconClient? _rcon;

    private bool _disposed;
    private bool _isConnected;

    private uint _timeout = 10;
    private bool _multiPacketResponse;

    public RconSharpConnection(IPAddress ip, ushort port, string password, uint timeout = 10, bool multiPacketResponse = false)
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

        return await _rcon.ExecuteCommandAsync(command, _multiPacketResponse);
    }

    public async Task ConnectAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await EnsureConnectedAsync();
    }

    public async Task SetTimeout(uint timeout)
    {
        _timeout = timeout;
        CreateClient();

        await EnsureConnectedAsync();
    }

    public Task SetMultiPacketResponse(bool enabled)
    {
        _multiPacketResponse = enabled;

        return Task.CompletedTask;
    }

    private void CreateClient()
    {
        _isConnected = false;

        var socket = new SocketChannel(_ip.ToString(), _port, (int)_timeout, (int)_timeout);
        _rcon = RconClient.Create(socket);
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
        await _rcon.AuthenticateAsync(_password);

        _isConnected = true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _disposed = true;
        _rcon?.Disconnect();

        GC.ReRegisterForFinalize(this);
    }
}
