using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class MetalPurityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Milyem { get; set; }
        public decimal? Density { get; set; }
    }

    // Request
    public class GetAllMetalPuritysQueryRequest : IRequest<ResponseDto<List<MetalPurityDto>>>
    {
    }

    // Handler
    public class GetAllMetalPuritysQueryHandler : BaseHandler, IRequestHandler<GetAllMetalPuritysQueryRequest, ResponseDto<List<MetalPurityDto>>>
    {
        public GetAllMetalPuritysQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<MetalPurityDto>>> Handle(GetAllMetalPuritysQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<MetalPurityDto>>("api/MetalPurity");
        }
    }
}
