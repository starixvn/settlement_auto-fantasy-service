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
        LogService logService,
        IOptions<AppSettingsKey> appSettingsKey,
        MatchService matchService)
    : RefundOSService
{
    private readonly AppSettingsKey appSettingsKey = appSettingsKey.Value;

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

        await matchService.UpdateMatchInfo(settleCriteria);

        await logService.WriteLogSettle(match, settleCriteria);

        ExecuteAllSiteDb(settleCriteria);
    }

    private void ExecuteAllSiteDb(SettleCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }
}