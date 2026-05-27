using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateRolesCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Handler
    public class UpdateRolesCommandHandler : BaseHandler, IRequestHandler<UpdateRolesCommandRequest, ResponseDto<bool>>
    {
        public UpdateRolesCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateRolesCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateRolesCommandRequest, bool>("api/Roles", request);
        }
    }
}
