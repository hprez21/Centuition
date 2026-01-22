using Azure.AI.OpenAI;
using CentuitionApp.Components;
using CentuitionApp.Components.Account;
using CentuitionApp.Data;
using CentuitionApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using System.Diagnostics;
using System.Globalization;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Configure default culture to en-US
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Connection string priority: Environment Variable > User Secrets > appsettings.json
var connectionString = Environment.GetEnvironmentVariable("CENTUITION_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found. Set 'CENTUITION_CONNECTION_STRING' environment variable or configure 'DefaultConnection' in appsettings.json or user secrets.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Register finance services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddScoped<FinancialTools>();
builder.Services.AddScoped<IFinancialAssistantService, FinancialAssistantService>();

builder.Services.AddTelerikBlazor();

// Configure request localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

//Azure OpenAI Client Configuration
var key = builder.Configuration["AzureOPENAI:Key"]
    ?? throw new InvalidOperationException("AzureOPENAI:Key not configured. See the README.md section '3. Configure AI Service' for setup instructions.");
var rawEndpoint = builder.Configuration["AzureOPENAI:Endpoint"]
    ?? throw new InvalidOperationException("AzureOPENAI:Endpoint not configured. See the README.md section '3. Configure AI Service' for setup instructions.");
var deploymentName = builder.Configuration["AzureOPENAI:DeploymentName"]
    ?? throw new InvalidOperationException("AzureOPENAI:DeploymentName not configured. See the README.md section '3. Configure AI Service' for setup instructions.");
var endpoint = NormalizeAzureOpenAIEndpoint(rawEndpoint);
builder.Logging.AddFilter("Azure.AI.OpenAI", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Extensions.AI", LogLevel.Warning);
builder.Services.AddChatClient(services =>
{
    var innerClient = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(key))
        .GetChatClient(deploymentName)
        .AsIChatClient();

    return innerClient
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();
});

//OpenAI Client Configuration
//var key = builder.Configuration["OPENAI_API_KEY"]
//    ?? throw new InvalidOperationException("OPENAI_API_KEY not configured. See the README.md section '3. Configure AI Service' for setup instructions.");

//var openAIModel = builder.Configuration["OPENAI_MODEL"] ?? "gpt-4o";

//builder.Services.AddChatClient(services =>
//{
//    var innerClient = new OpenAIClient(new ApiKeyCredential(key))
//        .GetChatClient(openAIModel)
//        .AsIChatClient();

//    return innerClient
//        .AsBuilder()
//        .UseFunctionInvocation()
//        .Build();
//});

builder.Services.AddScoped<ChatClientAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return new ChatClientAgent(chatClient, instructions: "You are a helpful financial assistant for Centuition.", name: "FinancialAssistant");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Apply request localization
app.UseRequestLocalization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

static string NormalizeAzureOpenAIEndpoint(string endpoint)
{
    var trimmed = endpoint.Trim();
    if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException("AzureOPENAI:Endpoint is not a valid absolute URI.");
    }

    var host = uri.Host;
    if (host.EndsWith(".cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase))
    {
        host = host.Replace(".cognitiveservices.azure.com", ".openai.azure.com", StringComparison.OrdinalIgnoreCase);
    }

    var baseUri = new UriBuilder(uri)
    {
        Host = host,
        Path = "",
        Query = "",
        Fragment = ""
    };

    return baseUri.Uri.ToString().TrimEnd('/') + "/";
}
