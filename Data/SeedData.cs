using Microsoft.EntityFrameworkCore;

namespace CentuitionApp.Data;

/// <summary>
/// Service to seed test data for development purposes
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Deletes all user data and re-seeds test data
    /// </summary>
    public static async Task ResetAndSeedTestDataAsync(ApplicationDbContext context, string userId)
    {
        // Delete all user transactions first (due to foreign key constraints)
        var userTransactions = await context.Transactions.Where(t => t.UserId == userId).ToListAsync();
        context.Transactions.RemoveRange(userTransactions);

        // Delete all user budgets
        var userBudgets = await context.Budgets.Where(b => b.UserId == userId).ToListAsync();
        context.Budgets.RemoveRange(userBudgets);

        // Delete all user accounts
        var userAccounts = await context.Accounts.Where(a => a.UserId == userId).ToListAsync();
        context.Accounts.RemoveRange(userAccounts);

        // Save deletions
        await context.SaveChangesAsync();

        // Now seed fresh data (force seed by passing forceReseed = true)
        await SeedTestDataForUserInternalAsync(context, userId);
    }

    /// <summary>
    /// Seeds budgets and transactions for a specific user for the last 12 months (relative to current date)
    /// </summary>
    public static async Task SeedTestDataForUserAsync(ApplicationDbContext context, string userId)
    {
        // Check if user already has budgets
        var existingBudgets = await context.Budgets.AnyAsync(b => b.UserId == userId);
        if (existingBudgets)
        {
            return; // Don't seed if user already has data
        }

        await SeedTestDataForUserInternalAsync(context, userId);
    }

    private static async Task SeedTestDataForUserInternalAsync(ApplicationDbContext context, string userId)
    {
        var random = new Random(42); // Fixed seed for reproducible data
        var seedDate = DateTime.UtcNow;
        var currentDate = DateTime.UtcNow;

        // Get all expense categories (system categories)
        var expenseCategories = await context.Categories
            .Where(c => c.Type == CategoryType.Expense && c.IsSystem)
            .ToListAsync();

        if (!expenseCategories.Any())
        {
            return; // No categories to create budgets for
        }

        // Add accounts if user doesn't have any
        var hasAccounts = await context.Accounts.AnyAsync(a => a.UserId == userId);
        int mainAccountId = 1;
        
        if (!hasAccounts)
        {
            var accounts = new List<Account>
            {
                new Account
                {
                    Name = "Main Checking Account",
                    Description = "Primary checking account for daily expenses",
                    AccountType = AccountType.Checking,
                    InitialBalance = 10000.00m,
                    CurrentBalance = 8500.00m,
                    Currency = "USD",
                    Color = "#1b6ec2",
                    Icon = "bank",
                    IsActive = true,
                    IncludeInTotal = true,
                    UserId = userId,
                    CreatedAt = seedDate
                },
                new Account
                {
                    Name = "High-Yield Savings",
                    Description = "Savings account with high interest rate",
                    AccountType = AccountType.Savings,
                    InitialBalance = 20000.00m,
                    CurrentBalance = 25000.00m,
                    Currency = "USD",
                    Color = "#27ae60",
                    Icon = "piggy-bank",
                    IsActive = true,
                    IncludeInTotal = true,
                    UserId = userId,
                    CreatedAt = seedDate
                },
                new Account
                {
                    Name = "Rewards Credit Card",
                    Description = "Credit card with cashback rewards",
                    AccountType = AccountType.CreditCard,
                    InitialBalance = 0m,
                    CurrentBalance = -1250.00m,
                    Currency = "USD",
                    Color = "#e74c3c",
                    Icon = "credit-card",
                    IsActive = true,
                    IncludeInTotal = true,
                    UserId = userId,
                    CreatedAt = seedDate
                },
                new Account
                {
                    Name = "401(k) Retirement",
                    Description = "Employer-sponsored retirement account",
                    AccountType = AccountType.Investment,
                    InitialBalance = 50000.00m,
                    CurrentBalance = 75000.00m,
                    Currency = "USD",
                    Color = "#9b59b6",
                    Icon = "chart-line",
                    IsActive = true,
                    IncludeInTotal = false,
                    UserId = userId,
                    CreatedAt = seedDate
                },
                new Account
                {
                    Name = "Emergency Fund",
                    Description = "3-6 months of emergency savings",
                    AccountType = AccountType.Savings,
                    InitialBalance = 10000.00m,
                    CurrentBalance = 10000.00m,
                    Currency = "USD",
                    Color = "#f39c12",
                    Icon = "shield",
                    IsActive = true,
                    IncludeInTotal = true,
                    UserId = userId,
                    CreatedAt = seedDate
                }
            };

            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();
            
            // Get the main account ID for transactions
            mainAccountId = accounts.First(a => a.AccountType == AccountType.Checking).Id;
        }
        else
        {
            // Get existing checking account
            var checkingAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.AccountType == AccountType.Checking);
            mainAccountId = checkingAccount?.Id ?? 1;
        }

        var budgets = new List<Budget>();
        var transactions = new List<Transaction>();

        // Budget names and base amounts for each category
        var budgetConfig = new Dictionary<string, (string[] names, decimal baseAmount, decimal variance)>
        {
            { "Food & Dining", (new[] { "Monthly Food Budget", "Grocery & Dining", "Food Expenses", "Meals Budget" }, 600m, 100m) },
            { "Transportation", (new[] { "Transport Budget", "Commute & Travel", "Gas & Transit", "Vehicle Expenses" }, 300m, 50m) },
            { "Housing", (new[] { "Housing Costs", "Rent & Mortgage", "Home Expenses", "Living Space" }, 1500m, 200m) },
            { "Utilities", (new[] { "Utility Bills", "Home Utilities", "Electric & Water", "Monthly Utilities" }, 200m, 50m) },
            { "Healthcare", (new[] { "Health Budget", "Medical Expenses", "Healthcare Costs", "Doctor & Medicine" }, 150m, 75m) },
            { "Entertainment", (new[] { "Fun & Games", "Entertainment Budget", "Leisure Activities", "Movies & Events" }, 200m, 50m) },
            { "Shopping", (new[] { "Shopping Budget", "Retail Purchases", "General Shopping", "Clothes & Items" }, 300m, 100m) },
            { "Education", (new[] { "Learning Budget", "Education Costs", "Books & Courses", "Self Improvement" }, 100m, 50m) },
            { "Personal Care", (new[] { "Self Care Budget", "Personal Expenses", "Grooming & Wellness", "Beauty & Health" }, 100m, 30m) },
            { "Insurance", (new[] { "Insurance Premiums", "Coverage Costs", "Protection Plan", "Monthly Insurance" }, 250m, 50m) },
            { "Subscriptions", (new[] { "Digital Services", "Subscription Costs", "Monthly Subs", "Streaming & Apps" }, 80m, 20m) },
            { "Travel", (new[] { "Vacation Fund", "Travel Budget", "Trip Expenses", "Adventure Budget" }, 400m, 200m) },
            { "Gifts & Donations", (new[] { "Giving Budget", "Gifts & Charity", "Donations", "Generosity Fund" }, 100m, 50m) },
            { "Other Expense", (new[] { "Miscellaneous", "Other Costs", "Extra Expenses", "Unexpected Items" }, 150m, 75m) }
        };

        // Transaction descriptions for expenses
        var transactionDescriptions = new Dictionary<string, string[]>
        {
            { "Food & Dining", new[] { "Walmart Grocery", "McDonald's", "Costco Food", "Starbucks Coffee", "Pizza Hut", "Subway", "Whole Foods", "Chipotle", "Trader Joe's", "Local Restaurant", "Uber Eats Order", "DoorDash Delivery", "Lunch Meeting", "Office Snacks", "Weekend Brunch" } },
            { "Transportation", new[] { "Shell Gas Station", "Chevron Fuel", "Uber Ride", "Lyft Trip", "Metro Pass", "Parking Fee", "Car Wash", "Oil Change", "Tire Rotation", "Bus Fare", "Train Ticket", "Toll Road Fee", "Airport Parking", "Car Insurance Payment" } },
            { "Housing", new[] { "Monthly Rent", "Mortgage Payment", "HOA Fees", "Property Tax", "Home Repair", "Lawn Service", "Pest Control", "Cleaning Service", "Furniture Purchase", "Home Improvement", "Appliance Repair", "Security System" } },
            { "Utilities", new[] { "Electric Bill", "Water & Sewer", "Gas Bill", "Internet Service", "Phone Bill", "Trash Collection", "Cable TV", "Solar Panel Lease" } },
            { "Healthcare", new[] { "Doctor Visit", "Pharmacy CVS", "Dental Checkup", "Eye Exam", "Lab Tests", "Specialist Visit", "Physical Therapy", "Mental Health", "Prescription Meds", "Health Supplements", "First Aid Supplies" } },
            { "Entertainment", new[] { "Movie Theater", "Concert Tickets", "Streaming Netflix", "Spotify Premium", "Video Games", "Board Games", "Sports Event", "Museum Entry", "Theme Park", "Bowling Night", "Mini Golf", "Escape Room" } },
            { "Shopping", new[] { "Amazon Purchase", "Target Shopping", "Best Buy Electronics", "Home Depot", "IKEA Furniture", "Clothing Store", "Shoe Purchase", "Office Supplies", "Kitchen Items", "Pet Supplies", "Seasonal Decor" } },
            { "Education", new[] { "Online Course", "Udemy Class", "Book Purchase", "School Supplies", "Tuition Payment", "Workshop Fee", "Conference Ticket", "Certification Exam", "Study Materials", "Language App" } },
            { "Personal Care", new[] { "Haircut", "Spa Treatment", "Gym Membership", "Skincare Products", "Makeup Purchase", "Nail Salon", "Massage Therapy", "Yoga Class", "Personal Trainer", "Vitamins" } },
            { "Insurance", new[] { "Auto Insurance", "Health Insurance", "Life Insurance", "Home Insurance", "Renters Insurance", "Dental Insurance", "Vision Insurance", "Pet Insurance", "Umbrella Policy" } },
            { "Subscriptions", new[] { "Netflix Monthly", "Disney+ Sub", "Amazon Prime", "Spotify Family", "YouTube Premium", "Adobe Creative Cloud", "Microsoft 365", "iCloud Storage", "Dropbox Plus", "VPN Service", "News Subscription" } },
            { "Travel", new[] { "Flight Tickets", "Hotel Booking", "Airbnb Stay", "Car Rental", "Travel Insurance", "Luggage Purchase", "Vacation Activities", "Resort Fee", "Cruise Deposit", "Passport Renewal", "Visa Fee" } },
            { "Gifts & Donations", new[] { "Birthday Gift", "Christmas Present", "Wedding Gift", "Charity Donation", "GoFundMe Contribution", "Church Tithe", "Anniversary Gift", "Baby Shower Gift", "Graduation Gift", "Thank You Gift" } },
            { "Other Expense", new[] { "Bank Fee", "ATM Withdrawal Fee", "Late Payment Fee", "Return Shipping", "Miscellaneous Item", "Emergency Expense", "Lost Item Replacement", "Pet Expense", "Legal Fee", "Tax Preparation" } }
        };

        // Generate list of the last 12 months (including current month)
        var monthsToGenerate = new List<(int Year, int Month)>();
        for (int i = 11; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            monthsToGenerate.Add((targetDate.Year, targetDate.Month));
        }

        // Create budgets and transactions for each of the last 12 months
        foreach (var (year, month) in monthsToGenerate)
        {
            foreach (var category in expenseCategories)
            {
                if (budgetConfig.TryGetValue(category.Name, out var config))
                {
                    // Create budget with some variation
                    var budgetAmount = config.baseAmount + (decimal)(random.NextDouble() * (double)config.variance * 2 - (double)config.variance);
                    budgetAmount = Math.Round(budgetAmount / 10) * 10; // Round to nearest 10

                    var nameIndex = (month - 1) % config.names.Length;
                    var budgetName = config.names[nameIndex];

                    var budget = new Budget
                    {
                        Name = budgetName,
                        Amount = budgetAmount,
                        SpentAmount = 0, // Will be calculated from transactions
                        Month = month,
                        Year = year,
                        CategoryId = category.Id,
                        UserId = userId,
                        IsActive = true,
                        CreatedAt = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc)
                    };

                    budgets.Add(budget);

                    // Create 2-8 transactions per category per month
                    var transactionCount = random.Next(2, 9);
                    decimal totalSpent = 0;

                    if (transactionDescriptions.TryGetValue(category.Name, out var descriptions))
                    {
                        for (int t = 0; t < transactionCount; t++)
                        {
                            // Random day in the month
                            var daysInMonth = DateTime.DaysInMonth(year, month);
                            
                            // For current month, only generate transactions up to today
                            var maxDay = (year == currentDate.Year && month == currentDate.Month) 
                                ? currentDate.Day 
                                : daysInMonth;
                            
                            if (maxDay < 1) continue;
                            
                            var day = random.Next(1, maxDay + 1);

                            // Random amount between 5% and 30% of budget
                            var transactionAmount = budgetAmount * (decimal)(0.05 + random.NextDouble() * 0.25);
                            transactionAmount = Math.Round(transactionAmount, 2);

                            // Don't exceed budget by too much (allow up to 120%)
                            if (totalSpent + transactionAmount > budgetAmount * 1.2m)
                            {
                                transactionAmount = Math.Max(0, budgetAmount * 1.2m - totalSpent);
                                if (transactionAmount < 5) continue;
                            }

                            var descIndex = random.Next(descriptions.Length);
                            var description = descriptions[descIndex];

                            var transactionDate = new DateTime(year, month, day, random.Next(8, 20), random.Next(0, 60), 0, DateTimeKind.Utc);

                            var transaction = new Transaction
                            {
                                Description = description,
                                Amount = transactionAmount,
                                Date = transactionDate,
                                Type = TransactionType.Expense,
                                CategoryId = category.Id,
                                AccountId = mainAccountId,
                                UserId = userId,
                                Notes = $"Auto-generated test transaction for {category.Name}",
                                IsReconciled = transactionDate < currentDate,
                                CreatedAt = seedDate
                            };

                            transactions.Add(transaction);
                            totalSpent += transactionAmount;
                        }
                    }

                    // Update budget's spent amount
                    budget.SpentAmount = totalSpent;
                }
            }
        }

        // Add income transactions
        var incomeCategories = await context.Categories
            .Where(c => c.Type == CategoryType.Income && c.IsSystem)
            .ToListAsync();

        var incomeDescriptions = new Dictionary<string, (string[] descriptions, decimal baseAmount, decimal variance)>
        {
            { "Salary", (new[] { "Monthly Salary", "Bi-weekly Paycheck", "Direct Deposit", "Salary Payment" }, 5000m, 500m) },
            { "Freelance", (new[] { "Freelance Project", "Consulting Work", "Side Gig Payment", "Contract Work" }, 800m, 400m) },
            { "Investments", (new[] { "Dividend Payment", "Stock Sale", "Investment Return", "Interest Income" }, 200m, 150m) },
            { "Rental Income", (new[] { "Rent Collection", "Tenant Payment", "Property Income" }, 1200m, 200m) },
            { "Other Income", (new[] { "Cashback Reward", "Tax Refund", "Gift Received", "Rebate" }, 100m, 80m) }
        };

        foreach (var (year, month) in monthsToGenerate)
        {
            foreach (var category in incomeCategories)
            {
                if (incomeDescriptions.TryGetValue(category.Name, out var config))
                {
                    // 1-3 income transactions per category per month
                    var transactionCount = category.Name == "Salary" ? 2 : random.Next(0, 3);

                    for (int t = 0; t < transactionCount; t++)
                    {
                        var daysInMonth = DateTime.DaysInMonth(year, month);
                        
                        // For current month, limit to today's date
                        var maxDay = (year == currentDate.Year && month == currentDate.Month) 
                            ? currentDate.Day 
                            : daysInMonth;
                        
                        int day;
                        if (category.Name == "Salary")
                        {
                            day = t == 0 ? 1 : 15;
                            // Skip salary payment if day hasn't occurred yet in current month
                            if (day > maxDay) continue;
                        }
                        else
                        {
                            if (maxDay < 1) continue;
                            day = random.Next(1, maxDay + 1);
                        }

                        var amount = config.baseAmount + (decimal)(random.NextDouble() * (double)config.variance * 2 - (double)config.variance);
                        amount = Math.Round(amount, 2);

                        var descIndex = random.Next(config.descriptions.Length);

                        var transactionDate = new DateTime(year, month, day, 9, 0, 0, DateTimeKind.Utc);

                        var transaction = new Transaction
                        {
                            Description = config.descriptions[descIndex],
                            Amount = amount,
                            Date = transactionDate,
                            Type = TransactionType.Income,
                            CategoryId = category.Id,
                            AccountId = mainAccountId,
                            UserId = userId,
                            Notes = $"Auto-generated test income for {category.Name}",
                            IsReconciled = transactionDate < currentDate,
                            CreatedAt = seedDate
                        };

                        transactions.Add(transaction);
                    }
                }
            }
        }

        // Save budgets and transactions
        context.Budgets.AddRange(budgets);
        context.Transactions.AddRange(transactions);

        await context.SaveChangesAsync();
    }
}
