using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Exceptions;
using DirectFantasy.Core.Models;
using DirectFantasy.Core.Services;
using DirectFantasy.Services.Services;
using Microsoft.Extensions.Options;
using Match = DirectFantasy.Core.Models.Match;

namespace DirectPayout.Fantasy.Services;

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
    private readonly AppSettingsKey appSettingsKey;

    public MatchSettlementHandlerImp(
        MatchService fantasyMatchService,
        CalcService settleService,
        LogService logService,
        RefundService refundService,
        IOptions<AppSettingsKey> appSettingsKey,

        ValidationMatchService validationMatchService)
    {
        this.matchService = fantasyMatchService;
        this.settleService = settleService;
        this.logService = logService;
        this.refundService = refundService;
        this.validationMatchService = validationMatchService;
        this.appSettingsKey = appSettingsKey.Value;
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

        var retryCount = 0;

        while (retryCount <= appSettingsKey.RetryCount)
        {
            try
            {
                await HandleRefund(match);
            }
            catch (MarkedTicketException exception)
            {
                retryCount = await ProcessWhenCachedException(retryCount, match, exception);
            }
        }
    }

    private async Task HandleCalc(Match match)
    {
        var validateResult = await validationMatchService.ValidateEventAndTicket(match);

        if (validateResult.IsSuccessful)
        {
            logService.WriteDirectly(match.MatchId, match.Action);
            await settleService.Settle(match);
            return;
        }

        await logService.Write(match, validateResult.ErrMsg);
        await settleService.InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed);
    }

    private async Task HandleRefund(Match match)
    {
        await validationMatchService.ValidateTicketMatch(match);

        var result = await validationMatchService.ValidateEventAndTicket(match);

        if (result.IsSuccessful)
        {
            await refundService.HandleRefundPush(match);
        }
        else
        {
            await logService.Write(match, result.ErrMsg);
            await matchService.UpdateMatchRemark(match, result.ErrMsg, MatchStatus.Failed);
        }

        matchService.Remove(match);
    }

    private async Task<int> ProcessWhenCachedException(int retryCount, Match match, AutoRefundException exception)
    {
        await matchService.UpdateMatchRemark(match, exception.Description, MatchStatus.Running);

        if (retryCount >= appSettingsKey.RetryCount)
        {
            await logService.Write(match, exception.Description);
            matchService.RemoveAndUpdateStatus(match, MatchStatus.Failed);
        }
        else
        {
            await Task.Delay(appSettingsKey.RetryInterval);
        }

        return retryCount + 1;
    }
}