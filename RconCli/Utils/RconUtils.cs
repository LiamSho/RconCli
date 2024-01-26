using System.Globalization;
using RconCli.Services;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Utils;

public static class RconUtils
{
    public static async Task RunInteractive(Profile profile)
    {
        PrintHelp();

        var rcon = new RconConnection(profile);

        AnsiConsole.WriteLine("RCON client created.");

        while (true)
        {
            var command = AnsiConsole.Prompt(new TextPrompt<string>("[red][[Command]] [/]"));

            switch (command)
            {
                case "::exit":
                    AnsiConsole.MarkupLine("Exiting RCON shell.");
                    rcon.Dispose();
                    AnsiConsole.WriteLine("RCON client disposed.");
                    return;
            }

            await ExecuteCommand(rcon, command);
        }
    }

    public static async Task RunSingleShot(Profile profile, string command)
    {
        var rcon = new RconConnection(profile);

        await ExecuteCommand(rcon, command);

        rcon.Dispose();
    }

    private static async Task ExecuteCommand(RconConnection connection, string command)
    {
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

        table.Caption = new TableTitle("RCON CLI shell mode commands");
        table.ShowHeaders = true;

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
