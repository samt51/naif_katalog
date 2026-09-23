using MediatR;
using atasay_katalog.Models;
using System.Collections.Generic;

namespace atasay_katalog.Core.Features.ProductFeature.Queries
{
    public class GetProductsByCategoryIdQueryRequest : IRequest<ResponseDto<List<GetProductsByCategoryIdQueryResponse>>>
    {
        public int CategoryId { get; set; }
    }
}
