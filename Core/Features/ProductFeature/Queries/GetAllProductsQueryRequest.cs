using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetAllProductsQueryRequest : IRequest<ResponseDto<List<Product>>>
    {
    }
}
