using System.Text.RegularExpressions;

namespace RconCli.Extensions;

public static partial class RegexExtensions
{
    [GeneratedRegex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")]
    private static partial Regex IpV4AddressRegex();

    [GeneratedRegex("^[a-zA-Z0-9_-]+$")]
    private static partial Regex IdentifierRegex();

    public static bool IsIpV4Address(this string address)
    {
        return IpV4AddressRegex().Match(address).Success;
    }

    public static bool IsIdentifier(this string identifier)
    {
        return IdentifierRegex().Match(identifier).Success;
    }
}
