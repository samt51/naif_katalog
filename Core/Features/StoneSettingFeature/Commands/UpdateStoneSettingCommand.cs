using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneSettingFeature.Commands
{
    public class UpdateStoneSettingCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public decimal SettingPrice { get; set; }
    }

    public class UpdateStoneSettingCommandHandler : BaseHandler, IRequestHandler<UpdateStoneSettingCommandRequest, ResponseDto<bool>>
    {
        public UpdateStoneSettingCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateStoneSettingCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneSettingCommandRequest, bool>("api/StoneSetting", request);
        }
    }
}
