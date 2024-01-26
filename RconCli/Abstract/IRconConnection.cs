namespace RconCli.Abstract;

public interface IRconConnection : IDisposable
{
    public Task<string> SendCommandAsync(string command);

    public Task ConnectAsync();

    public Task SetTimeout(uint timeout);

    public Task SetMultiPacketResponse(bool enabled);
}
