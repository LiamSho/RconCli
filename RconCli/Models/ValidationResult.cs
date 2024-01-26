namespace RconCli.Models;

public record ValidationResult
{
    public bool IsSuccess { get; init; }

    public List<string> Errors { get; init; } = [];
}
