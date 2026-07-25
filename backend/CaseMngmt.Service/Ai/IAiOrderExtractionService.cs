using CaseMngmt.Models.Orders;

namespace CaseMngmt.Service.Ai
{
    public interface IAiOrderExtractionService
    {
        Task<OrderExtractionResult?> ExtractAsync(byte[] fileBytes, string mediaType, Guid companyId);
    }
}
