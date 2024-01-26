using System.Text.RegularExpressions;

namespace RconCli.Extensions;

public static partial class RegexExtensions
{
    [GeneratedRegex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$")]
    // ReSharper disable once InconsistentNaming
    private static partial Regex IPv4AddressRegex();

    [GeneratedRegex("^[a-zA-Z0-9_-]+$")]
    private static partial Regex IdentifierRegex();

    public static bool IsIPv4Address(this string address)
    {
        return IPv4AddressRegex().Match(address).Success;
    }

    public static bool IsIdentifier(this string identifier)
    {
        return IdentifierRegex().Match(identifier).Success;
    }
}
