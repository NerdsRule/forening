
using Microsoft.Extensions.DependencyInjection.Extensions;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var configuredApiBaseUrl = builder.Configuration["services:apiservice:https:0"]
    ?? builder.Configuration["services:apiservice:http:0"]
    ?? builder.Configuration["ApiSettings:BaseUrl"];

var authApiBaseAddress = Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var parsedApiUri)
    ? parsedApiUri
    : new Uri(builder.HostEnvironment.BaseAddress);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IPrivateLocalStorageService, PrivateLocalStorageService>();
builder.Services.AddScoped<IUiStateService, UiStateService>();

#region Authentication
// register the cookie handler
builder.Services.AddTransient<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
var accountService = builder.Services.RemoveAll<IAccountService>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(sp => (IAccountService)sp.GetRequiredService<AuthenticationStateProvider>());

// To make AuthorizeView work in WASM (Fond in other app)
builder.Services.AddCascadingAuthenticationState();

// configure client for auth interactions
builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = authApiBaseAddress;
    client.Timeout = TimeSpan.FromMinutes(1);
}).AddHttpMessageHandler<CookieHandler>();


// Added by code
//builder.Services.AddOidcAuthentication(options =>
//{
    // Configure your authentication provider optio ns here.
    // For more information, see https://aka.ms/blazor-standalone-auth
//    builder.Configuration.Bind("Local", options.ProviderOptions);
//});

#endregion

builder.Services.AddScoped<IDepartmentTaskService, DepartmentTaskService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPrizeService, PrizeService>();
builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
await builder.Build().RunAsync();

