using AutoPayout.Constants;
using Match = DirectPayout.Fantasy.Models.Match;

namespace DirectPayout.Fantasy.Services;

public interface MatchSettlementHandler
{
    Task Progress(Match match);
}

public class MatchSettlementHandlerImp : MatchSettlementHandler
{
    private readonly FantasyMatchService fantasyMatchService;
    private readonly SettleService settleService;
    private readonly LogService logService;

    public MatchSettlementHandlerImp(
        FantasyMatchService fantasyMatchService,
        SettleService settleService,
        LogService logService)
    {
        this.fantasyMatchService = fantasyMatchService;
        this.settleService = settleService;
        this.logService = logService;
    }

    public async Task Progress(Match match)
    {
        try
        {
            await HandleSettle(match);
        }
        catch (Exception ex)
        {
            await logService.Write(match, ex.Message);
            await settleService.InsertAutoPayoutResult(match, 0, MatchStatus.Failed);
        }
        finally
        {
            fantasyMatchService.Remove(match);
        }
    }

    private async Task HandleSettle(Match match)
    {
        var validateResult = await settleService.VerifyMatch(match);

        if (validateResult.IsSuccessful)
        {
            logService.WriteDirectly(match, match.Action);
            await settleService.Settle(match);
        }
        else
        {
            await logService.Write(match, validateResult.ErrMsg);
            await settleService.InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed);
        }
    }
}