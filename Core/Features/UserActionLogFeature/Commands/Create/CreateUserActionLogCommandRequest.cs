using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.UserActionLogFeature.Commands.Create
{
    public class CreateUserActionLogCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int UserId { get; set; }
        public string ActionType { get; set; }
        public int? ProductId { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}