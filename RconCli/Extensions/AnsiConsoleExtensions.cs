using System.Globalization;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Extensions;

public static class AnsiConsoleExtensions
{
    public static void PrintErrors(this IAnsiConsole console, IEnumerable<string> errors)
    {
        console.MarkupLine("[red]Oops! Something went wrong.[/]");

        foreach (var error in errors)
        {
            console.MarkupLine($"> [red]{error}[/]");
        }
    }

    public static void PrintProfile(this IAnsiConsole console, Profile profile, bool includePassword = false)
    {
        var table = new Table();

        table.AddColumns("Parameter", "Value");

        table.AddRow("Name", profile.Name);
        table.AddRow("Host", profile.Host);
        table.AddRow("Port", profile.Port.ToString(CultureInfo.InvariantCulture));

        if (includePassword)
        {
            table.AddRow("Password", profile.Password);
        }

        table.AddRow("Description", profile.Description);

        table.Caption = new TableTitle("Profile Detail");
        table.ShowHeaders = false;

        console.Write(table);
    }

    public static void PrintProfiles(this IAnsiConsole console, IEnumerable<Profile> profiles, bool includePassword = false)
    {
        var table = new Table();

        var columns = new List<string> { "Name", "Host", "Port", "Password", "Description" };
        if (includePassword is false)
        {
            columns.Remove("Password");
        }

        table.AddColumns(columns.ToArray());

        foreach (var profile in profiles)
        {
            var row = new List<string> { profile.Name, profile.Host, profile.Port.ToString(CultureInfo.InvariantCulture), profile.Password, profile.Description };
            if (includePassword is false)
            {
                row.RemoveAt(3);
            }

            table.AddRow(row.ToArray());
        }

        table.Caption = new TableTitle("Profile List");
        table.ShowHeaders = true;

        console.Write(table);
    }
}
