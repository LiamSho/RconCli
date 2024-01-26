using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using RconCli.Exceptions;
using RconCli.Extensions;
using RconCli.Services;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Utils;

public static class RconUtils
{
    public static async Task RunInteractive(Profile profile)
    {
        PrintWelcome();
        PrintHelp();

        var rcon = await profile.CreateConnection();

        AnsiConsole.MarkupLine("RCON client created.");
        AnsiConsole.WriteLine();

        while (true)
        {
            var command = AnsiConsole.Prompt(new TextPrompt<string>("[red][[Command]] [/]"));

            var isShellCommand = false;

            if (command.StartsWith("::", StringComparison.InvariantCulture))
            {
                isShellCommand = true;
            }
            else if (command.StartsWith('$'))
            {
                isShellCommand = false;
                command = command[1..];
            }

            if (isShellCommand)
            {
                AnsiConsole.WriteLine();

                switch (command)
                {
                    case "::exit":
                        rcon.Dispose();
                        AnsiConsole.MarkupLine("RCON client disposed.");
                        AnsiConsole.WriteLine();
                        return;
                    default:
                        AnsiConsole.MarkupLine("Unknown shell command.");
                        AnsiConsole.WriteLine();
                        continue;
                }
            }

            await ExecuteCommand(rcon, command);
        }
    }

    public static async Task RunSingleShot(Profile profile, string command)
    {
        var rcon = await profile.CreateConnection();

        await ExecuteCommand(rcon, command);

        rcon.Dispose();
    }

    private static async Task ExecuteCommand(RconConnection connection, string command)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(CultureInfo.InvariantCulture,
            "{0}[red] [[Command]] [/]{1}",
            DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture),
            command.EscapeMarkup());

        try
        {
            var result = await connection.SendCommand(command);
            AnsiConsole.MarkupLine(CultureInfo.InvariantCulture,
                "{0}[green] [[Results]] [/]{1}",
                DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture),
                result.EscapeMarkup());
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine("Oops! RCON command execution threw an exception.");
            AnsiConsole.WriteException(e);
        }

        AnsiConsole.WriteLine();
    }

    private static async Task<RconConnection> CreateConnection(this Profile profile)
    {
        IPAddress ip;

        if (profile.Host.IsIPv4Address())
        {
            ip = IPAddress.Parse(profile.Host);
        }
        else
        {
            var addresses = await Dns.GetHostAddressesAsync(profile.Host);

            var ipv4Addresses = addresses
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();

            if (ipv4Addresses.Length == 0)
            {
                throw new DnsResolveException($"Could not resolve host '{profile.Host}' to an IPv4 address.");
            }

            ip = ipv4Addresses[0];

            AnsiConsole.MarkupLine(CultureInfo.InvariantCulture,
                "Resolved host [bold]'{0}'[/] to [bold]'{1}'[/].",
                profile.Host.EscapeMarkup(),
                ip.ToString().EscapeMarkup());
        }

        return new RconConnection(ip, profile.Port, profile.Password);
    }

    private static void PrintHelp()
    {
        var commands = new Dictionary<string, string>
        {
            { "exit", "Exit the RCON shell." }
        };

        var table = new Table();

        table.AddColumns("Command", "Description");

        foreach (var (key, value) in commands)
        {
            table.AddRow($"[bold]::{key}[/]", value);
        }

        table.Title = new TableTitle("RCON CLI shell mode commands", new Style(Color.Black));
        table.ShowHeaders = true;

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("If your RCON command conflicts with a shell command, prefix it with [bold]'$'[/].");
        AnsiConsole.MarkupLine("  > [bold]'::exit'[/] will be interpreted as a shell command [bold]'::exit'[/] and be executed.");
        AnsiConsole.MarkupLine("  > [bold]'$::exit'[/] will be interpreted as a RCON server command [bold]'::exit'[/] and send through RCON.");
        AnsiConsole.MarkupLine("  > [bold]'$$::exit'[/] will be interpreted as a RCON server command [bold]'$::exit'[/] and send through RCON.");

        AnsiConsole.WriteLine();
    }

    private static void PrintWelcome()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unknown";

        AnsiConsole.Write(new FigletText("RCON CLI"));
        AnsiConsole.MarkupLine(
            CultureInfo.InvariantCulture,
            "Welcome to the RCON CLI v{0}",
            version.EscapeMarkup());

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Entering RCON shell mode.");
        AnsiConsole.WriteLine();
    }
}
