namespace DirectFantasy.Services;

public interface DirectFantasyController
{
    Task ProcessAsync();
}

public class DirectFantasyControllerImp : DirectFantasyController
{
    private readonly MatchSettlementHandler matchSettlementHandler;
    private readonly MatchService matchService;

    public DirectFantasyControllerImp(MatchSettlementHandler matchSettlementHandler, MatchService matchService)
    {
        this.matchSettlementHandler = matchSettlementHandler;
        this.matchService = matchService;
    }

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