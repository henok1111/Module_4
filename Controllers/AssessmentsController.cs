// Controllers/AssessmentsController.cs
// ─────────────────────────────────────────────────────────────
// This is a controller-based endpoint
// The document used a minimal API (MapGet) but since we used
// --use-controllers flag when creating the project, we use controllers
//
// JavaScript/Express equivalent:
//   const router = express.Router();
//   router.get('/results', authMiddleware, (req, res) => {
//       res.json({ courseCode: 'CS-101', ... });
//   });
//
// [Authorize] on the controller = every route inside needs auth
// [ApiController] = enables automatic model validation + routing
// [Route("api/[controller]")] = base URL is /api/assessments
// ─────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

// [Authorize] means every endpoint in this controller
// requires an authenticated user
// Unauthenticated requests get 401 before the method even runs
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    private readonly ILogger<AssessmentsController> _logger;

    // ASP.NET Core injects ILogger automatically
    public AssessmentsController(ILogger<AssessmentsController> logger)
    {
        _logger = logger;
    }

    // GET /api/assessments/results
    // Returns a placeholder assessment result
    // Protected by [Authorize] on the class — no anonymous access
    [HttpGet("results")]
    public IActionResult GetResults()
    {
        // Log who is accessing this endpoint
        // User.Identity.Name comes from the claim we set in TrainingAuthHandler
        var username = User.Identity?.Name ?? "unknown";
        _logger.LogInformation(
            "Assessment results accessed by {Username}", username);

        // Return placeholder data — real data comes in later modules
        var result = new AssessmentResult("CS-101", "S-001", "A");
        return Ok(result);
    }

    // GET /api/assessments
    // Returns a list of placeholder assessments
    // Also protected by [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        var username = User.Identity?.Name ?? "unknown";
        _logger.LogInformation(
            "All assessments accessed by {Username}", username);

        // Placeholder list — real data comes in later modules
        var results = new List<AssessmentResult>
        {
            new AssessmentResult("CS-101", "S-001", "A"),
            new AssessmentResult("CS-102", "S-002", "B"),
            new AssessmentResult("CS-103", "S-003", "A+")
        };

        return Ok(results);
    }
}
