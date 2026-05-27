using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.CategoryFeature.Queries
{
    public class GetAllCategoriesQueryRequest : IRequest<ResponseDto<List<CategoryDto>>>
    {
    }
}
