using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductCommandHandler : BaseHandler, IRequestHandler<CreateProductCommandRequest, ResponseDto<bool>>
    {
        public CreateProductCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(CreateProductCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateProductCommandRequest, bool>("api/Products", request);
        }
    }
}
