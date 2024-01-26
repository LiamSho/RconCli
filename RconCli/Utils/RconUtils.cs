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
    public static async Task RunInteractive(Profile profile, uint timeoutInSeconds)
    {
        PrintWelcome();
        PrintHelp();

        var rcon = await profile.CreateConnection();

        AnsiConsole.MarkupLine("RCON client created.");
        AnsiConsole.WriteLine();

        var timeout = timeoutInSeconds;

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
                    case "::info":
                        PrintInfoTable(profile, timeout);
                        continue;
                    case "::help":
                        PrintHelp();
                        continue;
                    case "::timeout":
                        var seconds = AnsiConsole.Prompt(new TextPrompt<uint>("Enter timeout in seconds: "));
                        timeout = seconds;
                        AnsiConsole.WriteLine();
                        continue;
                    default:
                        AnsiConsole.MarkupLine("Unknown shell command.");
                        AnsiConsole.WriteLine();
                        continue;
                }
            }

            await ExecuteCommand(rcon, command, timeout);
        }
    }

    public static async Task RunSingleShot(Profile profile, string command, uint timeout)
    {
        var rcon = await profile.CreateConnection();

        await ExecuteCommand(rcon, command, timeout);

        rcon.Dispose();
    }

    private static async Task ExecuteCommand(RconConnection connection, string command, uint timeout)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(CultureInfo.InvariantCulture,
            "{0}[red] [[Command]] [/]{1}",
            DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture),
            command.EscapeMarkup());

        try
        {
            var result = await connection.SendCommand(command, timeout);
            AnsiConsole.MarkupLine(CultureInfo.InvariantCulture,
                "{0}[green] [[Results]] [/]{1}",
                DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture),
                result.EscapeMarkup());
        }
        catch (TimeoutException e)
        {
            AnsiConsole.MarkupLine("Oops! The RCON command timed out.");
            AnsiConsole.MarkupLine(CultureInfo.InvariantCulture, "  > [red]{0}[/]", e.Message);
        }
        catch (SocketException e)
        {
            AnsiConsole.MarkupLine("Oops! A socket exception occurred while executing the RCON command.");
            AnsiConsole.MarkupLine(CultureInfo.InvariantCulture, "  > [red]{0}[/]", e.Message);
        }
        catch (ObjectDisposedException e)
        {
            AnsiConsole.MarkupLine("Oops! The RCON connection was disposed while executing the RCON command.");
            AnsiConsole.MarkupLine("This is not possible to happen. Try to exit and restart the RCON shell.");
            AnsiConsole.WriteException(e);
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine("Oops! RCON command execution threw an unknown exception.");
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
            { "exit", "Exit the RCON shell." },
            { "info", "Show connection information" },
            { "help", "Print this help message" },
            { "timeout", "Set the command timeout in seconds. (Default is 10)" }
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

    private static void PrintInfoTable(Profile profile, uint timeout)
    {
        var infoTable = new Table();
        infoTable.AddColumns("Property", "Value");
        infoTable.AddRow("Profile", profile.Name);
        infoTable.AddRow("Description", profile.Description);
        infoTable.AddRow("Host", profile.Host);
        infoTable.AddRow("Port", profile.Port.ToString(CultureInfo.InvariantCulture));
        infoTable.AddRow("Timeout", timeout.ToString(CultureInfo.InvariantCulture));
        infoTable.ShowHeaders = false;
        AnsiConsole.Write(infoTable);
        AnsiConsole.WriteLine();
    }
}
