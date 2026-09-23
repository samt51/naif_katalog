using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class MetalTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllMetalTypesQueryRequest : IRequest<ResponseDto<List<MetalTypeDto>>>
    {
    }

    // Handler
    public class GetAllMetalTypesQueryHandler : BaseHandler, IRequestHandler<GetAllMetalTypesQueryRequest, ResponseDto<List<MetalTypeDto>>>
    {
        public GetAllMetalTypesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<MetalTypeDto>>> Handle(GetAllMetalTypesQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<MetalTypeDto>>("api/MetalType");
        }
    }
}
