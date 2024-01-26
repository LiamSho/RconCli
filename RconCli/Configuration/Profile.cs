using System.Text.Json.Serialization;

namespace RconCli.Configuration;

public record Profile
{
    public string Name { get; set; } = null!;

    public string Host { get; set; } = null!;

    public ushort Port { get; set; } = 25575;

    public string Password { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(Profile[]))]
public partial class ProfileJsonSerializationContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Profile[]))]
public partial class ProfileJsonMetadataContext : JsonSerializerContext;
