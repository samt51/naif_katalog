using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class UpdateProductCommandHandler : BaseHandler, IRequestHandler<UpdateProductCommandRequest, ResponseDto<bool>>
    {
        public UpdateProductCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(UpdateProductCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateProductCommandRequest, bool>("api/Products", request);
        }
    }
}
