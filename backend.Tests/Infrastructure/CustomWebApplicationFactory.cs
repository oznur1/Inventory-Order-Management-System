
using backend.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace backend.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // SQL Server için kayıt edilmiş DbContext yapılandırmalarını kaldır.
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            // Test ortamında yalnızca SQLite kullan.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync(
            "DROP TABLE IF EXISTS InventoryBalances;");

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE InventoryBalances (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                WarehouseId INTEGER NOT NULL,
                QuantityOnHand INTEGER NOT NULL,
                ReservedQuantity INTEGER NOT NULL DEFAULT 0,
                ReorderLevel INTEGER NOT NULL,
                RowVersion BLOB NOT NULL DEFAULT X'00',
                UpdatedAt TEXT NOT NULL,
                CONSTRAINT CK_InventoryBalances_QuantityOnHand
                    CHECK (QuantityOnHand >= 0),
                CONSTRAINT CK_InventoryBalances_ReservedQuantity
                    CHECK (
                        ReservedQuantity >= 0
                        AND ReservedQuantity <= QuantityOnHand
                    ),
                CONSTRAINT FK_InventoryBalances_Products_ProductId
                    FOREIGN KEY (ProductId)
                    REFERENCES Products(Id)
                    ON DELETE RESTRICT,
                CONSTRAINT FK_InventoryBalances_Warehouses_WarehouseId
                    FOREIGN KEY (WarehouseId)
                    REFERENCES Warehouses(Id)
                    ON DELETE RESTRICT
            );

            CREATE UNIQUE INDEX
                IX_InventoryBalances_ProductId_WarehouseId
            ON InventoryBalances(ProductId, WarehouseId);
            """);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
