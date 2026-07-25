using CaseMngmt.Models.Chat;

namespace CaseMngmt.Service.Chat
{
    public interface IChatAssistantService
    {
        Task<string> AskAsync(Guid companyId, string message, List<ChatHistoryTurn> history);
    }
}
