using CaseMngmt.Models.AiMatching;

namespace CaseMngmt.Repository.AiMatching
{
    public interface IOrderRiskRepository
    {
        Task<int> SoftDeleteByOrderIdAsync(Guid orderId);
        Task<int> AddRangeAsync(List<OrderRiskLineResult> results);
        Task<List<OrderRiskLineResult>> GetByOrderIdAsync(Guid orderId);
        Task<Dictionary<Guid, string>> GetOverallRiskLevelsByOrderIdsAsync(List<Guid> orderIds);
    }
}
