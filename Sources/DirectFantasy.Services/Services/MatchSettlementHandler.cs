using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Exceptions;
using DirectFantasy.Core.Services;
using Match = DirectFantasy.Core.Models.Match;

namespace DirectFantasy.Services.Services;

public interface MatchSettlementHandler
{
    Task Progress(Match match);
}

public class MatchSettlementHandlerImp : MatchSettlementHandler
{
    private readonly MatchService matchService;
    private readonly ValidationMatchService validationMatchService;
    private readonly CalcService settleService;
    private readonly RefundService refundService;
    private readonly LogService logService;

    public MatchSettlementHandlerImp(
        MatchService fantasyMatchService,
        CalcService settleService,
        LogService logService,
        RefundService refundService,
        ValidationMatchService validationMatchService)
    {
        this.matchService = fantasyMatchService;
        this.settleService = settleService;
        this.logService = logService;
        this.refundService = refundService;
        this.validationMatchService = validationMatchService;
    }

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
        try
        {
            await HandleCalc(match);
        }
        catch (Exception ex)
        {
            await logService.Write(match, ex.Message);
            await settleService.InsertAutoPayoutResult(match, 0, MatchStatus.Failed);
        }
        finally
        {
            matchService.Remove(match);
        }
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
            await logService.Write(match, exception.Description);
            await matchService.UpdateMatchStatus(match, MatchStatus.Failed);
        }
        finally
        {
            matchService.Remove(match);
        }
    }

    private async Task HandleCalc(Match match)
    {
        var validateResult = await validationMatchService.ValidateTicket(match);

        if (validateResult.IsSuccessful)
        {
            await settleService.Settle(match);
            return;
        }

        Console.WriteLine(validateResult.ErrMsg);
        await logService.Write(match, validateResult.ErrMsg);
        await settleService.InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed);
    }

    private async Task HandleRefund(Match match)
    {
        await validationMatchService.ValidateTicketMatch(match);

        var result = await validationMatchService.ValidateTicket(match);

        if (result.IsSuccessful)
        {
            await refundService.HandleRefundPush(match);
        }
        else
        {
            await logService.Write(match, result.ErrMsg);
            await matchService.UpdateMatchRemark(match, result.ErrMsg, MatchStatus.Failed);
        }
    }
}