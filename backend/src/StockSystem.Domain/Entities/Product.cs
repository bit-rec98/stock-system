using StockSystem.Domain.Common;

namespace StockSystem.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int MinStockLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual Supplier? Supplier { get; set; }
}
