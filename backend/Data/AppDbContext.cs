
using Microsoft.EntityFrameworkCore;
using backend.Entities;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryBalance> InventoryBalances
        => Set<InventoryBalance>();
    public DbSet<StockMovement> StockMovements
        => Set<StockMovement>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StockReservation> StockReservations
        => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.SKU)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(p => p.SKU).IsUnique();

            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.UnitPrice)
                .HasPrecision(18, 2);
        });

        // Warehouse
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.Property(w => w.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(w => w.Code).IsUnique();

            entity.Property(w => w.Name)
                .HasMaxLength(200)
                .IsRequired();
        });

        // InventoryBalance
        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.HasIndex(i => new
            {
                i.ProductId,
                i.WarehouseId
            }).IsUnique();

            entity.HasOne(i => i.Product)
                .WithMany(p => p.InventoryBalances)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Warehouse)
                .WithMany(w => w.InventoryBalances)
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StockMovement
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(s => s.MovementType)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(s => s.ReferenceNumber)
                .HasMaxLength(50);

            entity.Property(s => s.Notes)
                .HasMaxLength(500);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_StockMovements_Quantity",
                    "[Quantity] > 0");

                t.HasCheckConstraint(
                    "CK_StockMovements_MovementType",
                    "[MovementType] IN ('Inbound', 'Outbound')");
            });

            entity.HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(o => o.CustomerEmail)
                .HasMaxLength(254);

            entity.Property(o => o.ShippingAddress)
                .HasMaxLength(500);

            entity.Property(o => o.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_OrderItems_Quantity",
                    "[Quantity] > 0");

                t.HasCheckConstraint(
                    "CK_OrderItems_UnitPrice",
                    "[UnitPrice] >= 0");
            });

            entity.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StockReservation
        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.Property(s => s.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_StockReservations_Quantity",
                    "[Quantity] > 0");
            });

            entity.HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Order)
                .WithMany(o => o.StockReservations)
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
