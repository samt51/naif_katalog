using MediatR;
using atasay_katalog.Models;
using System.Collections.Generic;

namespace atasay_katalog.Core.Features.CategoryFeature.Queries
{
    public class GetAllCategoriesQueryRequest : IRequest<ResponseDto<List<CategoryDto>>>
    {
    }
}
