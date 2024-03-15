using Spectre.Console;

namespace RconCli.Models;

public record PrintableTable
{
    public string Title { get; set; }

    public List<string> Columns { get; } = [];

    public List<string[]> Rows { get; } = [];

    public PrintableTable(string title)
    {
        Title = title;
    }

    public void AddRow(params string[] values)
    {
        Rows.Add(values);
    }

    public void Print(IAnsiConsole console)
    {
        var table = new Table();
        table.Title(Title);

        table.AddColumns(Columns.ToArray());

        foreach (var row in Rows)
        {
            table.AddRow(row);
        }

        console.Write(table);
    }
}
