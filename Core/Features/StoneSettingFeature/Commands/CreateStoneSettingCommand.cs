using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneSettingFeature.Commands
{
    public class CreateStoneSettingCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int UnitId { get; set; }
        public decimal SettingPrice { get; set; }
    }

    public class CreateStoneSettingCommandHandler : BaseHandler, IRequestHandler<CreateStoneSettingCommandRequest, ResponseDto<bool>>
    {
        public CreateStoneSettingCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateStoneSettingCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneSettingCommandRequest, bool>("api/StoneSetting", request);
        }
    }
}
