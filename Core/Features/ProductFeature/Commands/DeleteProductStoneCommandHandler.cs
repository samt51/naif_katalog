using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class DeleteProductStoneCommandHandler : BaseHandler, IRequestHandler<DeleteProductStoneCommandRequest, ResponseDto<bool>>
    {
        public DeleteProductStoneCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(DeleteProductStoneCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.DeleteAsync<bool>($"api/ProductStone/{request.Id}");
        }
    }
}
