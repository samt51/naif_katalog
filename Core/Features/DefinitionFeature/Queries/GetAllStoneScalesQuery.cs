using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
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
