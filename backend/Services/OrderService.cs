
using backend.Data;
using backend.DTOs;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // POST: Create a new order and reserve stock.
    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException("Order request is required.");

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new InvalidOperationException("Customer name is required.");

        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException(
                "An order must contain at least one item.");

        if (dto.Items.Any(i =>
            i is null ||
            i.ProductId <= 0 ||
            i.WarehouseId <= 0 ||
            i.Quantity <= 0))
        {
            throw new InvalidOperationException(
                "Order items must have valid product, warehouse and quantity values.");
        }

        var duplicateItem = dto.Items
            .GroupBy(i => new { i.ProductId, i.WarehouseId })
            .Any(g => g.Count() > 1);

        if (duplicateItem)
            throw new InvalidOperationException(
                "The same product cannot appear twice for the same warehouse.");

        var productIds = dto.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException(
                "One or more products could not be found.");

        foreach (var product in products.Values)
        {
            if (!product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' is inactive.");

            if (product.UnitPrice < 0)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' has an invalid price.");
        }

        var warehouseIds = dto.Items
            .Select(i => i.WarehouseId)
            .Distinct()
            .ToList();

        var warehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id);

        if (warehouses.Count != warehouseIds.Count)
            throw new InvalidOperationException(
                "One or more warehouses could not be found.");

        foreach (var warehouse in warehouses.Values)
        {
            if (!warehouse.IsActive)
                throw new InvalidOperationException(
                    $"Warehouse '{warehouse.Name}' is inactive.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var inventoryBalances = await _context.InventoryBalances
                .Where(i =>
                    productIds.Contains(i.ProductId) &&
                    warehouseIds.Contains(i.WarehouseId))
                .ToListAsync();

            var inventoryDictionary = inventoryBalances
                .ToDictionary(i => (i.ProductId, i.WarehouseId));

            var order = new Order
            {
                CustomerName = dto.CustomerName.Trim(),
                CustomerEmail = dto.CustomerEmail?.Trim(),
                ShippingAddress = dto.ShippingAddress?.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];

                var inventoryKey = (
                    item.ProductId,
                    item.WarehouseId
                );

                if (!inventoryDictionary.TryGetValue(
                    inventoryKey, out var inventory))
                {
                    throw new InvalidOperationException(
                        $"Product '{product.Name}' is not available in the selected warehouse.");
                }

                var availableQuantity =
                    inventory.QuantityOnHand -
                    inventory.ReservedQuantity;

                if (availableQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Available: {availableQuantity}, Requested: {item.Quantity}.");
                }

                var unitPrice = product.UnitPrice;
                var lineTotal = checked(unitPrice * item.Quantity);
                totalAmount = checked(totalAmount + lineTotal);

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });

                inventory.ReservedQuantity = checked(
                    inventory.ReservedQuantity + item.Quantity);

                inventory.UpdatedAt = DateTime.UtcNow;

                order.StockReservations.Add(new StockReservation
                {
                    ProductId = product.Id,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.Quantity,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,

                Items = order.Items.Select(i =>
                    new OrderItemResponseDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = products[i.ProductId].Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.UnitPrice * i.Quantity
                    }).ToList()
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "Inventory was updated by another transaction. " +
                "Please refresh stock information and try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // GET: Get all orders.
    public async Task<List<OrderResponseDto>> GetAllAsync()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var productIds = orders
            .SelectMany(o => o.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return orders
            .Select(order => MapToResponse(order, products))
            .ToList();
    }

    // GET: Get order by ID.
    public async Task<OrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return null;

        var productIds = order.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return MapToResponse(order, products);
    }

    // POST: Ship an order and consume reserved stock.
    public async Task<OrderResponseDto?> ShipAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Load order, items and reservations.
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StockReservations)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
                return null;

            // 2. Only pending orders can be shipped.
            if (order.Status != "Pending")
            {
                throw new InvalidOperationException(
                    $"Order cannot be shipped. Current status: {order.Status}.");
            }

            // 3. Get active reservations.
            var activeReservations = order.StockReservations
                .Where(r => r.Status == "Active")
                .ToList();

            if (activeReservations.Count == 0)
            {
                throw new InvalidOperationException(
                    "No active stock reservations found for this order.");
            }

            // 4. Verify reservations match order items.
            var orderQuantities = order.Items
                .GroupBy(i => (i.ProductId, i.WarehouseId))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(i => i.Quantity));

            var reservationQuantities = activeReservations
                .GroupBy(r => (r.ProductId, r.WarehouseId))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.Quantity));

            if (orderQuantities.Count != reservationQuantities.Count ||
                orderQuantities.Any(item =>
                    !reservationQuantities.TryGetValue(
                        item.Key, out var quantity) ||
                    quantity != item.Value))
            {
                throw new InvalidOperationException(
                    "Stock reservations do not match the order items.");
            }

            // 5. Load inventory balances.
            var productIds = activeReservations
                .Select(r => r.ProductId)
                .Distinct()
                .ToList();

            var warehouseIds = activeReservations
                .Select(r => r.WarehouseId)
                .Distinct()
                .ToList();

            var inventoryBalances = await _context.InventoryBalances
                .Where(i =>
                    productIds.Contains(i.ProductId) &&
                    warehouseIds.Contains(i.WarehouseId))
                .ToListAsync();

            var inventoryDictionary = inventoryBalances
                .ToDictionary(i => (i.ProductId, i.WarehouseId));

            // 6. Consume reserved stock.
            foreach (var reservation in activeReservations)
            {
                var key = (
                    reservation.ProductId,
                    reservation.WarehouseId
                );

                if (!inventoryDictionary.TryGetValue(
                    key, out var inventory))
                {
                    throw new InvalidOperationException(
                        "Inventory balance could not be found.");
                }

                if (inventory.ReservedQuantity < reservation.Quantity)
                {
                    throw new InvalidOperationException(
                        "Reserved inventory quantity is insufficient.");
                }

                if (inventory.QuantityOnHand < reservation.Quantity)
                {
                    throw new InvalidOperationException(
                        "Physical inventory quantity is insufficient.");
                }

                inventory.QuantityOnHand -= reservation.Quantity;
                inventory.ReservedQuantity -= reservation.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                reservation.Status = "Fulfilled";

                // 7. Create outbound stock movement.
                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = reservation.ProductId,
                    WarehouseId = reservation.WarehouseId,
                    MovementType = "Outbound",
                    Quantity = reservation.Quantity,
                    ReferenceNumber = $"ORDER-{order.Id}",
                    Notes = "Stock consumed for shipped order.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 8. Update order status.
            order.Status = "Shipped";

            // 9. Save and commit.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 10. Return updated order.
            return await GetByIdAsync(id);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new InvalidOperationException(
                "Inventory was updated by another transaction. " +
                "Please refresh stock information and try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Map order entity to response DTO.
    private static OrderResponseDto MapToResponse(
        Order order,
        Dictionary<int, Product> products)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,

            Items = order.Items.Select(item =>
                new OrderItemResponseDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = products.TryGetValue(
                        item.ProductId, out var product)
                        ? product.Name
                        : null,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.UnitPrice * item.Quantity
                }).ToList()
        };
    }
}
