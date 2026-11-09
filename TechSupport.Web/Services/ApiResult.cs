namespace TechSupport.Web.Services;

public class ApiResult<T>
{
    public bool Success { get; private init; }

    public int StatusCode { get; private init; }

    public string? Message { get; private init; }

    public T? Data { get; private init; }

    public static ApiResult<T> Ok(T? data) => new()
    {
        Success = true,
        StatusCode = StatusCodes.Status200OK,
        Data = data
    };

    public static ApiResult<T> Fail(int statusCode, string message) => new()
    {
        StatusCode = statusCode,
        Message = message
    };
}
