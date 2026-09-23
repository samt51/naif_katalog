using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class StoneScaleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllStoneScalesQueryRequest : IRequest<ResponseDto<List<StoneScaleDto>>>
    {
    }

    // Handler
    public class GetAllStoneScalesQueryHandler : BaseHandler, IRequestHandler<GetAllStoneScalesQueryRequest, ResponseDto<List<StoneScaleDto>>>
    {
        public GetAllStoneScalesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<StoneScaleDto>>> Handle(GetAllStoneScalesQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneScaleDto>>("api/StoneScale");
        }
    }
}
