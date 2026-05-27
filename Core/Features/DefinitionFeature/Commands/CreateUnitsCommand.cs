using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateUnitsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateUnitsCommandHandler : BaseHandler, IRequestHandler<CreateUnitsCommandRequest, ResponseDto<bool>>
    {
        public CreateUnitsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateUnitsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateUnitsCommandRequest, bool>("api/Units", request);
        }
    }
}
