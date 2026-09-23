using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class StoneClarityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllStoneClaritysQueryRequest : IRequest<ResponseDto<List<StoneClarityDto>>>
    {
    }

    // Handler
    public class GetAllStoneClaritysQueryHandler : BaseHandler, IRequestHandler<GetAllStoneClaritysQueryRequest, ResponseDto<List<StoneClarityDto>>>
    {
        public GetAllStoneClaritysQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<StoneClarityDto>>> Handle(GetAllStoneClaritysQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneClarityDto>>("api/StoneClarity");
        }
    }
}
