using System.Net;
namespace RaceDay.Web.Infrastructure;

public class ApiResult {
    public bool Success { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? Error { get; init; }
    public static ApiResult Ok(HttpStatusCode status) => new() { Success = true, StatusCode = status };
    public static ApiResult Fail(string error, HttpStatusCode? status = null) => new() { Success = false, Error = error, StatusCode = status };
}

public class ApiResult<T> : ApiResult {
    public T? Data { get; init; }
    public static ApiResult<T> Ok(T data, HttpStatusCode status) => new() { Success = true, Data = data, StatusCode = status };
    public new static ApiResult<T> Fail(string error, HttpStatusCode? status = null) => new() { Success = false, Error = error, StatusCode = status };
}

/// <summary>Thrown when the API rejects the stored token (expired / invalid). Handled globally by <see cref="SessionExpiredFilter"/>.</summary>
public class SessionExpiredException : Exception {
    public SessionExpiredException() : base("The session has expired.") { }
}
