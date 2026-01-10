using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing financial accounts
/// </summary>
public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;

    public AccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAccountsAsync(string userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(int id, string userId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {        
        var existingAccount = await _context.Accounts
            .AnyAsync(a => a.UserId == account.UserId && a.Name == account.Name);
        
        if (existingAccount)
        {
            throw new InvalidOperationException($"An account with the name '{account.Name}' already exists. Please use a different name.");
        }
        
        account.CreatedAt = DateTime.UtcNow;
        account.CurrentBalance = account.InitialBalance;
        
        try
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                                            ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException($"An account with the name '{account.Name}' already exists. Please use a different name.");
        }
        
        return account;
    }

    public async Task<Account> UpdateAccountAsync(Account account)
    {
        var existing = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == account.Id && a.UserId == account.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Account not found.");
        }
        
        var duplicateName = await _context.Accounts
            .AnyAsync(a => a.UserId == account.UserId && a.Name == account.Name && a.Id != account.Id);
        
        if (duplicateName)
        {
            throw new InvalidOperationException($"Another account with the name '{account.Name}' already exists. Please use a different name.");
        }

        existing.Name = account.Name;
        existing.Description = account.Description;
        existing.AccountType = account.AccountType;
        existing.Currency = account.Currency;
        existing.Color = account.Color;
        existing.Icon = account.Icon;
        existing.IsActive = account.IsActive;
        existing.IncludeInTotal = account.IncludeInTotal;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true ||
                                            ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException($"Another account with the name '{account.Name}' already exists. Please use a different name.");
        }
        
        return existing;
    }

    public async Task<bool> DeleteAccountAsync(int id, string userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
        {
            return false;
        }
        
        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.AccountId == id || t.DestinationAccountId == id);

        if (hasTransactions)
        {            
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Accounts.Remove(account);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalBalanceAsync(string userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotal)
            .SumAsync(a => a.CurrentBalance);
    }

    public async Task<Dictionary<AccountType, decimal>> GetBalancesByTypeAsync(string userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .GroupBy(a => a.AccountType)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(a => a.CurrentBalance));
    }

    public async Task UpdateAccountBalanceAsync(int accountId, decimal amount, bool isAddition)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        
        if (account != null)
        {
            if (isAddition)
            {
                account.CurrentBalance += amount;
            }
            else
            {
                account.CurrentBalance -= amount;
            }
            
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
