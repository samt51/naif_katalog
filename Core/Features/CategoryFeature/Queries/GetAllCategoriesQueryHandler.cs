using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.CategoryFeature.Queries
{
    public class GetAllCategoriesQueryHandler : BaseHandler, IRequestHandler<GetAllCategoriesQueryRequest, ResponseDto<List<CategoryDto>>>
    {
        public GetAllCategoriesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<CategoryDto>>> Handle(GetAllCategoriesQueryRequest request, CancellationToken cancellationToken)
        {
            var apiResult = await _apiService.GetAsync<List<CategoryDto>>("api/Category");
            return apiResult;
        }
    }
}
