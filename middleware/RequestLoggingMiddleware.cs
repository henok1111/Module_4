// Middleware/RequestLoggingMiddleware.cs
// ─────────────────────────────────────────────────────────────
// Custom middleware that runs on EVERY request
//
// What it does:
//   1. Generates a unique correlation ID per request
//   2. Attaches X-Correlation-Id to the response headers
//   3. Logs request entry with method + path + correlation ID
//   4. Passes control down the pipeline
//   5. Logs response exit with status code + elapsed time + same ID
//
// JavaScript/Express equivalent:
//   app.use((req, res, next) => {
//       const id = generateId();
//       res.setHeader('X-Correlation-Id', id);
//       console.log(`→ ${req.method} ${req.path} id=${id}`);
//       next();
//       console.log(`← ${res.statusCode} id=${id}`);
//   });
// ─────────────────────────────────────────────────────────────

using System.Diagnostics;

namespace TmsApi.Middleware;

public class RequestLoggingMiddleware
{
    // _next is the rest of the pipeline after this middleware
    // Calling await _next(context) passes control forward
    // Same as calling next() in Express
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ── BEFORE passing to next middleware ─────────────────

        // Generate a short unique ID for this specific request
        // Guid.NewGuid()       → globally unique ID: a3f9b2c1-4d5e-...
        // .ToString("N")       → removes dashes: a3f9b2c14d5e...
        // [..8]                → first 8 chars only: a3f9b2c1
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // IMPORTANT: set the header BEFORE await _next(context)
        // Once the response starts, headers are locked
        // If you set it after next(), it will be ignored on many responses
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var method = context.Request.Method; // GET, POST, PUT, DELETE
        var path   = context.Request.Path;   // /api/assessments/results

        // Start measuring time before the pipeline runs
        var stopwatch = Stopwatch.StartNew();

        // Log the incoming request — entry line
        _logger.LogInformation(
            "→ Request  {Method} {Path} | id={CorrelationId}",
            method, path, correlationId);

        // ── Hand off to the next middleware ───────────────────
        // The entire rest of the pipeline runs here
        // Auth, authorization, endpoint handler — all of it
        // When they finish, execution returns here
        await _next(context);

        // ── AFTER the full pipeline has completed ─────────────

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode; // 200, 401, 404...
        var elapsed    = stopwatch.ElapsedMilliseconds;

        // Log the completed response — exit line
        // Same correlationId ties entry and exit together in the logs
        _logger.LogInformation(
            "← Response {StatusCode} | {ElapsedMs}ms | id={CorrelationId}",
            statusCode, elapsed, correlationId);
    }
}