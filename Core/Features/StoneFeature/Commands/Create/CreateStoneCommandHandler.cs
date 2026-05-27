using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneFeature.Commands
{
    public class CreateStoneCommandHandler : BaseHandler, IRequestHandler<CreateStoneCommandRequest, ResponseDto<CreateStoneCommandResponse>>
    {
        public CreateStoneCommandHandler(IApiService apiService) : base(apiService) {}

        public async Task<ResponseDto<CreateStoneCommandResponse>> Handle(CreateStoneCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneCommandRequest, CreateStoneCommandResponse>("api/Stone", request);
        }
    }
}
