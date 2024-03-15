using System.Globalization;
using System.Runtime.InteropServices;
using Cocona;
using Cocona.ShellCompletion.Candidate;
using RconCli.Enums;
using RconCli.Extensions;
using RconCli.Providers;
using RconCli.Services;
using RconCli.Utils;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Commands;

[HasSubCommands(typeof(ProfileCommand), "profile", Description = "Manage profiles.")]
public class RootCommand
{
    [Command("connect", Aliases = ["c"], Description = "Connect to a server defined in a profile. (alias: c)")]
    public async Task ConnectAsync(
        [Argument("profile name", Description = "The name of the profile.")]
        [CompletionCandidates(typeof(ProfileNameProvider))]
        string name,
        [Option('c', Description = "Execute the command and exit.")] string? command = null,
        [Option('m', Description = "Enable RCON Multi-Packet response mode.")] bool multiPacket = false,
        [Option('t', Description = "Timeout in seconds.")] uint timeout = 10)
    {
        var profile = await ProfileManager.Instance.GetProfileAsync(name);

        if (profile is null)
        {
            AnsiConsole.Console.PrintErrors([$"Profile '{name}' does not exist."]);
            return;
        }

        if (command is null)
        {
            await RconUtils.RunInteractive(profile, timeout, multiPacket);
        }
        else
        {
            await RconUtils.RunSingleShot(profile, command, timeout, multiPacket);
        }
    }

    [Command("direct", Aliases = ["d", "rcon"], Description = "Connect to a server directly. (alias: d, rcon)")]
    public async Task DirectConnectAsync(
        [Option('H', Description = "Server host. Can be IPv4 address or a hostname that can be resolved to a IPv4 address.")] string host,
        [Option('p', Description = "Server port.")] ushort port,
        [Option('w', Description = "Server password.")] string password,
        [Option('c', Description = "Execute the command and exit.")] string? command = null,
        [Option('e', Description = "RCON library to use.")] RconLibrary library = RconLibrary.RconSharp,
        [Option('m', Description = "Enable RCON Multi-Packet response mode.")] bool multiPacket = false,
        [Option('t', Description = "Timeout in seconds.")] uint timeout = 10)
    {
        var profile = new Profile
        {
            Name = "direct-connect",
            Host = host,
            Port = port,
            Password = password,
            Library = library,
            Description = string.Empty
        };

        var validationResult = profile.Validate();

        if (validationResult.IsSuccess is false)
        {
            AnsiConsole.Console.PrintErrors(validationResult.Errors);
            return;
        }

        if (command is null)
        {
            await RconUtils.RunInteractive(profile, timeout, multiPacket);
        }
        else
        {
            await RconUtils.RunSingleShot(profile, command, timeout, multiPacket);
        }
    }

    [Command("info", Aliases = ["i"], Description = "Display RCON CLI info. (alias: i)")]
    public void InfoAsync()
    {
        AnsiConsole.Console.PrintFiglet();

        AnsiConsole.WriteLine();

        var appDataDirectory = PathUtils.GetAppDataDirectory();

        AnsiConsole.MarkupLine(CultureInfo.InvariantCulture, "Runtime: [bold]{0}[/]", RuntimeInformation.FrameworkDescription);
        AnsiConsole.MarkupLine($"Application data directory: [bold]{appDataDirectory}[/]");
    }
}
