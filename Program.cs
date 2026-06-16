using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ── Session 1: Authentication ──────────────────────────────────────
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

// ── Session 2: DI lifetime validation ─────────────────────────────
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// ── Session 2: Service registrations ──────────────────────────────
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// ── Session 2: Options pattern ─────────────────────────────────────
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ── Session 3: Controllers + ProblemDetails + OpenAPI ──────────────
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ──────────────────────────────────────────────────────────────────
var app = builder.Build();
// ──────────────────────────────────────────────────────────────────

// ── Session 1: Logging middleware — must be outermost ──────────────
app.UseMiddleware<RequestLoggingMiddleware>();

// ── Session 3: Environment toggle ─────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Session 3: Wire all controllers ───────────────────────────────
app.MapControllers();

// ── Session 1: Protected minimal API route ─────────────────────────
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode  = "CS-101",
    studentId   = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

// ── Session 2: Smoke test ──────────────────────────────────────────
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// ── Session 3: Test error route ────────────────────────────────────
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.Run();