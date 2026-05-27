using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.CategoryFeature.Commands
{
    public class CreateCategoryCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}

