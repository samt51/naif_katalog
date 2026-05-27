using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class CustomerGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal DiscountRate { get; set; }
    }

    // Request
    public class GetAllCustomerGroupsQueryRequest : IRequest<ResponseDto<List<CustomerGroupDto>>>
    {
    }

    // Handler
    public class GetAllCustomerGroupsQueryHandler : BaseHandler, IRequestHandler<GetAllCustomerGroupsQueryRequest, ResponseDto<List<CustomerGroupDto>>>
    {
        public GetAllCustomerGroupsQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<CustomerGroupDto>>> Handle(GetAllCustomerGroupsQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<CustomerGroupDto>>("api/CustomerGroup");
        }
    }
}
