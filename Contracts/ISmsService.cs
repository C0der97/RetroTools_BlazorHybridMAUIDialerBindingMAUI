using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayRemind.Contracts
{
    public interface ISmsService
    {
        Task<List<SmsConversation>> GetConversationsAsync();
        Task<List<SmsMessage>> GetMessagesAsync(string threadId);
        Task<bool> SendMessageAsync(string address, string body);
        Task<bool> DeleteMessageAsync(string messageId);
        Task<bool> DeleteConversationAsync(string threadId);
        Task MarkAsReadAsync(string threadId);
    }
}
