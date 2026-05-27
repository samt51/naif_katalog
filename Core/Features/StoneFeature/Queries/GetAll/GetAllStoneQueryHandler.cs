using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneFeature.Queries
{
    public class GetAllStoneQueryHandler : BaseHandler, IRequestHandler<GetAllStoneQueryRequest, ResponseDto<List<GetAllStoneQueryResponse>>>
    {
        public GetAllStoneQueryHandler(IApiService apiService) : base(apiService) {}

        public async Task<ResponseDto<List<GetAllStoneQueryResponse>>> Handle(GetAllStoneQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<GetAllStoneQueryResponse>>("api/Stone");
        }
    }
}
