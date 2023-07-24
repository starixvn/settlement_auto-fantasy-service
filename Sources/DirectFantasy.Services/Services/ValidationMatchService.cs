using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Exceptions;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Criteria;
using DirectFantasy.Services.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace DirectFantasy.Services.Services;

public interface ValidationMatchService
{
    Task ValidateTicketMatch(Match match);

    Task<Result> ValidateEventAndTicket(Match match);
}

public class ValidationMatchServiceImp : ValidationMatchService
{
    private readonly AppSettingsKey appSettingsKey;
    private readonly IDynamicRepository repository;
    private readonly ScaleOutSettleService scaleOutSettleService;

    public ValidationMatchServiceImp(IOptions<AppSettingsKey> appSettingsKey,
        IDynamicRepository repository,
        ScaleOutSettleService scaleOutSettleService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
    }

    public async Task<Result> ValidateEventAndTicket(Match match)
    {
        var request = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Action = match.Action,
            SpName = match.IsRefund ? SpName.RefundPush : SpName.PayoutPush,
            IsAuto = true,
            IsAutoRefund = match.IsRefund
        };

        var resultMessage = await scaleOutSettleService.ValidateEventAndTicket(request);

        return new Result
        {
            ErrCode = resultMessage.ErrorCode,
            ErrMsg = resultMessage.ErrorMessage
        };
    }

    public async Task ValidateTicketMatch(Match match)
    {
        var listTicketInfo = await GetAllTicketInfo(match);

        if (listTicketInfo.Exists(item => item.IsExistMarkedTicket))
        {
            throw new MarkedTicketException();
        }
    }

    private async Task<List<MatchTicketInfo>> GetAllTicketInfo(Match match)
    {
        var listTicketInfo = new List<MatchTicketInfo>();

        foreach (var server in appSettingsKey.SiteDbServer)
        {
            var criteria = new MatchTicketInfoCriteria
            {
                MatchId = match.MatchId,
                TypeId = match.TypeId,
                DbServer = server
            };

            var ticketInfo = await repository.GetAsync<MatchTicketInfo>(criteria) ?? new MatchTicketInfo();
            listTicketInfo.Add(ticketInfo);
        }

        return listTicketInfo;
    }
}