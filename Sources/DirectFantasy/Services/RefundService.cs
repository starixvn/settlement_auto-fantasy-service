using DirectFantasy.Models;
using DirectFantasy.Criteria;

namespace DirectFantasy.Services;

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

    private readonly CreateCriteriaService createCriteriaService;

    public RefundServiceImp(
    IDynamicRepository repository,
    ScaleOutSettleService scaleOutSettleService,
    IOptions<AppSettingsKey> appSettingsKey,
    LogService logService,
    CreateCriteriaService createCriteriaService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
        this.logService = logService;
        this.createCriteriaService = createCriteriaService;
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

        await WriteLog(match);

        ExecuteRefundPush(match);
    }

    private async Task WriteLog(Match match)
    {
        var insertLogCriteria = createCriteriaService.CreateLogRefundCriteria(match);
        await logService.WriteLogResettle(insertLogCriteria);

        var message = $"NAP: Auto Calculate {match.Action} for matchId:{match.MatchId} score FT: 0-0 HT: 0-0";
        await logService.WriteLogDirectly(match.MatchId, message);
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