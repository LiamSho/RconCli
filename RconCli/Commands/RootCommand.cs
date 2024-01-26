using Cocona;
using Cocona.ShellCompletion.Candidate;
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
    [Command("connect", Description = "Connect to a server defined in a profile.")]
    public async Task ConnectAsync(
        [Argument("profile name", Description = "The name of the profile.")]
        [CompletionCandidates(typeof(ProfileNameProvider))]
        string name,
        [Option('c', Description = "Execute the command and exit.")] string? command = null)
    {
        var profile = await ProfileManager.Instance.GetProfileAsync(name);

        if (profile is null)
        {
            AnsiConsole.Console.PrintErrors([$"Profile '{name}' does not exist."]);
            return;
        }

        if (command is null)
        {
            await RconUtils.RunInteractive(profile);
        }
        else
        {
            await RconUtils.RunSingleShot(profile, command);
        }
    }

    [Command("direct", Description = "Connect to a server directly.")]
    public async Task ConnectAsync(
        [Option('H', Description = "Server host. Can be IPv4 address or a hostname that can be resolved to a IPv4 address.")] string host,
        [Option('p', Description = "Server port.")] ushort port,
        [Option('w', Description = "Server password.")] string password,
        [Option('c', Description = "Execute the command and exit.")] string? command = null)
    {
        var profile = new Profile
        {
            Name = "direct-connect",
            Host = host,
            Port = port,
            Password = password,
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
            await RconUtils.RunInteractive(profile);
        }
        else
        {
            await RconUtils.RunSingleShot(profile, command);
        }
    }
}
