using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Spectre.Console;
using Profile = RconCli.Configuration.Profile;

namespace RconCli.Extensions;

public static class AnsiConsoleExtensions
{
    public static void PrintErrors(this IAnsiConsole console, IEnumerable<string> errors)
    {
        console.MarkupLine("Oops! Something went wrong.");

        foreach (var error in errors)
        {
            console.PrintError(error);
        }
    }

    public static void PrintError(this IAnsiConsole console, object error)
    {
        console.MarkupLine(CultureInfo.InvariantCulture, "  > [red]{0}[/]", error);
    }

    [SuppressMessage("Minor Code Smell", "S3878:Arrays should not be created for params parameters")]
    public static void MarkupLineWithTime(this IAnsiConsole console, string format, params object[] args)
    {
        console.MarkupLine(CultureInfo.InvariantCulture,
            "{0} " + format,
            [DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture), ..args]);
    }

    public static void MarkupLineWithTime(this IAnsiConsole console, object message)
    {
        console.MarkupLine(CultureInfo.InvariantCulture,
            "{0} {1}",
            DateTimeOffset.Now.ToString("T", CultureInfo.InvariantCulture),
            message);
    }

    public static void PrintProfile(this IAnsiConsole console, Profile profile, bool includePassword = false)
    {
        var dictionary = new Dictionary<string, string>
        {
            { "Name", profile.Name },
            { "Host", profile.Host },
            { "Port", profile.Port.ToString(CultureInfo.InvariantCulture) },
            { "Library", profile.Library.ToString() }
        };

        if (includePassword)
        {
            dictionary.Add("Password", profile.Password);
        }

        dictionary.Add("Description", profile.Description);

        console.PrintDictionary("Profile Detail", dictionary);
    }

    public static void PrintDictionary(this IAnsiConsole console, string title, IDictionary<string, string> dictionary)
    {
        console.PrintDictionary(title, "Property", "Value", dictionary);
    }

    public static void PrintDictionary(this IAnsiConsole console, string title, string keyTitle, string valueTitle, IDictionary<string, string> dictionary)
    {
        var table = new Table();

        table.AddColumns(keyTitle, valueTitle);

        foreach (var (key, value) in dictionary)
        {
            table.AddRow(key, value);
        }

        table.Title = new TableTitle(title, new Style(Color.Black));

        console.Write(table);
    }

    public static void PrintProfiles(this IAnsiConsole console, IEnumerable<Profile> profiles, bool includePassword = false)
    {
        var table = new Table();

        var columns = new List<string> { "Name", "Host", "Port", "Library", "Password", "Description" };
        if (includePassword is false)
        {
            columns.RemoveAt(4);
        }

        table.AddColumns(columns.ToArray());

        foreach (var profile in profiles)
        {
            var row = new List<string>
            {
                profile.Name,
                profile.Host,
                profile.Port.ToString(CultureInfo.InvariantCulture),
                profile.Library.ToString(),
                profile.Password,
                profile.Description
            };
            if (includePassword is false)
            {
                row.RemoveAt(4);
            }

            table.AddRow(row.ToArray());
        }

        table.Title = new TableTitle("Profile List", new Style(Color.Black));

        console.Write(table);
    }

    public static void PrintFiglet(this IAnsiConsole console)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unknown";

        console.Write(new FigletText("RCON CLI"));
        console.MarkupLine(
            CultureInfo.InvariantCulture,
            "Welcome to the RCON CLI v{0}",
            version.EscapeMarkup());

        console.WriteLine();

        console.MarkupLine("RCON CLI is an Open Source project licensed under the MIT license.");
        console.MarkupLine("Source code is available at: [link]https://github.com/LiamSho/RconCli[/]");
        console.MarkupLine("License information is available at: [link]https://github.com/LiamSho/RconCli/blob/main/LICENSE[/]");
    }
}
