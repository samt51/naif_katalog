using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneFeature.Commands
{
    public class DeleteStoneCommandHandler : BaseHandler, IRequestHandler<DeleteStoneCommandRequest, ResponseDto<DeleteStoneCommandResponse>>
    {
        public DeleteStoneCommandHandler(IApiService apiService) : base(apiService) {}

        public async Task<ResponseDto<DeleteStoneCommandResponse>> Handle(DeleteStoneCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.DeleteAsync<DeleteStoneCommandResponse>($"api/Stone/{request.Id}");
        }
    }
}
