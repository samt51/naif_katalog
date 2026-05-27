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
            var result = await _apiService.PutAsync<UpdateCategoryCommandRequest, object>("api/Category", request);
            return new ResponseDto<bool> { data = result.isSuccess, isSuccess = result.isSuccess, statusCode = result.statusCode, errors = result.errors };
        }
    }
}
