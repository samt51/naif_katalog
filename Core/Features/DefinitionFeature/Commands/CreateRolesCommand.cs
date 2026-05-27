using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateRolesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
    }

    // Handler
    public class CreateRolesCommandHandler : BaseHandler, IRequestHandler<CreateRolesCommandRequest, ResponseDto<bool>>
    {
        public CreateRolesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateRolesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateRolesCommandRequest, bool>("api/Roles", request);
        }
    }
}
