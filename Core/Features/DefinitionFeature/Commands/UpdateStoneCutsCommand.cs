using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateStoneCutsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateStoneCutsCommandHandler : BaseHandler, IRequestHandler<UpdateStoneCutsCommandRequest, ResponseDto<bool>>
    {
        public UpdateStoneCutsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateStoneCutsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneCutsCommandRequest, bool>("api/StoneCut", request);
        }
    }
}
