using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateStoneScalesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateStoneScalesCommandHandler : BaseHandler, IRequestHandler<UpdateStoneScalesCommandRequest, ResponseDto<bool>>
    {
        public UpdateStoneScalesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateStoneScalesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateStoneScalesCommandRequest, bool>("api/StoneScale", request);
        }
    }
}
