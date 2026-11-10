
namespace backend.DTOs;

public class StockMovementReportDto
{
    public int TotalMovements { get; set; }

    public int TotalInboundQuantity { get; set; }

    public int TotalOutboundQuantity { get; set; }

    public int NetStockChange { get; set; }

    public DateTime GeneratedAt { get; set; }
}
