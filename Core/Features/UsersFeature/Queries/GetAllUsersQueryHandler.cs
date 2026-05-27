using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.UsersFeature.Queries
{
    public class GetAllUsersQueryHandler : BaseHandler, IRequestHandler<GetAllUsersQueryRequest, ResponseDto<List<UsersDto>>>
    {
        public GetAllUsersQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<UsersDto>>> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
        {
            var apiResult = await _apiService.GetAsync<List<UsersDto>>("api/Users");
            return apiResult;
        }
    }
}
