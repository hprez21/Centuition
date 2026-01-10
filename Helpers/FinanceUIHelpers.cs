using CentuitionApp.Data;
using CentuitionApp.Services;
using Telerik.Blazor;
using Telerik.SvgIcons;

namespace CentuitionApp.Helpers;

/// <summary>
/// Centralized UI helper methods for finance-related components.
/// Contains icon mappings, badge colors, and styling utilities.
/// </summary>
public static class FinanceUIHelpers
{
    #region Account Helpers

    /// <summary>
    /// Gets the appropriate SVG icon for an account type.
    /// </summary>
    public static ISvgIcon GetAccountIcon(AccountType type) => type switch
    {
        AccountType.Checking => SvgIcon.Calculator,
        AccountType.Savings => SvgIcon.Dollar,
        AccountType.CreditCard => SvgIcon.ApplyFormat,
        AccountType.Cash => SvgIcon.Dollar,
        AccountType.Investment => SvgIcon.ChartLineMarkers,
        AccountType.Loan => SvgIcon.FileData,
        _ => SvgIcon.Folder
    };

    /// <summary>
    /// Gets the Telerik badge theme color for an account type.
    /// </summary>
    public static string GetAccountTypeBadgeColor(AccountType type) => type switch
    {
        AccountType.Checking => ThemeConstants.Badge.ThemeColor.Primary,
        AccountType.Savings => ThemeConstants.Badge.ThemeColor.Success,
        AccountType.CreditCard => ThemeConstants.Badge.ThemeColor.Warning,
        AccountType.Cash => ThemeConstants.Badge.ThemeColor.Info,
        AccountType.Investment => ThemeConstants.Badge.ThemeColor.Tertiary,
        AccountType.Loan => ThemeConstants.Badge.ThemeColor.Error,
        _ => ThemeConstants.Badge.ThemeColor.Light
    };

    /// <summary>
    /// Gets the Bootstrap badge CSS class for an account type.
    /// </summary>
    public static string GetAccountTypeBadgeClass(AccountType type) => type switch
    {
        AccountType.Checking => "bg-primary",
        AccountType.Savings => "bg-success",
        AccountType.CreditCard => "bg-warning text-dark",
        AccountType.Cash => "bg-info text-dark",
        AccountType.Investment => "bg-secondary",
        AccountType.Loan => "bg-danger",
        _ => "bg-light text-dark"
    };

    #endregion

    #region Transaction Helpers

    /// <summary>
    /// Gets the Telerik badge theme color for a transaction type.
    /// </summary>
    public static string GetTransactionTypeBadgeColor(TransactionType type) => type switch
    {
        TransactionType.Income => ThemeConstants.Badge.ThemeColor.Success,
        TransactionType.Expense => ThemeConstants.Badge.ThemeColor.Error,
        TransactionType.Transfer => ThemeConstants.Badge.ThemeColor.Info,
        _ => ThemeConstants.Badge.ThemeColor.Light
    };

    /// <summary>
    /// Gets the Bootstrap badge CSS class for a transaction type.
    /// </summary>
    public static string GetTransactionTypeBadgeClass(TransactionType type) => type switch
    {
        TransactionType.Income => "bg-success",
        TransactionType.Expense => "bg-danger",
        TransactionType.Transfer => "bg-info",
        _ => "bg-light text-dark"
    };

    /// <summary>
    /// Gets the appropriate SVG icon for a transaction type.
    /// </summary>
    public static ISvgIcon GetTransactionTypeIcon(TransactionType type) => type switch
    {
        TransactionType.Income => SvgIcon.ArrowUp,
        TransactionType.Expense => SvgIcon.ArrowDown,
        TransactionType.Transfer => SvgIcon.ArrowsSwap,
        _ => SvgIcon.Gear
    };

    #endregion

    #region Budget Helpers

    /// <summary>
    /// Gets the Telerik badge theme color based on budget progress status.
    /// </summary>
    public static string GetBudgetStatusBadgeColor(BudgetProgress progress)
    {
        if (progress.IsOverBudget) return ThemeConstants.Badge.ThemeColor.Error;
        if (progress.PercentageUsed >= 80) return ThemeConstants.Badge.ThemeColor.Warning;
        return ThemeConstants.Badge.ThemeColor.Success;
    }

    /// <summary>
    /// Gets the CSS class for the progress bar based on budget progress.
    /// </summary>
    public static string GetBudgetProgressBarClass(BudgetProgress progress)
    {
        if (progress.IsOverBudget) return "progress-danger";
        if (progress.PercentageUsed >= 80) return "progress-warning";
        return "";
    }

    #endregion

    #region Category Helpers

    /// <summary>
    /// Gets the appropriate SVG icon for a category type.
    /// </summary>
    public static ISvgIcon GetCategoryTypeIcon(CategoryType type) => type switch
    {
        CategoryType.Income => SvgIcon.ArrowUp,
        CategoryType.Expense => SvgIcon.ArrowDown,
        _ => SvgIcon.Folder
    };

    /// <summary>
    /// Gets the Telerik badge theme color for a category type.
    /// </summary>
    public static string GetCategoryTypeBadgeColor(CategoryType type) => type switch
    {
        CategoryType.Income => ThemeConstants.Badge.ThemeColor.Success,
        CategoryType.Expense => ThemeConstants.Badge.ThemeColor.Error,
        _ => ThemeConstants.Badge.ThemeColor.Light
    };

    #endregion

    #region Common Styling Helpers

    /// <summary>
    /// Gets the CSS class for positive/negative values.
    /// </summary>
    public static string GetValueColorClass(decimal value) => value >= 0 ? "text-success" : "text-danger";

    /// <summary>
    /// Gets the sign prefix for displaying transaction amounts.
    /// </summary>
    public static string GetAmountSignPrefix(TransactionType type) => type switch
    {
        TransactionType.Income => "+",
        TransactionType.Expense => "-",
        _ => ""
    };

    /// <summary>
    /// Default color used for accounts when none is specified.
    /// </summary>
    public const string DefaultAccountColor = "#1b6ec2";

    #endregion
}
