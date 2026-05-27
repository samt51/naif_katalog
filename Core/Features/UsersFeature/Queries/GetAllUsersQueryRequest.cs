using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.UsersFeature.Queries
{
    public class GetAllUsersQueryRequest : IRequest<ResponseDto<List<UsersDto>>>
    {
    }
}
