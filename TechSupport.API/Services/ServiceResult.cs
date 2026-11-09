namespace TechSupport.API.Services;

public class ServiceResult<T>
{
    public bool Success { get; private init; }

    public int StatusCode { get; private init; }

    public string? Message { get; private init; }

    public T? Data { get; private init; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        StatusCode = StatusCodes.Status200OK,
        Data = data
    };

    public static ServiceResult<T> Created(T data) => new()
    {
        Success = true,
        StatusCode = StatusCodes.Status201Created,
        Data = data
    };

    public static ServiceResult<T> BadRequest(string message) => new()
    {
        StatusCode = StatusCodes.Status400BadRequest,
        Message = message
    };

    public static ServiceResult<T> NotFound(string message) => new()
    {
        StatusCode = StatusCodes.Status404NotFound,
        Message = message
    };
}
