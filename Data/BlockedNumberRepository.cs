using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayRemind.Data
{
    public class BlockedNumberRepository
    {
        private SQLiteAsyncConnection _database;

        public BlockedNumberRepository(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<BlockedNumber>().Wait();
        }

        public Task<List<BlockedNumber>> GetBlockedNumbersAsync()
        {
            return _database.Table<BlockedNumber>().OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public Task<BlockedNumber> GetBlockedNumberAsync(string phoneNumber)
        {
            return _database.Table<BlockedNumber>().Where(x => x.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
        }

        public Task<int> SaveBlockedNumberAsync(BlockedNumber item)
        {
             return _database.InsertAsync(item);
        }

        public Task<int> DeleteBlockedNumberAsync(BlockedNumber item)
        {
            return _database.DeleteAsync(item);
        }
        
        public Task<int> DeleteBlockedNumberByPhoneAsync(string phoneNumber)
        {
             return _database.Table<BlockedNumber>().DeleteAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsBlockedAsync(string phoneNumber)
        {
             // Normalize usage? For now exact match or simple contains
             // In a real app, you'd Normalize(phoneNumber) before checking
             var blocked = await _database.Table<BlockedNumber>().Where(x => x.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
             return blocked != null;
        }
    }
}
