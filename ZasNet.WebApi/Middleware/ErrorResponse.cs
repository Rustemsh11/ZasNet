namespace ZasNet.WebApi.Middleware;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int StatusCode { get; set; }
    public string? StackTrace { get; set; }
}

