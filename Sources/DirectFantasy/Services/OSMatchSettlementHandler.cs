using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Exceptions;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public interface OSMatchSettlementHandler
{
    Task Progress(Match match);
}

public class OSMatchSettlementHandlerImp(OSMatchService matchService,
        CriteriaService criteriaService,
        ValidationMatchService validationMatchService,
        CalcOSService calcOSService,
        LogService logService,
        RefundOSService refundOSService,
        ResultDetailService resultDetailService)
    : OSMatchSettlementHandler
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
        try
        {
            await HandleCalc(match);
        }
        catch (AppException ex)
        {
            await logService.WriteLog(match, ex.Description);
            await matchService.UpdateStatusResultOS(match, MatchStatus.Failed, ex.Description);
        }
        finally
        {
            matchService.Remove(match);
        }
    }

    private async Task ProgressRefund(Match match)
    {
        await refundOSService.InsResultRefundOS(match, string.Join(",", ResourceId.Basketball));
        match.ResultDetails = resultDetailService.CreateRefundResults();
        await validationMatchService.ValidateTicketMatch(match);
        var settledCriteria = criteriaService.CreateOSSettleCriteria(match);

        try
        {
            await HandleRefund(match, settledCriteria);
        }
        catch (MarkedTicketException exception)
        {
            await logService.WriteLog(match, exception.Description);
            await matchService.UpdateStatusResultOS(settledCriteria, MatchStatus.Failed);
        }
        finally
        {
            matchService.Remove(match);
        }
    }

    private async Task HandleCalc(Match match)
    {
        var relatedMatch = await matchService.GetRelatedMatches(match.MatchId);

        validationMatchService.ValidateRelatedMatch(relatedMatch);

        match.ResultDetails = resultDetailService.GenerateResults(relatedMatch);

        var settledCriteria = criteriaService.CreateOSSettleCriteria(match);

        var validateResult = await validationMatchService.ValidateEventTicketOS(settledCriteria);

        if (validateResult.IsSuccessful)
        {
            await calcOSService.Settle(match, settledCriteria);
            return;
        }

        var errorMsg = $"Failed Basketball Fantasy Payout: {validateResult.ErrMsg}";
        await logService.WriteLog(match, errorMsg);
        await matchService.UpdateStatusResultOS(settledCriteria, MatchStatus.Failed, errorMsg);
    }

    private async Task HandleRefund(Match match, SettleCriteria settledCriteria)
    {
        var validateResult = await validationMatchService.ValidateEventTicketOS(settledCriteria);

        if (validateResult.IsSuccessful)
        {
            await refundOSService.HandleRefundPush(match, settledCriteria);
            return;
        }

        await logService.WriteLog(match, $"Failed Basketball Fantasy Payout: {validateResult.ErrMsg}");
        await matchService.UpdateStatusResultOS(settledCriteria, MatchStatus.Failed);
    }
}