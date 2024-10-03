using DirectFantasy.Constants;
using DirectFantasy.Exceptions;
using Match = DirectFantasy.Models.Match;

namespace DirectFantasy.Services;

public interface MatchSettlementHandler
{
    Task Progress(Match match);
}


public class MatchSettlementHandlerImp(MatchService fantasyMatchService,
        CalcService settleService,
        LogService logService,
        RefundService refundService,
        ValidationMatchService validationMatchService)
    : MatchSettlementHandler
{
    public async Task Progress(Match match)
    {
        if (match.IsRefund)
        {
            await ProgressRefund(match);
            return;
        }

        await ProgressCalc(match);
    }

    private async Task ProgressCalc(Match match)
    {
        await HandleCalc(match);
        matchService.Remove(match);
    }

    private async Task ProgressRefund(Match match)
    {
        match.IsChecking = true;
        await refundService.InsertRefundResult(match);

        try
        {
            await HandleRefund(match);
        }
        catch (MarkedTicketException exception)
        {
            await logService.WriteLog(match, exception.Description);
            await matchService.UpdateAutoRefundResult(match, MatchStatus.Failed);
        }
        finally
        {
            matchService.Remove(match);
        }
    }

    private async Task HandleCalc(Match match)
    {
        var validateResult = await validationMatchService.ValidateEventTicket(match);

        if (validateResult.IsSuccessful)
        {
            await settleService.Settle(match);
            return;
        }

        await logService.WriteLog(match, $"Failed Direct Payout: {validateResult.ErrMsg}");
        await settleService.InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed, validateResult.ErrMsg);
    }

    private async Task HandleRefund(Match match)
    {
        await validationMatchService.ValidateTicketMatch(match);

        var result = await validationMatchService.ValidateEventTicket(match);

        if (result.IsSuccessful)
        {
            await refundService.HandleRefundPush(match);
            return;
        }

        await logService.WriteLog(match, $"Auto Refund failed: {result.ErrMsg}");
        await fantasyMatchService.UpdateMatchRemark(match, result.ErrMsg, MatchStatus.Failed);
    }
}