using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class StoneCutDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllStoneCutsQueryRequest : IRequest<ResponseDto<List<StoneCutDto>>>
    {
    }

    // Handler
    public class GetAllStoneCutsQueryHandler : BaseHandler, IRequestHandler<GetAllStoneCutsQueryRequest, ResponseDto<List<StoneCutDto>>>
    {
        public GetAllStoneCutsQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<StoneCutDto>>> Handle(GetAllStoneCutsQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneCutDto>>("api/StoneCut");
        }
    }
}
