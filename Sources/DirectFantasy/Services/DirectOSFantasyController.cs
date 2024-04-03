using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectFantasy.Services;

namespace DirectFantasy.Services;

public interface DirectOSFantasyController
{
    Task ProcessAsync();
}

public class DirectOSFantasyControllerImp : DirectOSFantasyController
{
    private readonly OSMatchSettlementHandler matchSettlementHandler;
    private readonly OSMatchService matchService;

    public DirectOSFantasyControllerImp(OSMatchService matchService, OSMatchSettlementHandler matchSettlementHandler)
    {
        this.matchService = matchService;
        this.matchSettlementHandler = matchSettlementHandler;
    }

    public async Task ProcessAsync()
    {
        var newMatches = (await matchService.PeekNewMatch())
            .ToList();

        //Console.WriteLine($"Count match: {newMatches.Count}");

        newMatches.ForEach(m =>
        {
            Task.Run(() => matchSettlementHandler.Progress(m))
               .ConfigureAwait(false);
        });

        //foreach (var newMatch in newMatches)
        //{
        //    await matchSettlementHandler.Progress(newMatch);
        //}
    }
}