using DirectFantasy.Criteria;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface LogService
{
    Task WriteLog(Match match, string message);

    Task WriteLogDirectly(int matchId, string message);

    Task WriteLogResettle(InsertLogCriteria criteria);
}

public class LogServiceImp : LogService
{
    private readonly IDynamicRepository repository;

    public LogServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

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

    public async Task WriteLogDirectly(int matchId, string message)
    {
        var criteria = new LogDirectlyCriteria
        {
            Extend = matchId,
            Doing = message
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task WriteLogResettle(InsertLogCriteria criteria)
    {
        await repository.ExecuteAsync(criteria);
    }
}