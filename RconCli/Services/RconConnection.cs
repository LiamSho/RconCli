using System.Net;
using CoreRCON;
using CoreRCON.PacketFormats;
using RconCli.Configuration;
using RconCli.Exceptions;
using RconCli.Extensions;

namespace RconCli.Services;

public class RconConnection : IDisposable
{
    private readonly RCON _rcon;

    private bool _disposed;
    private bool _isConnected;

    public RconConnection(Profile profile)
    {
        var host = profile.Host;

        IPAddress ip;
        if (host.IsIpV4Address())
        {
            ip = IPAddress.Parse(host);
        }
        else
        {
            var address = Dns.GetHostAddresses(host).FirstOrDefault()
                          ?? throw new DnsResolveException(host);
            ip = address;
        }

        _rcon = new RCON(ip, profile.Port, profile.Password);

        _rcon.OnDisconnected += OnDisconnected;
    }

    public async Task<string> SendCommand(string command)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await EnsureConnectedAsync();

        return await _rcon.SendCommandAsync(command);
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
        _rcon.Dispose();

        GC.ReRegisterForFinalize(this);
    }
}
