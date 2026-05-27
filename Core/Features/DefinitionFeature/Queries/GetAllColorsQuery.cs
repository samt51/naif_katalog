using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class ColorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorType { get; set; }
    }

    // Request
    public class GetAllColorsQueryRequest : IRequest<ResponseDto<List<ColorDto>>>
    {
    }

    // Handler
    public class GetAllColorsQueryHandler : BaseHandler, IRequestHandler<GetAllColorsQueryRequest, ResponseDto<List<ColorDto>>>
    {
        public GetAllColorsQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<ColorDto>>> Handle(GetAllColorsQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<ColorDto>>("api/Colors");
        }
    }
}

