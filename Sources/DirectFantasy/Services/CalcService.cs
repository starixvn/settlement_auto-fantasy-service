using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface CalcService
{
    Task Settle(Match match);

    Task InsertAutoPayoutResult(Match match, int sourceId, int statusId, string remark = "");
}

public class CalcServiceImp(ScaleOutSettleService scaleOutService,
        IDynamicRepository repository,
        IMapper mapper,
        IOptions<AppSettingsKey> appSettingsKey,
        LogService logService,
        CriteriaService createCriteriaService)
    : CalcService
{
    public async Task Settle(Match match)
    {
        await scaleOutService.UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Waiting);

        var settleCriteria = createCriteriaService.CreateMatchSettleCriteria(match);

        await UpdateMatchInfo(settleCriteria);

        await WriteLog(match);

        await ExecuteAllSiteDb(settleCriteria);

        await UpdateAutoPayoutResult(match, Source.InfoSite, MatchStatus.Completed);
    }

    public async Task InsertAutoPayoutResult(Match match, int sourceId, int statusId, string remark = "")
    {
        var criteria = createCriteriaService.CreateAutoPayoutResult(match, sourceId, statusId, remark);

        await repository.ExecuteAsync(criteria);
    }

    private async Task UpdateAutoPayoutResult(Match match, int sourceId, int statusId)
    {
        var criteria = createCriteriaService.CreateAutoPayoutResultUpdCriteria(match, sourceId, statusId);

        await repository.ExecuteAsync(criteria);
    }

    private async Task UpdateMatchInfo(MatchSettleCriteria criteria)
    {
        var updateMatchInfoRequest = mapper.Map<UpdateMatchInfoRequest>(criteria);
        updateMatchInfoRequest.IsAuto = true;

        await scaleOutService.UpdateMatchInfo(updateMatchInfoRequest);
    }

    private async Task ExecuteAllSiteDb(MatchSettleCriteria criteria)
    {
        var tasks = appSettingsKey.Value.SiteDbServer.Select(server => ExecuteOneSiteDb(criteria, server));
        await Task.WhenAll(tasks);
    }

    private async Task ExecuteOneSiteDb(MatchSettleCriteria criteria, string dbServer)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);
    }

    private async Task WriteLog(Match match)
    {
        var insertLogCriteria = createCriteriaService.CreateLogCalcCriteria(match);
        await repository.ExecuteAsync(insertLogCriteria);

        var message = GenerateMessage(match);
        await logService.WriteLogDirectly(match.MatchId, message);
    }

    private static string GenerateMessage(Match match)
    {
        var score = $"Score: {match.FinalHomeScore ?? 0}-{match.FinalAwayScore ?? 0} HT: {match.HtHomeScore ?? 0}-{match.HtAwayScore ?? 0}";
        return $"NAP: Auto Direct Calculate({match.Action}) for matchId: {match.MatchId} {score}";
    }
}