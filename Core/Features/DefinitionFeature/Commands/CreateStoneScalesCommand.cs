using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateStoneScalesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateStoneScalesCommandHandler : BaseHandler, IRequestHandler<CreateStoneScalesCommandRequest, ResponseDto<bool>>
    {
        public CreateStoneScalesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateStoneScalesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateStoneScalesCommandRequest, bool>("api/StoneScale", request);
        }
    }
}
