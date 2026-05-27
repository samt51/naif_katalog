using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.StoneSettingFeature.Queries
{
    public class StoneSettingDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string SettingType { get; set; }
        public decimal SettingPrice { get; set; }
    }

    public class GetAllStoneSettingQueryRequest : IRequest<ResponseDto<List<StoneSettingDto>>>
    {
    }

    public class GetAllStoneSettingQueryHandler : BaseHandler, IRequestHandler<GetAllStoneSettingQueryRequest, ResponseDto<List<StoneSettingDto>>>
    {
        public GetAllStoneSettingQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<StoneSettingDto>>> Handle(GetAllStoneSettingQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<StoneSettingDto>>("api/StoneSetting");
        }
    }
}
