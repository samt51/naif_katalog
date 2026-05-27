using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateStoneTypesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateStoneTypesCommandHandler : BaseHandler, IRequestHandler<UpdateStoneTypesCommandRequest, ResponseDto<bool>>
    {
        public UpdateStoneTypesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateStoneTypesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneTypesCommandRequest, bool>("api/StoneType", request);
        }
    }
}
