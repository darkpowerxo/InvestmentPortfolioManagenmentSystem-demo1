using InvestmentPortfolioManager.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Investment Portfolio Manager API",
        Version = "v1",
        Description = "CDPQ-style Investment Portfolio Management System API / API du système de gestion de portefeuille d'investissement de style CDPQ",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Investment Portfolio Manager",
            Email = "contact@portfoliomanager.com"
        }
    });
    
    // Add support for bilingual descriptions
    c.EnableAnnotations();
});

// Register application services
// FinancialCalculations is static, no need to register
builder.Services.AddScoped<PortfolioAnalyticsService>();
builder.Services.AddScoped<MarketDataService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add localization support
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-CA", "fr-CA" };
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investment Portfolio Manager API v1");
        c.RoutePrefix = string.Empty; // Make Swagger UI the root page
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRequestLocalization();

app.UseRouting();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Message = "Investment Portfolio Manager API is running / L'API du gestionnaire de portefeuille d'investissement fonctionne"
}).WithTags("Health");

app.Run();
