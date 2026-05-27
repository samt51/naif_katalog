using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductMetalCommandHandler : BaseHandler, IRequestHandler<CreateProductMetalCommandRequest, ResponseDto<bool>>
    {
        public CreateProductMetalCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(CreateProductMetalCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateProductMetalCommandRequest, bool>("api/ProductMetal", request);
        }
    }
}
