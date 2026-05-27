using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetProductsByCategoryIdQueryRequest : IRequest<ResponseDto<List<GetProductsByCategoryIdQueryResponse>>>
    {
        public int CategoryId { get; set; }
    }
}
