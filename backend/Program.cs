
using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4201"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers and Error Handling
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Dependency Injection
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StockMovementService>();

// API Documentation
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply EF Core migrations when explicitly enabled.
if (builder.Configuration.GetValue<bool>("ApplyDatabaseMigrations"))
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    dbContext.Database.Migrate();
}

// Global Exception Handling
app.UseExceptionHandler();

// Swagger Configuration
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP Pipeline
app.UseHttpsRedirection();

app.UseCors("AngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

// WebApplicationFactory Integration Test Support
public partial class Program
{
}
