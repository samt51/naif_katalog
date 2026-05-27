using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.StoneFeature.Commands
{
    public class CreateStoneCommandRequest : IRequest<ResponseDto<CreateStoneCommandResponse>>
    {
public decimal CostPrice { get; set; }
        public int? StoneSettingId { get; set; }
        public int StoneTypeId { get; set; }
        public int UnitId { get; set; }
        public int? StoneCutId { get; set; }
        public int? StoneClarityId { get; set; }
        public int? StoneScaleId { get; set; }
        public int? ColorId { get; set; }
    }
}

