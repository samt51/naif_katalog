using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateUnitsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateUnitsCommandHandler : BaseHandler, IRequestHandler<UpdateUnitsCommandRequest, ResponseDto<bool>>
    {
        public UpdateUnitsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateUnitsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateUnitsCommandRequest, bool>("api/Units", request);
        }
    }
}
