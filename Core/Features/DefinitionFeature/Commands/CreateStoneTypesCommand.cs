using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateStoneTypesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateStoneTypesCommandHandler : BaseHandler, IRequestHandler<CreateStoneTypesCommandRequest, ResponseDto<bool>>
    {
        public CreateStoneTypesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateStoneTypesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneTypesCommandRequest, bool>("api/StoneType", request);
        }
    }
}
