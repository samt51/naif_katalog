using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.CategoryFeature.Commands.UpdateOrder
{
    public class UpdateCategoryOrderDto
    {
        public int Id { get; set; }
        public int OrderIndex { get; set; }
    }

    public class UpdateCategoryOrderCommandRequest : IRequest<ResponseDto<bool>>
    {
        public List<UpdateCategoryOrderDto> Categories { get; set; } = new List<UpdateCategoryOrderDto>();
    }
}