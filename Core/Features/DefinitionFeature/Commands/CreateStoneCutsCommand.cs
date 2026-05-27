using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateStoneCutsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateStoneCutsCommandHandler : BaseHandler, IRequestHandler<CreateStoneCutsCommandRequest, ResponseDto<bool>>
    {
        public CreateStoneCutsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateStoneCutsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneCutsCommandRequest, bool>("api/StoneCut", request);
        }
    }
}
