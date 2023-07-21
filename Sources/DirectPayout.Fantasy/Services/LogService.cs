using AutoPayout.Criteria;
using DirectPayout.Fantasy.Criteria;
using DirectPayout.Fantasy.Models;
using Fanex.Data.Repository;

namespace DirectPayout.Fantasy.Services;

public interface LogService
{
    Task Write(Match match, string message);

    void WriteDirectly(Match match, string message);
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

    public void WriteDirectly(Match match, string message)
    {
        try
        {
            var criteria = new LogDirectlyCriteria
            {
                Extend = match.MatchId,
                Doing = GenerateLogData(match)
            };

            repository.Execute(criteria);
        }
        catch (Exception e)
        {
            Fanex.Logging.LogService.Error("DirectPayout.Fantasy (Write Log Directly): ", e);
        }
    }

    private static string GenerateLogData(Match match)
    {
        var score = $"score: {match.FinalHomeScore ?? 0}-{match.FinalAwayScore ?? 0} HT: {match.HtHomeScore ?? 0}-{match.HtAwayScore ?? 0}";
        var message = $"NAP: Auto Direct Calculate({match.Action}) for matchid: {match.MatchId} {score}";

        return message;
    }
}