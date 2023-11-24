using AutoMapper;
using DirectFantasy.Criteria;
using Fanex.Settlement.ScaleOut.Request;

namespace DirectFantasy.WorkerService.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<MatchSettleCriteria, UpdateMatchInfoRequest>();
    }
}