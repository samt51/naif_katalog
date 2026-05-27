using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.ProductFeature.Commands
{
    public class CreateProductStoneCommandHandler : BaseHandler, IRequestHandler<CreateProductStoneCommandRequest, ResponseDto<bool>>
    {
        public CreateProductStoneCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(CreateProductStoneCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateProductStoneCommandRequest, bool>("api/ProductStone", request);
        }
    }
}
