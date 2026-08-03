using MediatR;
using naif_katalog.Models;

namespace naif_katalog.Core.Features.CategoryFeature.Commands
{
    public class CreateCategoryCommandRequest : IRequest<ResponseDto<CreateCategoryCommandResponse>>
    {
        public string Name { get; set; }
        public int ParentId { get; set; }
    }

    public sealed class CreateCategoryCommandResponse { }
}

