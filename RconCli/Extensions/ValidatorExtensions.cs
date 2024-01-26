using RconCli.Configuration;
using RconCli.Models;
using RconCli.Validator;

namespace RconCli.Extensions;

public static class ValidatorExtensions
{
    public static ValidationResult Validate(this Profile profile)
    {
        return ProfileValidator.Validate(profile);
    }
}
