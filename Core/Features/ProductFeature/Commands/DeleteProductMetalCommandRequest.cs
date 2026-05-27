using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class DeleteProductMetalCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
    }
}
