using Cocona;
using Cocona.ShellCompletion.Candidate;
using RconCli.Extensions;
using RconCli.Providers;
using RconCli.Services;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Commands;

public class ProfileCommand
{
    [Command("add", Description = "Add a new profile.")]
    public async Task AddAsync(
        [Argument("profile name", Description = "The name of the profile, only allow letters, numbers, underscores and dashes.", Order = 0)] string name,
        [Option('H', Description = "Server host. Can be IPv4 address or a hostname that can be resolved to a IPv4 address.")] string host,
        [Option('p', Description = "Server port.")] ushort port,
        [Option('w', Description = "Server password.")] string password,
        [Option('d', Description = "Description message.")] string? description = null)
    {
        var profile = new Profile
        {
            Name = name,
            Host = host,
            Port = port,
            Password = password,
            Description = description ?? string.Empty
        };

        var result = await ProfileManager.Instance.CreateProfileAsync(profile);

        if (result.IsT1)
        {
            AnsiConsole.Console.PrintErrors(result.AsT1);
            return;
        }

        AnsiConsole.MarkupLine($"[green]Profile '{name}' added successfully.[/]");
        AnsiConsole.Console.PrintProfile(profile);
    }

    [Command("list", Aliases = ["ls"], Description = "List all profiles. (alias: ls)")]
    public async Task ListAsync(
        [Option('s', Description = "Show passwords in the output.")] bool clear = false)
    {
        var profiles = await ProfileManager.Instance.GetProfilesAsync();
        AnsiConsole.Console.PrintProfiles(profiles, clear);
    }

    [Command("remove", Aliases = ["rm"], Description = "Remove a profile. (alias: rm)")]
    public async Task RemoveAsync(
        [Argument("profile name", Description = "The name of the profile.", Order = 0)]
        [CompletionCandidates(typeof(ProfileNameProvider))]
        string name)
    {
        var result = await ProfileManager.Instance.RemoveProfileAsync(name);

        if (result.IsT1)
        {
            AnsiConsole.Console.PrintErrors([result.AsT1]);
            return;
        }

        AnsiConsole.MarkupLine($"[green]Profile '{name}' removed successfully.[/]");
        AnsiConsole.Console.PrintProfile(result.AsT0);
    }

    [Command("edit", Description = "Edit a profile.")]
    public async Task EditAsync(
        [Argument("profile name", Description = "The name of the profile, only allow letters, numbers, underscores and dashes.", Order = 0)]
        [CompletionCandidates(typeof(ProfileNameProvider))]
        string name,
        [Option('H', Description = "Server host. Can be IPv4 address or a hostname that can be resolved to a IPv4 address.")] string? host = null,
        [Option('p', Description = "Server port.")] ushort? port = null,
        [Option('w', Description = "Server password.")] string? password = null,
        [Option('d', Description = "Description message.")] string? description = null)
    {
        var existingProfile = await ProfileManager.Instance.GetProfileAsync(name);

        if (existingProfile is null)
        {
            AnsiConsole.Console.PrintErrors([$"Profile '{name}' does not exist."]);
            return;
        }

        existingProfile.Host = host ?? existingProfile.Host;
        existingProfile.Port = port ?? existingProfile.Port;
        existingProfile.Password = password ?? existingProfile.Password;
        existingProfile.Description = description ?? existingProfile.Description;

        var result = await ProfileManager.Instance.UpdateProfileAsync(existingProfile);

        if (result.IsT1)
        {
            AnsiConsole.Console.PrintErrors(result.AsT1);
            return;
        }

        AnsiConsole.MarkupLine($"[green]Profile '{name}' updated successfully.[/]");
        AnsiConsole.Console.PrintProfile(result.AsT0, password is not null);
    }
}
