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
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MetalTypeId { get; set; }
        public int? ClarityId { get; set; }
        public int? StoneId { get; set; }
        public int? StoneTypeId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? ColumnIndex { get; set; }
        public string? OrderBy { get; set; }
        public bool ApplyCustomerPricing { get; set; } = true;
    }
}
