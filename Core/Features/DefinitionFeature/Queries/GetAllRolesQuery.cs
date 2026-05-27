using MediatR;
using naif_katalog.Models;
using naif_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace naif_katalog.Core.Features.DefinitionFeature.Queries
{
    // DTO
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Request
    public class GetAllRolesQueryRequest : IRequest<ResponseDto<List<RoleDto>>>
    {
    }

    // Handler
    public class GetAllRolesQueryHandler : BaseHandler, IRequestHandler<GetAllRolesQueryRequest, ResponseDto<List<RoleDto>>>
    {
        public GetAllRolesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<RoleDto>>> Handle(GetAllRolesQueryRequest request, CancellationToken cancellationToken)
        {
            return await _apiService.GetAsync<List<RoleDto>>("api/Roles");
        }
    }
}
