using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.CategoryFeature.Commands.UpdateOrder
{
    public class UpdateCategoryOrderCommandHandler : BaseHandler, IRequestHandler<UpdateCategoryOrderCommandRequest, ResponseDto<bool>>
    {
        public UpdateCategoryOrderCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(UpdateCategoryOrderCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<UpdateCategoryOrderCommandRequest, bool>("api/Category/UpdateOrder", request);
        }
    }
}