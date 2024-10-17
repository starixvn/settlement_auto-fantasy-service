using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface LogService
{
    Task WriteLog(Match match, string message);

    Task WriteLogDirectly(int matchId, string message);
    Task WriteLogResettle(InsertLogCriteria insertLogCriteria);
    Task WriteLogSettle(Match match, SettleCriteria settleCriteria);
}

public class LogServiceImp(IDynamicRepository repository) : LogService
{
    public async Task WriteLog(Match match, string message)
    {
        var criteria = new SettleLogCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            HtHomeScore = match.HtHomeScore,
            HtAwayScore = match.HtAwayScore,
            FinalHomeScore = match.FinalHomeScore,
            FinalAwayScore = match.FinalAwayScore,
            IsRefund = match.IsRefund,
            Remark = message
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task WriteLogResettle(InsertLogCriteria criteria)
    {
        await repository.ExecuteAsync(criteria);
    }


    public async Task WriteLogSettle(Match match, SettleCriteria settleCriteria)
    {
        var insertLogCriteria = new InsertLogCriteria
        {
            MatchId = settleCriteria.MatchId,
            Action = settleCriteria.Action,
            SelectedResources = settleCriteria.SelectedResources,
            SportType = match.SportType,
        };

        await repository.ExecuteAsync(insertLogCriteria);

        var message = GenerateMessage(match, settleCriteria);
        await WriteLogDirectly(match.MatchId, message);
    }

    public async Task WriteLogDirectly(int matchId, string message)
    {
        var criteria = new LogDirectlyCriteria
        {
            Extend = matchId,
            Doing = message
        };

        await repository.ExecuteAsync(criteria);
    }

    private static string GenerateMessage(Match match, SettleCriteria settleCriteria)
    {
        var score = $"results: {settleCriteria.SelectedResources}";
        return $"NAP: Auto Fantasy Basketball Calculate({match.Action}) for matchId: {match.MatchId} {score}";
    }
}