using AutoMapper;
using AutoPayout.Constants;
using AutoPayout.Criteria;
using AutoPayout.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using Match = DirectPayout.Fantasy.Models.Match;
using SettleType = AutoPayout.Constants.SettleType;

namespace DirectPayout.Fantasy.Services;

public interface SettleService
{
    Task<Result> VerifyMatch(Match match);

    Task Settle(Match match);

    Task InsertAutoPayoutResult(Match match, int sourceId, int statusId);
}

public class SettleServiceImp : SettleService
{
    private readonly ScaleOutSettleService scaleOutService;
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly AppSettingsKey appSettingsKey;

    public SettleServiceImp(ScaleOutSettleService scaleOutService,
        IDynamicRepository repository,
        IMapper mapper,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.scaleOutService = scaleOutService;
        this.repository = repository;
        this.mapper = mapper;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public async Task<Result> VerifyMatch(Match match)
    {
        var eventTicketRequest = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Mode = match.TypeId == SettleType.Halftime ? Mode.HalfTime : Mode.FullTime,
            Action = match.Action,
            IsAuto = true,
            SpName = "NAP_Admin_Settlement_1_AutoPayout_Push",
        };

        var result = await scaleOutService.ValidateEventAndTicket(eventTicketRequest);

        return new()
        {
            ErrCode = result.ErrorCode,
            ErrMsg = result.ErrorMessage,
        };
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
        var updateMatchInfoRequest = mapper.Map<MatchSettleCriteria, UpdateMatchInfoRequest>(criteria);
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
}