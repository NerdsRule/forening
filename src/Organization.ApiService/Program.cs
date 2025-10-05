using Organization.ApiService.V1;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));


// Register RootDbReadWrite with scoped lifetime to ensure a new instance per request
builder.Services.AddScoped<IRootDbReadWrite>(x => new RootDbReadWrite(builder.Services));
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

WeatherEndpoints.MapWeatherEndpoints(app);
WeatherEndpointsV2.MapWeatherEndpointsV2(app);
OrganizationEndpoints.MapOrganizationEndpoints(app);

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
