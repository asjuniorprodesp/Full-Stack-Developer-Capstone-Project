namespace SkillSnap.Client.Services;

public sealed class ApiResult<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public int? StatusCode { get; }

    private ApiResult(bool success, T? data, string? errorMessage, int? statusCode)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ApiResult<T> Ok(T data)
    {
        return new ApiResult<T>(true, data, null, null);
    }

    public static ApiResult<T> Fail(string errorMessage, int? statusCode = null)
    {
        return new ApiResult<T>(false, default, errorMessage, statusCode);
    }
}