using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.UserActionLogFeature.Queries
{
    public class GetAllUserActionLogsQueryRequest : IRequest<ResponseDto<List<UserActionLogDto>>>
    {
    }
}
