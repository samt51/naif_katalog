using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Commands
{
    // Request
    public class UpdateCustomerGroupsCommandRequest : IRequest<ResponseDto<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal DiscountRate { get; set; }
    }

    // Handler
    public class UpdateCustomerGroupsCommandHandler : BaseHandler, IRequestHandler<UpdateCustomerGroupsCommandRequest, ResponseDto<bool>>
    {
        public UpdateCustomerGroupsCommandHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<bool>> Handle(UpdateCustomerGroupsCommandRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.PutAsync<UpdateCustomerGroupsCommandRequest, bool>("api/CustomerGroup", request);
        }
    }
}
