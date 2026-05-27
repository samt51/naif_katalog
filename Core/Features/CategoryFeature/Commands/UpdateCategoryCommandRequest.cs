using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.CategoryFeature.Commands
{
    public class UpdateCategoryCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}

