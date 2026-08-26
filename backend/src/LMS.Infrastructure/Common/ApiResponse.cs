namespace LMS.Infrastructure.Common;

/// <summary>
/// Uniform API response envelope. Every endpoint returns { "data": T }.
/// Error responses use ASP.NET Core ProblemDetails (RFC 7807) — never this wrapper.
/// </summary>
public sealed class ApiResponse<T>
{
    public T Data { get; init; }

    public ApiResponse(T data)
    {
        Data = data;
    }

    public static ApiResponse<T> Ok(T data) => new(data);
}
