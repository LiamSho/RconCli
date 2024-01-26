using System.Diagnostics.CodeAnalysis;

namespace RconCli.Models;

public readonly struct Result<TSuccess, TFailed> where TSuccess : class where TFailed : class
{
    private readonly TSuccess? _success;
    private readonly TFailed? _failed;

    private Result(TSuccess? success, TFailed? failed)
    {
        _success = success;
        _failed = failed;
    }

    public static Result<TSuccess, TFailed> Success([NotNull] TSuccess value) => new(value, null);
    public static Result<TSuccess, TFailed> Failed([NotNull] TFailed value) => new(null, value);

    public bool IsSuccess => _success is not null;
    public bool IsFailed => _failed is not null;

    public TSuccess AsSuccess() => _success ?? throw new InvalidOperationException("The result is not success.");
    public TFailed AsFailed() => _failed ?? throw new InvalidOperationException("The result is not failed.");

    public static implicit operator Result<TSuccess, TFailed>(TSuccess value) => Success(value);
    public static implicit operator Result<TSuccess, TFailed>(TFailed value) => Failed(value);
}
