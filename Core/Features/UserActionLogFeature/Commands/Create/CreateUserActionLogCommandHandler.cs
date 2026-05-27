using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.UserActionLogFeature.Commands.Create
{
    public class CreateUserActionLogCommandHandler : BaseHandler, IRequestHandler<CreateUserActionLogCommandRequest, ResponseDto<bool>>
    {
        public CreateUserActionLogCommandHandler(IApiService apiService) : base(apiService) {}
        public async Task<ResponseDto<bool>> Handle(CreateUserActionLogCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateUserActionLogCommandRequest, bool>("api/UserActionLog", request);
        }
    }
}