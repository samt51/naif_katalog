using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class StoneTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllStoneTypesQueryRequest : IRequest<ResponseDto<List<StoneTypeDto>>>
    {
    }

    // Handler
    public class GetAllStoneTypesQueryHandler : BaseHandler, IRequestHandler<GetAllStoneTypesQueryRequest, ResponseDto<List<StoneTypeDto>>>
    {
        public GetAllStoneTypesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<StoneTypeDto>>> Handle(GetAllStoneTypesQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneTypeDto>>("api/StoneType");
        }
    }
}
