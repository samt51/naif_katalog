using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateMetalPuritysCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateMetalPuritysCommandHandler : BaseHandler, IRequestHandler<UpdateMetalPuritysCommandRequest, ResponseDto<bool>>
    {
        public UpdateMetalPuritysCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateMetalPuritysCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateMetalPuritysCommandRequest, bool>("api/MetalPurity", request);
        }
    }
}
