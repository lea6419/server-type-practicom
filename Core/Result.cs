public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorMessage = null;
    }

    private Result(string errorMessage)
    {
        IsSuccess = false;
        Value = default;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Fail(string errorMessage) => new Result<T>(errorMessage);
}
