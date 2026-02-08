using System.Collections.Generic;

namespace PayRemind.Contracts
{
    public class SmsConversation
    {
        public string ThreadId { get; set; }
        public string Address { get; set; } // Main address
        public string ContactName { get; set; } // If available
        public string Snippet { get; set; }
        public long Date { get; set; }
        public int UnreadCount { get; set; }
        public string PhotoUri { get; set; }
    }
}
