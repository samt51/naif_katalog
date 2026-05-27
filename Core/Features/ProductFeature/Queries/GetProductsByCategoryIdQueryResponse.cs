namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetProductsByCategoryIdQueryResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageName { get; set; }
        public string Category { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public decimal Gram { get; set; }
        public string Karat { get; set; }
        public decimal DiamondCarat { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public string MetalPurityName { get; set; }
        public decimal LiveGoldPrice { get; set; }
        public decimal CalculatedPrice { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
