using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class UpdateProductCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Gram { get; set; }
        public string Karat { get; set; }
        public decimal DiamondCarat { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
        public int? MetalPurityId { get; set; }
        public int? ColorId { get; set; }
        public decimal LaborMultiplier { get; set; }
        public decimal PolishingCost { get; set; }
        public decimal LiveGoldPrice { get; set; }
        
        public List<string> ImageNames { get; set; } = new List<string>();
        public List<UpdateProductStoneDto> ProductStones { get; set; } = new List<UpdateProductStoneDto>();
        public List<UpdateProductMetalDto> ProductMetals { get; set; } = new List<UpdateProductMetalDto>();
    }

    public class UpdateProductStoneDto
    {
        public int Id { get; set; } // 0 if new
        public int StoneId { get; set; }
        public int Quantity { get; set; }
        public decimal Carat { get; set; }
        public decimal TotalCarat { get; set; }
        public int? ClarityId { get; set; }
        public int? ColorId { get; set; }
    }

    public class UpdateProductMetalDto
    {
        public int Id { get; set; } // 0 if new
        public int MetalTypeId { get; set; }
        public decimal Weight { get; set; }
    }
}
