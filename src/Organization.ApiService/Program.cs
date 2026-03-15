
using Organization.Shared.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();

// Enable authorization services - policies can be added here as needed
builder.Services.AddAuthorization();

// Add CORS support for Blazor WebAssembly
// Read endpoints from Environment variable or configuration if needed, for now we hardcode localhost origins for development
ApiServiceStatic.AllowedOrigins = Environment.GetEnvironmentVariable("CORS")?
    .Split(';', StringSplitOptions.RemoveEmptyEntries)
    .Select(origin => origin.Trim())
    .ToArray() ?? ["https://localhost:7145", "http://localhost:5179", "https://localhost:8081", "http://localhost:8081"];
//new[] { "https://localhost:7145", "http://localhost:5179", "https://localhost:8081", "http://localhost:8081" };

builder.Services.AddCors(options =>
{
    
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(ApiServiceStatic.AllowedOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .WithMethods("GET", "PUT", "DELETE", "POST")
                    .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

#region Database
var connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
//var connectionString = builder.Configuration.GetConnectionString("MemoryConnection");
//builder.Services.AddDbContext<AppDbContext>(options => {options.UseInMemoryDatabase("TestDb");});
//var connectionString = builder.Configuration.GetConnectionString("LocalConnection");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));


// Register RootDbReadWrite with scoped lifetime to ensure a new instance per request
builder.Services.AddScoped<IRootDbReadWrite, RootDbReadWrite>();

// Add Identity services
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

// Configure cookie authentication for Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OrgAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/v1/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/v1/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await using var scope = app.Services.CreateAsyncScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Enable CORS
app.UseCors("AllowLocalhost");

// Ensure authentication/authorization middleware are in place.
// Authentication must run before Authorization and before endpoint routing that requires it.
app.UseAuthentication();
app.UseAuthorization();

WeatherEndpointsV2.MapWeatherEndpointsV2(app);
OrganizationEndpoints.MapOrganizationEndpoints(app);
UserRolesEndpoints.MapUserRolesEndpoints(app);
AppUserOrganizationEndpoints.MapAppUserOrganizationEndpoints(app);
AppUserDepartmentEndpoints.MapAppUserDepartmentEndpoints(app);
DepartmentEndpoint.MapDepartmentEndpoints(app);
PrizeEndpoint.MapPrizeEndpoints(app);
TaskEndpoint.MapTaskEndpoints(app);

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
