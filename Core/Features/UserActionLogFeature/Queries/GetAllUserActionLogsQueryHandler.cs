using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.UserActionLogFeature.Queries
{
    public class GetAllUserActionLogsQueryHandler : BaseHandler, IRequestHandler<GetAllUserActionLogsQueryRequest, ResponseDto<List<UserActionLogDto>>>
    {
        public GetAllUserActionLogsQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<UserActionLogDto>>> Handle(GetAllUserActionLogsQueryRequest request, CancellationToken cancellationToken)
        {
            var apiResult = await _apiService.GetAsync<List<UserActionLogDto>>("api/UserActionLog");
            return apiResult;
        }
    }
}
