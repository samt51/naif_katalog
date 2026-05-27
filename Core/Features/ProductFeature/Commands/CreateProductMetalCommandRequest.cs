using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductMetalCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int ProductId { get; set; }
        public int MetalTypeId { get; set; }
        public decimal Gram { get; set; }
    }
}
