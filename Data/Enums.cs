namespace CentuitionApp.Data;

/// <summary>
/// Represents the type of financial account
/// </summary>
public enum AccountType
{
    Checking = 0,
    Savings = 1,
    CreditCard = 2,
    Cash = 3,
    Investment = 4,
    Loan = 5,
    Other = 6
}

/// <summary>
/// Represents the type of financial transaction
/// </summary>
public enum TransactionType
{
    Expense = 0,
    Income = 1,
    Transfer = 2
}

/// <summary>
/// Represents the frequency for recurring transactions
/// </summary>
public enum RecurrenceFrequency
{
    Daily = 0,
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Yearly = 5
}

/// <summary>
/// Represents the type of category (expense or income)
/// </summary>
public enum CategoryType
{
    Expense = 0,
    Income = 1
}
