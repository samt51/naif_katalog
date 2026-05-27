using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductStoneCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int ProductId { get; set; }
        public int StoneId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Carat { get; set; }
        public decimal TotalCarat { get; set; }
    }
}
