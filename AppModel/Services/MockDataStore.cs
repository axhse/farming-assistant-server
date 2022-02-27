using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
    public class MockDataStore : IDataStore<Account>
    {
        private readonly List<Account> _accounts;

        public MockDataStore()
        {
            _accounts = new() { new Account() };
        }

        public async Task<bool> AddItemAsync(Account account)
        {
            _accounts.Add(account);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Account account)
        {
            var oldAccount = _accounts.Where(item => item.CustomerInfo.Username == account.CustomerInfo.Username).FirstOrDefault();
            _accounts.Remove(oldAccount);
            _accounts.Add(account);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string username)
        {
            var oldAccount = _accounts.Where((Account item) => item.CustomerInfo.Username == username).FirstOrDefault();
            _accounts.Remove(oldAccount);
            return await Task.FromResult(true);
        }

        public async Task<Account> GetItemAsync(string username)
        {
            return await Task.FromResult(_accounts.FirstOrDefault(item => item.CustomerInfo.Username == username));
        }

        public async Task<IEnumerable<Account>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(_accounts);
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                List<AccountInfo> accountInfoList = new();
                foreach (Account account in _accounts)
                {
                    accountInfoList.Add(account.ConvertToAccountInfo());
                }
                string data = JsonSerializer.Serialize(accountInfoList);
                File.WriteAllText(StaticSettings.AccountDataPath, data);
                return await Task.FromResult(true);
            }
            catch { }
            return await Task.FromResult(false);
        }

        public async Task<bool> LoadAsync()
        {
            try
            {
                string data = File.ReadAllText(StaticSettings.AccountDataPath);
                List<AccountInfo> accountInfoList = JsonSerializer.Deserialize<List<AccountInfo>>(data);
                if (accountInfoList.Count != 0)
                {
                    _accounts.Clear();
                    foreach (AccountInfo accountInfo in accountInfoList)
                    {
                        _accounts.Add(Account.ConvertFromAccountInfo(accountInfo));
                    }
                    return await Task.FromResult(true);
                }
            }
            catch { }
            return await Task.FromResult(false);
        }
    }
}