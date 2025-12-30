
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Enable authorization services - policies can be added here as needed
builder.Services.AddAuthorization();

// Add CORS support for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("https://localhost:7145", "http://localhost:5179")
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options => {options.UseInMemoryDatabase("TestDb");});
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));


// Register RootDbReadWrite with scoped lifetime to ensure a new instance per request
builder.Services.AddScoped<IRootDbReadWrite>(x => new RootDbReadWrite(builder.Services));

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
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add additional authentication schemes if needed
// builder.Services.AddAuthentication()
//     .AddCookie("Cookies", options =>
//     {
//         options.LoginPath = "/Account/Login";
//         options.LogoutPath = "/Account/Logout";
//         options.AccessDeniedPath = "/Account/AccessDenied";
//         options.ExpireTimeSpan = TimeSpan.FromHours(8);
//         options.SlidingExpiration = true;
//     });
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

WeatherEndpoints.MapWeatherEndpoints(app);
WeatherEndpointsV2.MapWeatherEndpointsV2(app);
OrganizationEndpoints.MapOrganizationEndpoints(app);
UserRolesEndpoints.MapUserRolesEndpoints(app);
AppUserOrganizationEndpoints.MapAppUserOrganizationEndpoints(app);
AppUserDepartmentEndpoints.MapAppUserDepartmentEndpoints(app);

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
