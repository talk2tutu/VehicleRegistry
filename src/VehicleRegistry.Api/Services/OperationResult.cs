namespace VehicleRegistry.Api.Services;

public sealed class OperationResult<T>
{
    public T? Value { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];
    public bool IsNotFound { get; private init; }
    public bool IsSuccess => !IsNotFound && Errors.Count == 0;

    public static OperationResult<T> Success(T value) => new() { Value = value };
    public static OperationResult<T> Invalid(IReadOnlyList<string> errors) => new() { Errors = errors };
    public static OperationResult<T> NotFound() => new() { IsNotFound = true };
}