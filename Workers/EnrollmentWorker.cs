using TmsApi.Services;
namespace TmsApi.Workers;

public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        var all = svc.GetAllAsync().Result;
    }
}