using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Web.Dashboard.Models;

public readonly struct OperationResult<T>
{
    private readonly T? _item;

    private OperationResult(T? item, IImmutableList<string> errorMessages)
    {
        _item = item;
        ErrorMessages = errorMessages;
    }

    public IImmutableList<string> ErrorMessages { get; }

    public bool TryGetResult([NotNullWhen(true)] out T? result)
    {
        result = _item;
        return _item != null;
    }

    public static OperationResult<T> SuccessResult(T item) =>
        new(item, ImmutableList<string>.Empty);

    public static OperationResult<T> FailureResult(params string[] errorMessages) =>
        new(default, ImmutableList.Create(errorMessages));

    public static OperationResult<T> FailureResult(IEnumerable<string> errorMessages) =>
        new(default, ImmutableList.CreateRange(errorMessages));
}

public readonly struct OperationResult
{
    private OperationResult(IImmutableList<string> errorMessages)
    {
        ErrorMessages = errorMessages;
    }

    public IImmutableList<string> ErrorMessages { get; }

    public bool IsSuccess() => ErrorMessages.Count == 0;

    public static OperationResult SuccessResult() =>
        new(ImmutableList<string>.Empty);

    public static OperationResult FailureResult(params string[] errorMessages) =>
        new(ImmutableList.Create(errorMessages));

    public static OperationResult FailureResult(IEnumerable<string> errorMessages) =>
        new(ImmutableList.CreateRange(errorMessages));
}

public static class OperationResults
{
    #region OperationResult
    public static OperationResult Success<T>() =>
        OperationResult.SuccessResult();

    public static OperationResult Failure(params string[] errorMessages) =>
        OperationResult.FailureResult(errorMessages);

    public static OperationResult Failure(IEnumerable<string> errorMessages) =>
        OperationResult.FailureResult(errorMessages);
    #endregion

    #region OperationResult<T>
    public static OperationResult<T> Success<T>(in T item) =>
        OperationResult<T>.SuccessResult(item);

    public static OperationResult<T> Failure<T>(params string[] errorMessages) =>
        OperationResult<T>.FailureResult(errorMessages);

    public static OperationResult<T> Failure<T>(IEnumerable<string> errorMessages) =>
        OperationResult<T>.FailureResult(errorMessages);
    #endregion
}
