using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Services;

namespace DirectFantasy.Services;

public interface CalcOSService
{
    Task Settle(Match match, SettleCriteria settleCriteria);
}

public class CalcOSServiceImp : CalcOSService
{
    private readonly IDynamicRepository repository;
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly LogService logService;
    private readonly AppSettingsKey appSettingsKey;
    private readonly OSMatchService matchService;

    public CalcOSServiceImp(
        ScaleOutSettleService scaleOutSettleService,
        IDynamicRepository repository,
        IOptions<AppSettingsKey> appSettingsKey,
        LogService logService,
        OSMatchService matchService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.repository = repository;
        this.logService = logService;
        this.appSettingsKey = appSettingsKey.Value;
        this.matchService = matchService;
    }

    public async Task Settle(Match match, SettleCriteria settleCriteria)
    {
        await scaleOutSettleService.UpdateUnderSettlementFlag(settleCriteria.MatchId, 0, settleCriteria.Action);
        await matchService.UpdateStatusResultOS(settleCriteria, MatchStatus.Waiting);
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