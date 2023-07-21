namespace DirectPayout.Fantasy.Services;

public interface DirectPayoutController
{
    Task ProcessAsync();
}

public class DirectPayoutControllerImp : DirectPayoutController
{
    private readonly MatchSettlementHandler matchService;
    private readonly FantasyMatchService fantasyMatchService;

    public DirectPayoutControllerImp(MatchSettlementHandler matchService, FantasyMatchService fantasyMatchService)
    {
        this.matchService = matchService;
        this.fantasyMatchService = fantasyMatchService;
    }

    public async Task ProcessAsync()
    {
        var newMatches = await fantasyMatchService.PeekNewMatch();
        newMatches.ForEach(m =>
        {
            Task.Run(() => matchService.Progress(m))
               .ConfigureAwait(false);
        });
    }
}