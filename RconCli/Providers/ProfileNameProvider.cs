using Cocona.CommandLine;
using Cocona.ShellCompletion.Candidate;
using RconCli.Services;

namespace RconCli.Providers;

public class ProfileNameProvider : ICoconaCompletionOnTheFlyCandidatesProvider
{
    public IReadOnlyList<CompletionCandidateValue> GetCandidates(CoconaCompletionCandidatesMetadata metadata, ParsedCommandLine parsedCommandLine)
    {
        var profiles = ProfileManager.Instance
            .GetProfilesAsync()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        return profiles
            .Select(x => new CompletionCandidateValue(x.Name, x.Description))
            .ToList();
    }
}
