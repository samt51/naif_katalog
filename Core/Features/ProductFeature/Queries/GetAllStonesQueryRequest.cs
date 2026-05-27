using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Queries
{
    public class GetAllStonesQueryRequest : IRequest<ResponseDto<List<StoneDto>>> {}

    public class GetAllStonesQueryHandler : BaseHandler, IRequestHandler<GetAllStonesQueryRequest, ResponseDto<List<StoneDto>>>
    {
        public GetAllStonesQueryHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<List<StoneDto>>> Handle(GetAllStonesQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneDto>>("api/Stone");
        }
    }
}
