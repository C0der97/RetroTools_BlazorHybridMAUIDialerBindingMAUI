using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayRemind.Contracts
{
    public class CallLogEntry
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public long Date { get; set; }
        public string Type { get; set; } // Incoming, Outgoing, Missed
    }

    public interface ICallLogService
    {
        Task<List<CallLogEntry>> GetCallLogsAsync();
    }
}
