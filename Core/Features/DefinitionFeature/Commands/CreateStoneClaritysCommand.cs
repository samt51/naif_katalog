using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateStoneClaritysCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateStoneClaritysCommandHandler : BaseHandler, IRequestHandler<CreateStoneClaritysCommandRequest, ResponseDto<bool>>
    {
        public CreateStoneClaritysCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateStoneClaritysCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneClaritysCommandRequest, bool>("api/StoneClarity", request);
        }
    }
}
