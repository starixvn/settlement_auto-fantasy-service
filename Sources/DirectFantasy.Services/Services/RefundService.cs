using DirectFantasy.Core.Models;
using DirectFantasy.Core.Services;
using DirectFantasy.Services.Criteria;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace DirectFantasy.Services.Services;

public interface RefundService
{
    Task InsertRefundResult(Match match);

    Task HandleRefundPush(Match match);
}

public class RefundServiceImp : RefundService
{
    private readonly IDynamicRepository repository;
    private readonly AppSettingsKey appSettingsKey;
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly LogService logService;

    public RefundServiceImp(
    IDynamicRepository repository,
    ScaleOutSettleService scaleOutSettleService,
    IOptions<AppSettingsKey> appSettingsKey,
    LogService logService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
        this.logService = logService;
    }

    public async Task InsertRefundResult(Match match)
    {
        var criteria = new RefundResultInsertCriteria
        {
            ParentId = match.MatchId,
            TypeId = match.TypeId,
        };
        await repository.ExecuteAsync(criteria);
    }

    public async Task HandleRefundPush(Match match)
    {
        await scaleOutSettleService.UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await UpdateMatchRefundInfo(match);

        WriteLogDirectly(match);

        ExecuteRefundPush(match);
    }

    private void WriteLogDirectly(Match match)
    {
        var message = $"NAP: Auto Calculate {match.Action} for matchId:{match.MatchId} score FT: 0-0 HT: 0-0";
        logService.WriteDirectly(match.MatchId, message);
    }

    private void ExecuteRefundPush(Match match)
    {
        var criteria = new RefundPushCriteria
        {
            MatchId = match.MatchId
        };

        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }

    private async Task UpdateMatchRefundInfo(Match match)
    {
        var requestUpdateInfo = new UpdateMatchInfoRequest
        {
            MatchId = match.MatchId,
            Action = match.Action,
            AdminId = 0,
        };

        await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
    }
}