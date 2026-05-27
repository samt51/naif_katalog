using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneSettingFeature.Commands
{
    public class DeleteStoneSettingCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteStoneSettingCommandHandler : BaseHandler, IRequestHandler<DeleteStoneSettingCommandRequest, ResponseDto<bool>>
    {
        public DeleteStoneSettingCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(DeleteStoneSettingCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.DeleteAsync<bool>("api/StoneSetting/" + request.Id);
        }
    }
}
