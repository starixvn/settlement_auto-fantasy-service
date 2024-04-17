using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using Fanex.Settlement.ScaleOut.Services;

namespace DirectFantasy.Services;

public interface RefundOSService
{
    Task InsResultRefundOS(Match match, string resourceIds);

    Task HandleRefundPush(Match match, SettleCriteria settleCriteria);
}

public class RefundOSServiceImp : RefundOSService
{
    private readonly IDynamicRepository repository;
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly LogService logService;
    private readonly AppSettingsKey appSettingsKey;

    public RefundOSServiceImp(
        IDynamicRepository repository,
        ScaleOutSettleService scaleOutSettleService,
        LogService logService,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
        this.logService = logService;
        this.appSettingsKey = appSettingsKey.Value;
    }

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

        await WriteLog(match, settleCriteria);

        ExecuteAllSiteDb(settleCriteria);
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

    private async Task WriteLog(Match match, SettleCriteria settleCriteria)
    {
        var insertLogCriteria = new Criteria.InsertLogCriteria()
        {
            MatchId = settleCriteria.MatchId,
            Action = settleCriteria.Action,
            SelectedResources = settleCriteria.SelectedResources,
            SportType = match.SportType,
        };

        await logService.WriteLogResettle(insertLogCriteria);

        var message = GenerateMessage(match, settleCriteria);
        await logService.WriteLogDirectly(match.MatchId, message);
    }

    private static string GenerateMessage(Match match, SettleCriteria settleCriteria)
    {
        var score = $"results: {settleCriteria.SelectedResources}";
        return $"NAP: Auto Fantasy Basketball Calculate({match.Action}) for matchId: {match.MatchId} {score}";
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