using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateMetalTypesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateMetalTypesCommandHandler : BaseHandler, IRequestHandler<UpdateMetalTypesCommandRequest, ResponseDto<bool>>
    {
        public UpdateMetalTypesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateMetalTypesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateMetalTypesCommandRequest, bool>("api/MetalType", request);
        }
    }
}
