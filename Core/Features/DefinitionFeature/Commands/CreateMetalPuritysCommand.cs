using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateMetalPuritysCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateMetalPuritysCommandHandler : BaseHandler, IRequestHandler<CreateMetalPuritysCommandRequest, ResponseDto<bool>>
    {
        public CreateMetalPuritysCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateMetalPuritysCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateMetalPuritysCommandRequest, bool>("api/MetalPurity", request);
        }
    }
}
