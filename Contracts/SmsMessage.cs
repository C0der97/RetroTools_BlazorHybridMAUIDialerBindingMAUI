using System;

namespace PayRemind.Contracts
{
    public class SmsMessage
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public string Address { get; set; } // Phone number
        public string Body { get; set; }
        public long Date { get; set; } // Timestamp
        public bool IsIncoming { get; set; } // true = inbox, false = sent
        public bool IsRead { get; set; }
    }
}
