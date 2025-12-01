
using Microsoft.Extensions.DependencyInjection.Extensions;
using Organization.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// register the cookie handler
builder.Services.AddTransient<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
var accountService = builder.Services.RemoveAll<IAccountService>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(sp => (IAccountService)sp.GetRequiredService<AuthenticationStateProvider>());

// To make AuthorizeView work in WASM
builder.Services.AddCascadingAuthenticationState();

// configure client for auth interactions
builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri(builder.Environment.ContentRootPath);
    client.Timeout = TimeSpan.FromMinutes(1);
}).AddHttpMessageHandler<CookieHandler>();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// register the custom state provider
//var accountService = builder.Services.RemoveAll<IAccountService>();
//builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(sp => (IAccountService)sp.GetRequiredService<AuthenticationStateProvider>());


builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
