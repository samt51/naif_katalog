using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class DeleteProductCommandHandler : BaseHandler, IRequestHandler<DeleteProductCommandRequest, ResponseDto<bool>>
    {
        public DeleteProductCommandHandler(IApiService apiService) : base(apiService) {}

        public async Task<ResponseDto<bool>> Handle(DeleteProductCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.DeleteAsync<bool>($"api/Product/{request.Id}$");
        }
    }
}
