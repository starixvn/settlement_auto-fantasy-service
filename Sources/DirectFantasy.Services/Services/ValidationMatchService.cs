using AutoMapper;
using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Exceptions;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Criteria;
using DirectFantasy.Services.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace DirectFantasy.Services.Services;

public interface ValidationMatchService
{
    Task ValidateTicketMatch(Match match);

    Task<Result> ValidateTicket(Match match);
}

public class ValidationMatchServiceImp : ValidationMatchService
{
    private readonly AppSettingsKey appSettingsKey;
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly GeneralService generalService;

    public ValidationMatchServiceImp(IOptions<AppSettingsKey> appSettingsKey,
        IDynamicRepository repository,
        IMapper mapper,
        GeneralService generalService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
        this.mapper = mapper;
        this.generalService = generalService;
    }

    public async Task<Result> ValidateTicket(Match match)
    {
        var request = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Action = match.Action,
            SpName = match.IsRefund ? SpName.RefundPush : SpName.PayoutPush,
            IsAuto = true,
            IsAutoRefund = match.IsRefund
        };

        var criteria = mapper.Map<EventTicketRequest, ValidateTicketCriteria>(request);
        var resultMessage = await generalService.ExecuteAsync(criteria);

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
                TypeId = 1, // Single
                DbServer = server
            };

            var ticketInfo = await repository.GetAsync<MatchTicketInfo>(criteria) ?? new MatchTicketInfo();
            listTicketInfo.Add(ticketInfo);
        }

        return listTicketInfo;
    }
}