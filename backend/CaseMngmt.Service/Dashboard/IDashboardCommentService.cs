using CaseMngmt.Models.Dashboard;

namespace CaseMngmt.Service.Dashboard
{
    public interface IDashboardCommentService
    {
        Task<DashboardAiCommentViewModel?> GenerateCommentAsync(Guid companyId);
    }
}
