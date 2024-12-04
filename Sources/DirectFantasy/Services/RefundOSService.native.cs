using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface RefundOSService
{
    Task InsResultRefundOS(Match match, string resourceIds);

    Task HandleRefundPush(Match match, SettleCriteria settleCriteria);
}

public class RefundOSServiceImp(IDynamicRepository repository,
        ScaleOutSettleService scaleOutSettleService,
        IOptions<AppSettingsKey> appSettingsKey,
        OSMatchService matchService,
        LogService logService)
    : RefundOSService
{
    public async Task InsResultRefundOS(Match match, string resourceIds)
    {
        var criteria = new ResultRefundOSInsCriteria
        {
            MatchId = match.MatchId,
            ResourceIds = resourceIds
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task HandleRefundPush(Match match, SettleCriteria settleCriteria)
    {
        await scaleOutSettleService.UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await UpdateMatchInfo(settleCriteria);

        await matchService.UpdateStatusResultOS(settleCriteria, MatchStatus.Waiting);
        
        await logService.WriteLogSettle(match, settleCriteria);
        
        await ExecuteAllSiteDb(settleCriteria);

        await matchService.UpdateStatusResultOS(settleCriteria, MatchStatus.Completed);
    }
    private async Task ExecuteAllSiteDb(SettleCriteria criteria)
    {
        var servers = appSettingsKey.Value.SiteDbServer;
        var tasks = servers.Select(server => ExecuteOneSiteDb(criteria, server));
        await Task.WhenAll(tasks);
    }

    private async Task ExecuteOneSiteDb(SettleCriteria criteria, string server)
    {
        criteria.DbServer = server;
        await repository.ExecuteAsync(criteria);
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
}