using CentuitionApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Services;

/// <summary>
/// Service implementation for managing financial accounts
/// </summary>
public class AccountService : IAccountService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AccountService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Account>> GetAllAccountsAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(int id, string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {        
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingAccount = await context.Accounts
            .AnyAsync(a => a.UserId == account.UserId && a.Name == account.Name);
        
        if (existingAccount)
        {
            throw new InvalidOperationException($"An account with the name '{account.Name}' already exists. Please use a different name.");
        }
        
        account.CreatedAt = DateTime.UtcNow;
        account.CurrentBalance = account.InitialBalance;
        
        try
        {
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == account.Id && a.UserId == account.UserId);

        if (existing == null)
        {
            throw new InvalidOperationException("Account not found.");
        }
        
        var duplicateName = await context.Accounts
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
            await context.SaveChangesAsync();
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
        await using var context = await _contextFactory.CreateDbContextAsync();
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
        {
            return false;
        }
        
        var hasTransactions = await context.Transactions
            .AnyAsync(t => t.AccountId == id || t.DestinationAccountId == id);

        if (hasTransactions)
        {            
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.Accounts.Remove(account);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalBalanceAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Accounts
            .Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotal)
            .SumAsync(a => a.CurrentBalance);
    }

    public async Task<Dictionary<AccountType, decimal>> GetBalancesByTypeAsync(string userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .GroupBy(a => a.AccountType)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(a => a.CurrentBalance));
    }

    public async Task UpdateAccountBalanceAsync(int accountId, decimal amount, bool isAddition)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var account = await context.Accounts.FindAsync(accountId);
        
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
            await context.SaveChangesAsync();
        }
    }
}
