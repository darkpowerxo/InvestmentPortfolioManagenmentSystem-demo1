using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Infrastructure.Data;
using InvestmentPortfolioManager.Infrastructure.Extensions;
using InvestmentPortfolioManager.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application Services
builder.Services.AddApplicationServices();

// Register Infrastructure Services (repositories, unit of work)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Investment Portfolio Manager API", 
        Version = "v1",
        Description = "API pour la gestion de portefeuilles d'investissement / API for investment portfolio management"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investment Portfolio Manager API V1");
        c.RoutePrefix = string.Empty; // Make Swagger UI available at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    
    await context.Database.EnsureCreatedAsync();
    await seedDataService.SeedDataAsync();
}

app.Run();
