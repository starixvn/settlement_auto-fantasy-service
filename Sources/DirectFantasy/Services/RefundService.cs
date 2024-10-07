using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;

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
	private readonly CriteriaService createCriteriaService;
	private readonly MatchService matchService;

	public RefundServiceImp(
	IDynamicRepository repository,
	ScaleOutSettleService scaleOutSettleService,
	IOptions<AppSettingsKey> appSettingsKey,
	LogService logService,
	CriteriaService createCriteriaService,
	MatchService matchService)
	{
		this.appSettingsKey = appSettingsKey.Value;
		this.repository = repository;
		this.scaleOutSettleService = scaleOutSettleService;
		this.logService = logService;
		this.createCriteriaService = createCriteriaService;
		this.matchService = matchService;
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

		await ExecuteRefundPushAllDb(new RefundPushCriteria { MatchId = match.MatchId });

		await matchService.UpdateAutoRefundResult(match, MatchStatus.Completed);

	}

	private async Task WriteLog(Match match)
	{
		var insertLogCriteria = createCriteriaService.CreateLogRefundCriteria(match);
		await logService.WriteLogResettle(insertLogCriteria);

		var message = $"NAP: Auto Calculate {match.Action} for matchId:{match.MatchId} score FT: 0-0 HT: 0-0";
		await logService.WriteLogDirectly(match.MatchId, message);
	}


	private async Task ExecuteRefundPushAllDb(RefundPushCriteria criteria)
	{
		var tasks = appSettingsKey.SiteDbServer.Select(server => ExecuteOneSiteDb(criteria, server));
		await Task.WhenAll(tasks);
	}

	private async Task ExecuteOneSiteDb(RefundPushCriteria criteria, string server)
	{
		criteria.DbServer = server;
		await repository.ExecuteAsync(criteria);
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