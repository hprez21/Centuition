# Centuition - Personal Finance Management App

![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![Blazor](https://img.shields.io/badge/Blazor-Server-purple)
![Telerik](https://img.shields.io/badge/Telerik%20UI-12.0.0-green)

**Centuition** is a personal finance management application built with Blazor Server and Telerik UI for Blazor components. It allows users to manage accounts, transactions, budgets, and generate financial reports with AI-powered analysis.

## 📋 Features

- 💰 **Account Management**: Create and manage multiple accounts (checking, savings, cash, credit cards, investments)
- 📊 **Financial Dashboard**: Overview of balances, income, expenses, and savings
- 💳 **Transactions**: Track and manage income and expenses
- 📈 **Budgets**: Create and monitor budgets by category
- 📑 **Reports**: Financial analysis with interactive charts
- 🤖 **AI Analysis**: Financial summaries and recommendations using OpenAI
- 🔐 **Authentication**: Account system with Identity including Passkeys support
- 🎨 **Modern UI**: Interface built with Telerik UI for Blazor

## 🛠️ Prerequisites

### Required Software

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) or [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) or [VS Code](https://code.visualstudio.com/) with C# Dev Kit extension
- [Telerik UI for Blazor License](https://www.telerik.com/blazor-ui) (Trial or Commercial)

### Accounts and APIs

- **OpenAI API Key** (optional, for AI features): [Get API Key](https://platform.openai.com/api-keys)
- **Azure OpenAI** (alternative): Configure Azure OpenAI endpoint and key

## ⚙️ Configuration

### 1. Clone the Repository

```bash
git clone https://github.com/hprez21/Centuition
cd CentuitionApp
```

### 2. Configure the Connection String

The application looks for the connection string in this order:
1. **Environment Variable** `CENTUITION_CONNECTION_STRING` (Recommended)
2. **User Secrets** (Recommended for development)
3. **appsettings.json** (Not recommended - avoid committing secrets)

**Option A: Environment Variable (Recommended)**

```bash
# Windows PowerShell (current session)
$env:CENTUITION_CONNECTION_STRING = "Server=(localdb)\MSSQLLocalDB;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true"

# Windows - Set permanently for user
[Environment]::SetEnvironmentVariable("CENTUITION_CONNECTION_STRING", "Server=(localdb)\MSSQLLocalDB;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true", "User")

# Linux/macOS
export CENTUITION_CONNECTION_STRING="Server=localhost;Database=CentuitionDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

**Option B: User Secrets (Recommended for development)**

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true"
```

**Option C: appsettings.json** (⚠️ Not recommended - will be committed to source control)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Connection string examples:**

- **LocalDB (local development)**:
  ```
  Server=(localdb)\MSSQLLocalDB;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true
  ```

- **SQL Server Express**:
  ```
  Server=.\SQLEXPRESS;Database=CentuitionDB;Trusted_Connection=True;MultipleActiveResultSets=true
  ```

- **SQL Server with authentication**:
  ```
  Server=your-server;Database=CentuitionDB;User Id=your-username;Password=your-password;TrustServerCertificate=True
  ```

### 3. Configure AI Service (Optional)

The application supports both **Azure OpenAI** and **OpenAI** for AI-powered financial analysis. By default, Azure OpenAI is enabled.

#### Option A: Azure OpenAI (Default)

1. **Update the endpoint and deployment name** in `Program.cs`:

```csharp
builder.Services
    .AddChatClient(new AzureOpenAIClient(
         new Uri("your-azure-endpoint"),  // Replace with your Azure OpenAI endpoint (e.g., https://your-resource.openai.azure.com/)
        new ApiKeyCredential(key))
    .GetChatClient("your-deployment-name").AsIChatClient());  // Replace with your deployment name (e.g., gpt-4o-mini)
```

2. **Configure the API Key** using User Secrets (recommended):

```bash
dotnet user-secrets set "AzureOPENAI:Key" "your-azure-openai-key"
```

Or using environment variables:

```bash
# Windows PowerShell
$env:AzureOPENAI__Key = "your-azure-openai-key"

# Linux/macOS
export AzureOPENAI__Key="your-azure-openai-key"
```

#### Option B: OpenAI

1. **Uncomment the OpenAI section** in `Program.cs` and comment out the Azure OpenAI section:

```csharp
//Azure OpenAI Client Configuration - Comment this section
//var key = builder.Configuration["AzureOPENAI:Key"];
//...

//OpenAI Client Configuration - Uncomment this section
var key = builder.Configuration["OPENAI_API_KEY"];
if (!string.IsNullOrEmpty(key))
{
    builder.Services
        .AddChatClient(new OpenAIClient(
            new ApiKeyCredential(key))
        .GetChatClient("openai-model").AsIChatClient());  // Replace with model name (e.g., gpt-4o-mini, gpt-4o)
}
```

2. **Configure the API Key**:

```bash
dotnet user-secrets set "OPENAI_API_KEY" "your-openai-api-key"
```

Or using environment variables:

```bash
# Windows PowerShell
$env:OPENAI_API_KEY = "your-openai-api-key"

# Linux/macOS
export OPENAI_API_KEY="your-openai-api-key"
```

> **Note**: If you don't configure an AI service, the application will still work but AI-powered features (financial summaries, recommendations) will not be available.

### 4. Configure Telerik UI for Blazor

To restore Telerik packages and configure the project, follow the official documentation:

👉 [Getting Started with Telerik UI for Blazor](https://www.telerik.com/blazor-ui/documentation/getting-started/web-app)

The guide covers:
- Setting up the Telerik NuGet feed
- Installing the required packages
- Adding the necessary namespaces and services

### 5. Apply Database Migrations

```bash
dotnet ef database update
```

If you don't have EF Tools installed:

```bash
dotnet tool install --global dotnet-ef
```

### 6. Run the Application

```bash
dotnet run
```

The application will be available at: `https://localhost:5001` or `http://localhost:5000`

## 🚀 Getting Started

### 1. Create a User Account

1. Navigate to the application in your browser
2. Click on **"Create one"** or go to `/Account/Register`
3. Enter your email and password
4. Confirm your account (in development, confirmation is automatic)

### 2. Load Test Data

Once you have logged in:

1. You will be redirected to the **Dashboard** (`/finance/dashboard`)
2. In the top right corner, you will find the **"Reset Test Data"** button (⟳)
3. Click this button to populate the database with sample data:
   - Sample accounts (Checking, Savings, Credit Card, etc.)
   - Transactions from the last 12 months
   - Budgets configured by category
   - Predefined income and expense categories

> ⚠️ **Important**: The "Reset Test Data" button **will delete all existing user data** and replace it with fresh test data.

### 3. Explore the Application

- **Dashboard**: Overview of your finances with charts and metrics
- **Transactions**: List and manage all transactions
- **Accounts**: Manage financial accounts
- **Budgets**: Create and monitor monthly budgets
- **Categories**: Customize income/expense categories
- **Reports**: Detailed analysis with charts and export options

## 📁 Project Structure

```
CentuitionApp/
├── Components/
│   ├── Account/          # Authentication pages (Login, Register, etc.)
│   ├── Icons/            # SVG icon components
│   ├── Layout/           # Layouts and navigation
│   ├── Pages/            # Main pages
│   │   └── Finance/      # Finance module (Dashboard, Transactions, etc.)
│   └── Shared/           # Shared components
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── SeedData.cs       # Test data generator
│   └── Migrations/       # EF Core migrations
├── Models/               # View models
├── Services/             # Business services
├── wwwroot/              # Static files
├── appsettings.json      # Configuration
└── Program.cs            # Entry point
```

## 🔧 Additional Configuration

### Change the AI Model

In `Program.cs`, update the model/deployment name:

```csharp
// Azure OpenAI - use your deployment name
.GetChatClient("your-deployment-name")

// OpenAI - use the model name
.GetChatClient("gpt-4o-mini")   // Faster, cheaper
.GetChatClient("gpt-4o")        // More capable
.GetChatClient("gpt-3.5-turbo") // Legacy
```

### Configure Culture/Language

The application is configured to use en-US format. To change:

```csharp
// In Program.cs
var cultureInfo = new CultureInfo("es-ES"); // Spanish
```

## 🐛 Troubleshooting

### Error: "Connection string 'DefaultConnection' not found"
- Verify that `appsettings.json` has the correct connection string

### Error: "Unable to resolve service for type 'IChatClient'"
- Configure the OpenAI API Key or comment out the AI-related code in `Program.cs`

### Telerik package errors
- Verify that you have configured the Telerik NuGet feed
- Make sure you have a valid license (trial or commercial)

### Database is not created
```bash
dotnet ef database update --verbose
```

### Migration errors
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📜 License

This project is for educational and demonstration purposes.

## 🤝 Contributing

Contributions are welcome. Please open an issue first to discuss proposed changes.

---

**Built with** ❤️ **using Blazor and Telerik UI for Blazor**
