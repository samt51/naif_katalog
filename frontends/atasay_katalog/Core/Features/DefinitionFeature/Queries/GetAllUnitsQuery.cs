using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class UnitDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllUnitsQueryRequest : IRequest<ResponseDto<List<UnitDto>>>
    {
    }

    // Handler
    public class GetAllUnitsQueryHandler : BaseHandler, IRequestHandler<GetAllUnitsQueryRequest, ResponseDto<List<UnitDto>>>
    {
        public GetAllUnitsQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<UnitDto>>> Handle(GetAllUnitsQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<UnitDto>>("api/Units");
        }
    }
}
