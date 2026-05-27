using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneFeature.Commands
{
    public class UpdateStoneCommandHandler : BaseHandler, IRequestHandler<UpdateStoneCommandRequest, ResponseDto<UpdateStoneCommandResponse>>
    {
        public UpdateStoneCommandHandler(IApiService apiService) : base(apiService) {}

        public async Task<ResponseDto<UpdateStoneCommandResponse>> Handle(UpdateStoneCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneCommandRequest, UpdateStoneCommandResponse>("api/Stone", request);
        }
    }
}
