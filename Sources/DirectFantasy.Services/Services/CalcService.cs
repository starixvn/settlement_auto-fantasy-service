using AutoMapper;
using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Models;
using DirectFantasy.Core.Services;
using DirectFantasy.Services.Criteria;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace DirectPayout.Fantasy.Services;

public interface CalcService
{
    Task Settle(Match match);

    Task InsertAutoPayoutResult(Match match, int sourceId, int statusId);
}

public class CalcServiceImp : CalcService
{
    private readonly ScaleOutSettleService scaleOutService;
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly LogService logService;
    private readonly AppSettingsKey appSettingsKey;

    public CalcServiceImp(ScaleOutSettleService scaleOutService,
        IDynamicRepository repository,
        IMapper mapper,
        IOptions<AppSettingsKey> appSettingsKey,
        LogService logService)
    {
        this.scaleOutService = scaleOutService;
        this.repository = repository;
        this.mapper = mapper;
        this.appSettingsKey = appSettingsKey.Value;
        this.logService = logService;
    }

    public async Task Settle(Match match)
    {
        await scaleOutService.UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Waiting);

        var settleCriteria = new MatchSettleCriteria
        {
            MatchId = match.MatchId,
            HalfTimeAway = match.HtAwayScore,
            HalfTimeHome = match.HtHomeScore,
            FullTimeAway = match.FinalAwayScore,
            FullTimeHome = match.FinalHomeScore,
            Action = match.Action,
            SourceId = Source.InfoSite,
        };

        await UpdateMatchInfo(settleCriteria);

        WriteLogDirectly(match);

        ExecuteAllSiteDb(settleCriteria);
    }

    public async Task InsertAutoPayoutResult(Match match, int sourceId, int statusId)
    {
        var criteria = new AddAutoPayoutResultCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            SourceId = sourceId,
            StatusId = statusId,
            HomeValue = match.TypeId == SettleType.Halftime
                ? match.HtHomeScore ?? 0 : match.FinalHomeScore ?? 0,
            AwayValue = match.TypeId == SettleType.Halftime
                ? match.HtAwayScore ?? 0 : match.FinalAwayScore ?? 0,
        };

        await repository.ExecuteAsync(criteria);
    }

    private async Task UpdateMatchInfo(MatchSettleCriteria criteria)
    {
        var updateMatchInfoRequest = mapper.Map<UpdateMatchInfoRequest>(criteria);
        updateMatchInfoRequest.IsAuto = true;

        await scaleOutService.UpdateMatchInfo(updateMatchInfoRequest);
    }

    private void ExecuteAllSiteDb(MatchSettleCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }

    private void WriteLogDirectly(Match match)
    {
        var score = $"score: {match.FinalHomeScore ?? 0}-{match.FinalAwayScore ?? 0} HT: {match.HtHomeScore ?? 0}-{match.HtAwayScore ?? 0}";
        var message = $"NAP: Auto Direct Calculate({match.Action}) for matchid: {match.MatchId} {score}";

        logService.WriteDirectly(match.MatchId, message);
    }
}