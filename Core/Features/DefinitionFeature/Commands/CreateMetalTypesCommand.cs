using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateMetalTypesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateMetalTypesCommandHandler : BaseHandler, IRequestHandler<CreateMetalTypesCommandRequest, ResponseDto<bool>>
    {
        public CreateMetalTypesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateMetalTypesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateMetalTypesCommandRequest, bool>("api/MetalType", request);
        }
    }
}
