using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class CreateCustomerGroupsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public string Name { get; set; }
        public decimal DiscountRate { get; set; }
    }

    // Handler
    public class CreateCustomerGroupsCommandHandler : BaseHandler, IRequestHandler<CreateCustomerGroupsCommandRequest, ResponseDto<bool>>
    {
        public CreateCustomerGroupsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(CreateCustomerGroupsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PostAsync<CreateCustomerGroupsCommandRequest, bool>("api/CustomerGroup", request);
        }
    }
}
