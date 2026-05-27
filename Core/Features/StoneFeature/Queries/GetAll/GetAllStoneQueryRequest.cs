using MediatR;
using naif_katalog.Models;
using System.Collections.Generic;

namespace naif_katalog.Core.Features.StoneFeature.Queries
{
    public class GetAllStoneQueryRequest : IRequest<ResponseDto<List<GetAllStoneQueryResponse>>>
    {
    }
}
