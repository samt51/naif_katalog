using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class DeleteProductMetalCommandHandler : BaseHandler, IRequestHandler<DeleteProductMetalCommandRequest, ResponseDto<bool>>
    {
        public DeleteProductMetalCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(DeleteProductMetalCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.DeleteAsync<bool>($"api/ProductMetal/{request.Id}");
        }
    }
}
