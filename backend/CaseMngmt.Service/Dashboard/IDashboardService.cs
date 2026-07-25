using CaseMngmt.Models.Dashboard;

namespace CaseMngmt.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryViewModel> GetSummaryAsync(Guid companyId);
    }
}
