using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface CalcOSService
{
    Task Settle(Match match, SettleCriteria settleCriteria);
}

public class CalcOSServiceImp(
    ScaleOutSettleService scaleOutSettleService,
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey,
    LogService logService,
    OSMatchService matchService) : CalcOSService
{

    public async Task Settle(Match match, SettleCriteria settleCriteria)
    {
        await scaleOutSettleService.UpdateUnderSettlementFlag(settleCriteria.MatchId, 0, settleCriteria.Action);
        await matchService.UpdateStatusResultOS(settleCriteria, MatchStatus.Waiting);
        await UpdateMatchInfo(settleCriteria);
        await logService.WriteLogSettle(match, settleCriteria);
        await ExecuteAllSiteDb(settleCriteria);
        await matchService.UpdateStatusResultOS(settleCriteria, MatchStatus.Completed);
    }

    private async Task UpdateMatchInfo(SettleCriteria criteria)
    {
        var requestUpdateInfo = new UpdateMatchInfoRequest
        {
            Action = criteria.Action,
            AdminId = criteria.AdminId,
            MatchId = criteria.MatchId,
            SelectedResources = criteria.SelectedResources,
            IsAuto = criteria.IsAuto,
        };

        await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
    }

    private async Task ExecuteAllSiteDb(SettleCriteria criteria)
    {
        var tasks = appSettingsKey.Value.SiteDbServer.Select(server => ExecuteOneSiteDb(criteria, server));
        await Task.WhenAll(tasks);
    }

    private async Task ExecuteOneSiteDb(SettleCriteria criteria, string dbServer)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);
    }
}