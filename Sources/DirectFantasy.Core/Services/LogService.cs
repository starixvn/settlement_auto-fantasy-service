using DirectFantasy.Core.Criteria;
using DirectFantasy.Core.Models;
using Fanex.Data.Repository;

namespace DirectFantasy.Core.Services;

public interface LogService
{
    Task Write(Match match, string message);

    void WriteDirectly(int matchId, string message);
}

public class LogServiceImp : LogService
{
    private readonly IDynamicRepository repository;

    public LogServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task Write(Match match, string message)
    {
        var criteria = new SettleLogCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            HtHomeScore = match.HtHomeScore,
            HtAwayScore = match.HtAwayScore,
            FinalHomeScore = match.FinalHomeScore,
            FinalAwayScore = match.FinalAwayScore,
            Remark = message
        };

        await repository.ExecuteAsync(criteria);
    }

    public void WriteDirectly(int matchId, string message)
    {
        try
        {
            var criteria = new LogDirectlyCriteria
            {
                Extend = matchId,
                Doing = message
            };

            repository.Execute(criteria);
        }
        catch (Exception e)
        {
            Fanex.Logging.LogService.Error("DirectFantasy (Write Log Directly): ", e);
        }
    }
}