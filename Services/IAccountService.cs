using CentuitionApp.Data;

namespace CentuitionApp.Services;

/// <summary>
/// Service interface for managing financial accounts
/// </summary>
public interface IAccountService
{
    Task<List<Account>> GetAllAccountsAsync(string userId);
    Task<Account?> GetAccountByIdAsync(int id, string userId);
    Task<Account> CreateAccountAsync(Account account);
    Task<Account> UpdateAccountAsync(Account account);
    Task<bool> DeleteAccountAsync(int id, string userId);
    Task<decimal> GetTotalBalanceAsync(string userId);
    Task<Dictionary<AccountType, decimal>> GetBalancesByTypeAsync(string userId);
    Task UpdateAccountBalanceAsync(int accountId, decimal amount, bool isAddition);
}
