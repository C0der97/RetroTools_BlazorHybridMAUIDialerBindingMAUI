using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayRemind.Contracts
{
    public class ContactEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string PhotoUri { get; set; }
    }

    public interface IContactsService
    {
        Task<List<ContactEntry>> GetContactsAsync();
    }
}
