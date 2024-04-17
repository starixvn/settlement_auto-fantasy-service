using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

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

    private readonly CriteriaService createCriteriaService;

    private readonly AppSettingsKey appSettingsKey;

    public CalcServiceImp(ScaleOutSettleService scaleOutService,
        IDynamicRepository repository,
        IMapper mapper,
        IOptions<AppSettingsKey> appSettingsKey,
        LogService logService,
        CriteriaService createCriteriaService)
    {
        this.scaleOutService = scaleOutService;
        this.repository = repository;
        this.mapper = mapper;
        this.appSettingsKey = appSettingsKey.Value;
        this.logService = logService;
        this.createCriteriaService = createCriteriaService;
    }

    public async Task Settle(Match match)
    {
        await scaleOutService.UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Waiting);

        var settleCriteria = createCriteriaService.CreateMatchSettleCriteria(match);

        await UpdateMatchInfo(settleCriteria);

        await WriteLog(match);

        ExecuteAllSiteDb(settleCriteria);
    }

    public async Task InsertAutoPayoutResult(Match match, int sourceId, int statusId)
    {
        var criteria = createCriteriaService.CreateAutoPayoutResult(match, sourceId, statusId);

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

    private async Task WriteLog(Match match)
    {
        var insertLogCriteria = createCriteriaService.CreateLogCalcCriteria(match);
        await logService.WriteLogResettle(insertLogCriteria);

        var message = GenerateMessage(match);
        await logService.WriteLogDirectly(match.MatchId, message);
    }

    private static string GenerateMessage(Match match)
    {
        var score = $"Score: {match.FinalHomeScore ?? 0}-{match.FinalAwayScore ?? 0} HT: {match.HtHomeScore ?? 0}-{match.HtAwayScore ?? 0}";
        return $"NAP: Auto Direct Calculate({match.Action}) for matchId: {match.MatchId} {score}";
    }
}