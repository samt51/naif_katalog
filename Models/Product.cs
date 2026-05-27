using System.Collections.Generic;

namespace naif_katalog.Models;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Description { get; set; }
    public decimal Gram { get; set; }
    public string Karat { get; set; }
    public string MetalPurityName { get; set; }
    public decimal DiamondCarat { get; set; }
    public int? ColorId { get; set; }
    public string ColorName { get; set; }
    public decimal LiveGoldPrice { get; set; }
    public decimal CalculatedPrice { get; set; }
    public decimal LaborMultiplier { get; set; }
    public decimal PolishingCost { get; set; }
    public List<string> Images { get; set; } = new List<string>();
    public List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductStone> ProductStones { get; set; } = new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductStone>();
    public List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductMetal> ProductMetals { get; set; } = new List<naif_katalog.Core.Features.ProductFeature.Queries.ApiProductMetal>();
}
