namespace RconCli.Exceptions;

public class DnsResolveException : Exception
{
    public DnsResolveException(string hostname)
        :base($"DNS failed to resolve {hostname}.")
    {
    }
}
