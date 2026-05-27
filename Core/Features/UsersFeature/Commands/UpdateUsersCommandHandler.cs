using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.UsersFeature.Commands
{
    public class UpdateUsersCommandHandler : BaseHandler, IRequestHandler<UpdateUsersCommandRequest, ResponseDto<UpdateUsersCommandResponse>>
    {
        public UpdateUsersCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<UpdateUsersCommandResponse>> Handle(UpdateUsersCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateUsersCommandRequest, UpdateUsersCommandResponse>("api/Users", request);
        }
    }
}
