using CaseMngmt.Models.AiMatching;

namespace CaseMngmt.Service.AiMatching
{
    public interface IAiMatchingService
    {
        Task<OrderRiskSummaryViewModel?> RunMatchingAsync(Guid orderId, Guid companyId, Guid currentUserId);
        Task<OrderRiskSummaryViewModel?> GetLatestAsync(Guid orderId, Guid companyId);
    }
}
