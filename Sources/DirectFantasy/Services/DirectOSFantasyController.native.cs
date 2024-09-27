namespace DirectFantasy.Services;

public interface DirectOSFantasyController
{
    Task ProcessAsync();
}


public class DirectOSFantasyControllerImp(OSMatchService matchService, OSMatchSettlementHandler matchSettlementHandler)
    : DirectOSFantasyController
{
    public async Task ProcessAsync()
    {
        var newMatches = (await matchService.PeekNewMatch())
            .ToList();

        newMatches.ForEach(m =>
        {
            Task.Run(() => matchSettlementHandler.Progress(m))
               .ConfigureAwait(false);
        });
    }
}