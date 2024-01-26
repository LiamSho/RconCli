using System.Text.Json;
using OneOf;
using RconCli.Configuration;
using RconCli.Extensions;
using RconCli.Utils;

namespace RconCli.Services;

public class ProfileManager
{
    private readonly FileInfo _profileConfigurationFile;

    private ProfileManager()
    {
        var appDataDirectoryInfo = PathUtils.GetAppDataDirectory();
        _profileConfigurationFile = new FileInfo(
            Path.Combine(appDataDirectoryInfo.FullName, "profiles.json"));

        if (_profileConfigurationFile.Exists is false)
        {
            var stream = _profileConfigurationFile.Create();
            var jsonArray = "[]"u8.ToArray();
            stream.Write(jsonArray);
            stream.Close();
        }
    }

    private static ProfileManager? _instance;

    public static ProfileManager Instance
    {
        get
        {
            _instance ??= new ProfileManager();
            return _instance;
        }
    }

    public async Task<OneOf<Profile, List<string>>> CreateProfileAsync(Profile profile)
    {
        var validationResult = profile.Validate();

        if (validationResult.IsSuccess is false)
        {
            return validationResult.Errors;
        }

        var profiles = await GetProfilesAsync()
            .ToListAsync();

        if (profiles.Exists(x => x.Name == profile.Name))
        {
            return new List<string>(["The profile with the same name already existed."]);
        }

        profiles.Add(profile);
        var message = await WriteProfilesAsync(profiles);

        if (message is null)
        {
            return profile;
        }

        return new List<string> { message };
    }

    public async Task<Profile?> GetProfileAsync(string name)
    {
        var profiles = await GetProfilesAsync()
            .ToListAsync();

        return profiles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<OneOf<Profile, List<string>>> UpdateProfileAsync(Profile profile)
    {
        var profiles = await GetProfilesAsync()
            .ToListAsync();

        var existingProfile = profiles.FirstOrDefault(x => x.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase));

        if (existingProfile is null)
        {
            return new List<string> { $"Profile with name '{profile.Name}' does not exist" };
        }

        var validationResult = profile.Validate();
        if (validationResult.IsSuccess is false)
        {
            return validationResult.Errors;
        }

        profiles.Remove(existingProfile);
        profiles.Add(profile);

        var message = await WriteProfilesAsync(profiles);

        if (message is null)
        {
            return profile;
        }

        return new List<string> { message };
    }

    public async Task<OneOf<Profile, string>> RemoveProfileAsync(string name)
    {
        var profiles = await GetProfilesAsync()
            .ToListAsync();

        var existingProfile = profiles.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (existingProfile is null)
        {
            return $"Profile with name '{name}' does not exist";
        }

        profiles.Remove(existingProfile);

        var message = await WriteProfilesAsync(profiles);

        if (message is null)
        {
            return existingProfile;
        }

        return message;
    }

    public async Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        var json = await File.ReadAllTextAsync(_profileConfigurationFile.FullName);
        return JsonSerializer.Deserialize(json, ProfileJsonMetadataContext.Default.ProfileArray) ?? Enumerable.Empty<Profile>();
    }

    private async Task<string?> WriteProfilesAsync(IEnumerable<Profile> profiles)
    {
        try
        {
            var json = JsonSerializer.Serialize(profiles.ToArray(), ProfileJsonSerializationContext.Default.ProfileArray);
            await File.WriteAllTextAsync(_profileConfigurationFile.FullName, json);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
