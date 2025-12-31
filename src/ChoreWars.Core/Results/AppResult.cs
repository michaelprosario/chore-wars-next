namespace ChoreWars.Core.Results;

public class AppResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<ValidationError> ValidationErrors { get; set; } = new();
    public string? Message { get; set; }

    public static AppResult<T> Success(T data, string? message = null)
    {
        return new AppResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static AppResult<T> Failure(string error)
    {
        return new AppResult<T>
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    }

    public static AppResult<T> Failure(List<string> errors)
    {
        return new AppResult<T>
        {
            IsSuccess = false,
            Errors = errors
        };
    }

    public static AppResult<T> ValidationFailure(List<ValidationError> validationErrors)
    {
        return new AppResult<T>
        {
            IsSuccess = false,
            ValidationErrors = validationErrors
        };
    }
}

public class ValidationError
{
    public required string Field { get; set; }
    public required string Message { get; set; }
}
