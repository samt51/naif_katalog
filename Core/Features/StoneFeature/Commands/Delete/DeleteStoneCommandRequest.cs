using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.StoneFeature.Commands
{
    public class DeleteStoneCommandRequest : IRequest<ResponseDto<DeleteStoneCommandResponse>>
    {
        public int Id { get; set; }
    }
}
