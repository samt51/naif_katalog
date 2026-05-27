using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.UsersFeature.Commands
{
    public class CreateUsersCommandHandler : BaseHandler, IRequestHandler<CreateUsersCommandRequest, ResponseDto<CreateUsersCommandResponse>>
    {
        public CreateUsersCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<CreateUsersCommandResponse>> Handle(CreateUsersCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateUsersCommandRequest, CreateUsersCommandResponse>("api/Users", request);
        }
    }
}
