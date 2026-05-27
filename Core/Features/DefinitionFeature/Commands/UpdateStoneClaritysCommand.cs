using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateStoneClaritysCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateStoneClaritysCommandHandler : BaseHandler, IRequestHandler<UpdateStoneClaritysCommandRequest, ResponseDto<bool>>
    {
        public UpdateStoneClaritysCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateStoneClaritysCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneClaritysCommandRequest, bool>("api/StoneClarity", request);
        }
    }
}
