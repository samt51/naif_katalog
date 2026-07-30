using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.CategoryFeature.Commands
{
    public class UpdateCategoryCommandHandler : BaseHandler, IRequestHandler<UpdateCategoryCommandRequest, ResponseDto<bool>>
    {
        public UpdateCategoryCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(UpdateCategoryCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateCategoryCommandRequest, bool>("api/Category", request);
        }
    }
}
