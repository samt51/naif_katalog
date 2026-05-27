using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Gram { get; set; }
        public string Karat { get; set; }
        public decimal DiamondCarat { get; set; }
        public int CategoryId { get; set; }
        public int? MetalPurityId { get; set; }
        public int? ColorId { get; set; }
        public decimal LaborMultiplier { get; set; }
        public decimal PolishingCost { get; set; }
        public decimal LiveGoldPrice { get; set; }
        
        public List<string> ImageNames { get; set; } = new List<string>();
        public List<CreateProductStoneDto> ProductStones { get; set; } = new List<CreateProductStoneDto>();
        public List<CreateProductMetalDto> ProductMetals { get; set; } = new List<CreateProductMetalDto>();
    }

    public class CreateProductStoneDto
    {
        public int StoneId { get; set; }
        public int Quantity { get; set; }
        public decimal Carat { get; set; }
        public decimal TotalCarat { get; set; }
        public int? ClarityId { get; set; }
        public int? ColorId { get; set; }
    }

    public class CreateProductMetalDto
    {
        public int MetalTypeId { get; set; }
        public decimal Weight { get; set; }
    }
}
