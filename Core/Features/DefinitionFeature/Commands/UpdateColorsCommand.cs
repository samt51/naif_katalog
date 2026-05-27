using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateColorsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateColorsCommandHandler : BaseHandler, IRequestHandler<UpdateColorsCommandRequest, ResponseDto<bool>>
    {
        public UpdateColorsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateColorsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateColorsCommandRequest, bool>("api/Colors", request);
        }
    }
}
