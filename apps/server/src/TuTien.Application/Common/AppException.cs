namespace TuTien.Application.Common;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public AppException(string code, string message, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public sealed record ApiError(string Code, string Message);
