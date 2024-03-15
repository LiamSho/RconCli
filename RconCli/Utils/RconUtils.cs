using System.Globalization;
using System.Net;
using System.Net.Sockets;
using RconCli.Abstract;
using RconCli.Enums;
using RconCli.Exceptions;
using RconCli.Extensions;
using RconCli.Services;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Utils;

public static class RconUtils
{
    public static async Task RunInteractive(Profile profile, uint timeoutInSeconds = 10, bool isMultiPacketResponse = false)
    {
        PrintWelcome();
        PrintHelp();

        var timeout = timeoutInSeconds;
        var multiPacket = isMultiPacketResponse;
        var rconLibrary = profile.Library;

        var rcon = await profile.CreateConnection(timeout, multiPacket, rconLibrary);

        AnsiConsole.Console.MarkupLineWithTime("RCON client created.");
        AnsiConsole.WriteLine();

        var commandHistory = new Dictionary<DateTimeOffset, string>();

        while (true)
        {
            var command = AnsiConsole.Prompt(new TextPrompt<string>("[blue][[Command]] [/]"));

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
                        AnsiConsole.Console.MarkupLineWithTime("RCON client disposed.");
                        AnsiConsole.WriteLine();
                        return;
                    case "::info":
                        PrintInfoTable(profile, timeout, multiPacket, rconLibrary);
                        continue;
                    case "::help":
                        PrintHelp();
                        continue;
                    case "::timeout":
                        var seconds = AnsiConsole.Ask("Enter timeout in seconds: ", timeout);
                        timeout = seconds;
                        await rcon.SetTimeout(timeout);
                        AnsiConsole.WriteLine();
                        continue;
                    case "::multipacket":
                        var isMultiPacket = AnsiConsole.Ask("Enable Multi-Packet response?", isMultiPacketResponse);
                        multiPacket = isMultiPacket;
                        await rcon.SetMultiPacketResponse(multiPacket);
                        AnsiConsole.WriteLine();
                        continue;
                    case "::engine":
                        var engine = AnsiConsole.Prompt(
                            new SelectionPrompt<RconLibrary>()
                                .Title("Select RCON library")
                                .AddChoices(RconLibrary.CoreRcon, RconLibrary.RconSharp));
                        if (engine == rconLibrary)
                        {
                            continue;
                        }

                        rconLibrary = engine;
                        rcon = await profile.CreateConnection(timeout, multiPacket, rconLibrary);

                        AnsiConsole.WriteLine();

                        continue;
                    case "::history":
                        var history = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Command History")
                                .AddChoices("::Exit Selection")
                                .AddChoices(commandHistory.FormatHistory())
                                .PageSize(10)
                                .MoreChoicesText("Show more ..."));
                        if (history == "::Exit Selection")
                        {
                            continue;
                        }

                        command = history[23..];
                        break;
                    default:
                        AnsiConsole.Console.MarkupLineWithTime("Unknown shell command.");
                        AnsiConsole.WriteLine();
                        continue;
                }
            }

            commandHistory.Add(DateTimeOffset.UtcNow, command);
            await ExecuteCommand(rcon, command);
        }
    }

    public static async Task RunSingleShot(Profile profile, string command, uint timeout, bool isMultiPacketResponse = false)
    {
        var rcon = await profile.CreateConnection(timeout, isMultiPacketResponse);

        await ExecuteCommand(rcon, command);

        rcon.Dispose();
    }

    private static async Task ExecuteCommand(IRconConnection connection, string command)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.Console.MarkupLineWithTime(
            "[blue][[Client]] [/]{1}",
            command.EscapeMarkup());

        try
        {
            var result = await connection.SendCommandAsync(command);
            AnsiConsole.Console.MarkupLineWithTime(
                "[green][[Server]] [/]{1}",
                result.EscapeMarkup());
        }
        catch (TimeoutException e)
        {
            AnsiConsole.Console.MarkupLineWithTime("[red][[Errors]] [/]Oops! The RCON command timed out.");
            AnsiConsole.Console.PrintError(e.Message);
        }
        catch (SocketException e)
        {
            AnsiConsole.Console.MarkupLineWithTime("[red][[Errors]] [/]Oops! A socket exception occurred while executing the RCON command.");
            AnsiConsole.Console.PrintError(e.Message);
        }
        catch (ObjectDisposedException e)
        {
            AnsiConsole.Console.MarkupLineWithTime("[red][[Errors]] [/]Oops! The RCON connection was disposed while executing the RCON command.");
            AnsiConsole.Console.MarkupLineWithTime("[red][[Errors]] [/]This is not possible to happen. Try to exit and restart the RCON shell.");
            AnsiConsole.WriteException(e);
        }
        catch (Exception e)
        {
            AnsiConsole.Console.MarkupLineWithTime("[red][[Errors]] [/]Oops! RCON command execution threw an unknown exception.");
            AnsiConsole.WriteException(e);
        }

        AnsiConsole.WriteLine();
    }

    private static async Task<IRconConnection> CreateConnection(
        this Profile profile,
        uint timeout = 10,
        bool isMultiPacketResponse = false,
        RconLibrary? rconLibrary = null)
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

            AnsiConsole.Console.MarkupLineWithTime(
                "Resolved host [bold]'{1}'[/] to [bold]'{2}'[/].",
                profile.Host.EscapeMarkup(),
                ip.ToString().EscapeMarkup());
        }

        var library = rconLibrary ?? profile.Library;

        AnsiConsole.Console.MarkupLineWithTime(
            "Connecting using [bold]'{1}'[/] library.",
            library.ToString().EscapeMarkup());

        return library switch
        {
            RconLibrary.RconSharp => new RconSharpConnection(ip, profile.Port, profile.Password, timeout, isMultiPacketResponse),
            RconLibrary.CoreRcon => new CoreRconConnection(ip, profile.Port, profile.Password, timeout, isMultiPacketResponse),
            _ => throw new ArgumentOutOfRangeException(nameof(rconLibrary), rconLibrary, null)
        };
    }

    private static void PrintHelp()
    {
        var commands = new Dictionary<string, string>
        {
            { "[bold]::exit[/]", "Exit the RCON shell." },
            { "[bold]::info[/]", "Show connection information" },
            { "[bold]::help[/]", "Print this help message" },
            { "[bold]::timeout[/]", "Set the command timeout in seconds. (Default is 10)" },
            { "[bold]::multipacket[/]", "Enable or disable Multi-Packet response. (Default is disabled)" },
            { "[bold]::engine[/]", "Change the RCON library." },
            { "[bold]::history[/]", "Show command history of current session" }
        };

        AnsiConsole.Console.PrintDictionary(
            "RCON CLI shell mode commands",
            "Command",
            "Description",
            commands);

        AnsiConsole.MarkupLine("If your RCON command conflicts with a shell command, prefix it with [bold]'$'[/].");
        AnsiConsole.MarkupLine("  > [bold]'::exit'[/] will be interpreted as a shell command [bold]'::exit'[/] and be executed.");
        AnsiConsole.MarkupLine("  > [bold]'$::exit'[/] will be interpreted as a RCON server command [bold]'::exit'[/] and send through RCON.");
        AnsiConsole.MarkupLine("  > [bold]'$$::exit'[/] will be interpreted as a RCON server command [bold]'$::exit'[/] and send through RCON.");

        AnsiConsole.WriteLine();
    }

    private static void PrintWelcome()
    {
        AnsiConsole.Console.PrintFiglet();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Entering RCON shell mode.");
        AnsiConsole.WriteLine();
    }

    private static void PrintInfoTable(Profile profile, uint timeout, bool multiPacket, RconLibrary library)
    {
        var infos = new Dictionary<string, string>
        {
            { "Profile", profile.Name },
            { "Description", profile.Description },
            { "Host", profile.Host },
            { "Port", profile.Port.ToString(CultureInfo.InvariantCulture) },
            { "Timeout", timeout.ToString(CultureInfo.InvariantCulture) },
            { "Multi-Packet", multiPacket.ToString() },
            { "Library", library.ToString() }
        };

        AnsiConsole.Console.PrintDictionary("RCON connection info", infos);

        AnsiConsole.WriteLine();
    }

    private static IEnumerable<string> FormatHistory(this Dictionary<DateTimeOffset, string> history)
    {
        return history
            .OrderByDescending(x => x.Key)
            .Select(x => $"[green][[{x.Key.LocalDateTime:HH:mm:ss}]][/] {x.Value}");
    }
}
