namespace DirectFantasy.Services;

public interface DirectFantasyController
{
    Task ProcessAsync();
}

public class DirectFantasyControllerImp(MatchSettlementHandler matchSettlementHandler, MatchService matchService)
    : DirectFantasyController
{
    public async Task ProcessAsync()
    {
        var newMatches = (await matchService.PeekNewMatch())
            .Where(n => !n.IsChecking)
            .ToList();

        newMatches.ForEach(m =>
        {
            Task.Run(() => matchSettlementHandler.Progress(m))
               .ConfigureAwait(false);
        });
    }
}