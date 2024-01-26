using RconCli.Configuration;
using RconCli.Extensions;
using RconCli.Models;

namespace RconCli.Validator;

public static class ProfileValidator
{
    public static ValidationResult Validate(Profile p)
    {
        var result = new ValidationResult
        {
            IsSuccess = false,
            Errors = []
        };

        if (string.IsNullOrWhiteSpace(p.Name))
        {
            result.Errors.Add("Name is required.");
        }

        if (p.Name.IsIdentifier() is false)
        {
            result.Errors.Add("Name is invalid.");
        }

        if (string.IsNullOrWhiteSpace(p.Host))
        {
            result.Errors.Add("Host is required.");
        }

        if (p.Port == 0)
        {
            result.Errors.Add("Port is required.");
        }

        if (string.IsNullOrWhiteSpace(p.Password))
        {
            result.Errors.Add("Password is required.");
        }

        if (result.Errors.Count != 0)
        {
            return result;
        }

        return result with { IsSuccess = true };
    }
}
