using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogInformation(
            "➡ {Method} {Path} [{CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        var sw = Stopwatch.StartNew();

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "⬅ {StatusCode} {Path} [{CorrelationId}] {ElapsedMs}ms",
            context.Response.StatusCode,
            context.Request.Path,
            correlationId,
            sw.ElapsedMilliseconds);
    }
}