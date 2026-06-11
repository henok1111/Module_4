// Program.cs
// ─────────────────────────────────────────────────────────────
// This is the entry point of the ASP.NET Core application
//
// Two sections:
//   1. Service registration  — before builder.Build()
//   2. Middleware pipeline   — after builder.Build()
//
// JavaScript/Express equivalent:
//   const app = express();
//   app.use(express.json());     ← service registration
//   app.use(cors());             ← middleware pipeline
//   app.use('/api', router);     ← endpoint mapping
//   app.listen(5000);            ← app.Run()
// ─────────────────────────────────────────────────────────────

using TmsApi.Handlers;
using TmsApi.Middleware;


using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ── SERVICE REGISTRATION ──────────────────────────────────────
// Everything here runs ONCE at startup before any request arrives
// Think of it as: "what features does this app need?"

// Enables controller-based routing
// Scans the Controllers/ folder and registers all controllers
builder.Services.AddControllers();

// Registers our TrainingAuthHandler under the scheme name "Training"
// Any request that hits a protected route goes through this handler
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);

// Enables [Authorize] attribute and .RequireAuthorization()
builder.Services.AddAuthorization();

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────
// Order is critical — read this like a chain
// Request flows TOP to BOTTOM
// Response flows BOTTOM to TOP back through the same chain
//
// Pipeline visual:
//
// Request
//   ↓  RequestLoggingMiddleware   → logs entry, sets X-Correlation-Id
//   ↓  ExceptionHandler           → catches crashes below this point
//   ↓  HttpsRedirection           → http → https
//   ↓  Routing                    → matches URL to controller/action
//   ↓  Authentication             → who is this user?
//   ↓  Authorization              → is this user allowed?
//   ↓  Controller endpoint        → runs your code
//   ↑  (response travels back up through the same chain)

// 1. Custom logging — outermost so it wraps everything
//    Captures every request before anything else touches it
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception handler — must be early to catch errors from below
//    "/error" route would be mapped to an error controller in later modules
app.UseExceptionHandler("/error");

// 3. HTTPS redirection
app.UseHttpsRedirection();

// 4. Routing — match URL to the right controller
//    Must come before auth so auth knows what endpoint is being hit
app.UseRouting();

// 5. Authentication — calls TrainingAuthHandler for every request
//    Populates HttpContext.User if the request is authenticated
app.UseAuthentication();

// 6. Authorization — checks if the authenticated user can access the route
//    [Authorize] on a controller/action is enforced here
//    Unauthenticated users get 401 at this step
app.UseAuthorization();

// 7. Map controllers — wires all controllers to the pipeline
//    Must come after auth so protection is applied
app.MapControllers();

app.Run();