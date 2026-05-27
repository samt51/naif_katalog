using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateColorsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateColorsCommandHandler : BaseHandler, IRequestHandler<CreateColorsCommandRequest, ResponseDto<bool>>
    {
        public CreateColorsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateColorsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateColorsCommandRequest, bool>("api/Colors", request);
        }
    }
}
