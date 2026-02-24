
using Microsoft.Extensions.DependencyInjection.Extensions;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

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
    //client.BaseAddress = new Uri("https+http://apiservice");
    client.BaseAddress = new Uri("https://localhost:7375");
    //client.BaseAddress = new Uri("https://localhost:8080");
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

await builder.Build().RunAsync();
