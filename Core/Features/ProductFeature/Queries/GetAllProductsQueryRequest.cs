using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetAllProductsQueryRequest : IRequest<ResponseDto<List<Product>>>
    {
        public string? Code { get; set; }
        public string? Category { get; set; }
        public decimal? MinGram { get; set; }
        public decimal? MaxGram { get; set; }
        public int? MetalTypeId { get; set; }
        public int? ClarityId { get; set; }
        public int? StoneId { get; set; }
        public int? StoneTypeId { get; set; }
    }
}
