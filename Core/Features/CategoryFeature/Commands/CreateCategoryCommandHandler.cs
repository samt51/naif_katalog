using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.CategoryFeature.Commands
{
    public class CreateCategoryCommandHandler : BaseHandler, IRequestHandler<CreateCategoryCommandRequest, ResponseDto<bool>>
    {
        public CreateCategoryCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(CreateCategoryCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateCategoryCommandRequest, bool>("api/Category", request);
        }
    }
}
