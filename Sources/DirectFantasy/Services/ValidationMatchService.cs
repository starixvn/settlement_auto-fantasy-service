using AutoMapper.Internal;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Exceptions;
using DirectFantasy.Models;
using Fanex.Settlement.ScaleOut.Criteria.Normal;

namespace DirectFantasy.Services;

public interface ValidationMatchService
{
    Task ValidateTicketMatch(Match match);

    void ValidateRelatedMatch(FantasyMatch match);

    Task<Result> ValidateEventTicket(Match match);

    Task<Result> ValidateEventTicketOS(SettleCriteria settleCriteria);
}

public class ValidationMatchServiceImp(IOptions<AppSettingsKey> appSettingsKey,
        IDynamicRepository repository,
        IMapper mapper,
        GeneralService generalService)
    : ValidationMatchService
{
    public async Task<Result> ValidateEventTicket(Match match)
    {
        var request = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Action = match.Action,
            SpName = match.IsRefund ? SpName.RefundPush : SpName.PayoutPush,
            IsAuto = true,
            IsAutoRefund = match.IsRefund,
        };

        var criteria = mapper.Map<EventTicketRequest, ValidateTicketCriteria>(request);
        var resultMessage = await generalService.ExecuteAsync(criteria);

        return new Result
        {
            ErrCode = resultMessage.ErrorCode,
            ErrMsg = resultMessage.ErrorMessage
        };
    }

    public async Task<Result> ValidateEventTicketOS(SettleCriteria settleCriteria)
    {
        var request = new EventTicketRequest
        {
            MatchId = settleCriteria.MatchId,
            Action = settleCriteria.Action,
            SpName = settleCriteria.SpName,
            IsAuto = true,
            SelectedResources = settleCriteria.SelectedResources,
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

    public void ValidateRelatedMatch(FantasyMatch match)
    {
        ValidateInsufficient(match);
        ValidateHoldStatus(match);
        ValidateCalculatedMatch(match);
    }

    private void ValidateCalculatedMatch(FantasyMatch fantasyMatch)
    {
        foreach (var item in fantasyMatch.RelatedMatches)
        {
            if (item.EventStatus != EventStatus.Completed)
            {
                throw new RelatedMatchesNotCalculatedException();
            }
        }
    }

    private void ValidateInsufficient(FantasyMatch fantasyMatch)
    {
        var homeTeamIds = fantasyMatch.FantasyMatchDetail.FantasyHomeTeam.Split(",");
        var awayTeamIds = fantasyMatch.FantasyMatchDetail.FantasyAwayTeam.Split(",");

        var notExistsMatch = homeTeamIds
            .Concat(awayTeamIds)
            .Any(x =>
            {
                var exitsMatch = fantasyMatch.RelatedMatches.Find(m => m.HomeId == int.Parse(x) || m.AwayId == int.Parse(x));
                return exitsMatch is null;
            });

        if (notExistsMatch)
        {
            throw new RelatedMatchesInsufficientException();
        }
    }

    private static void ValidateHoldStatus(FantasyMatch fantasyMatch)
    {
        foreach (var item in fantasyMatch.RelatedMatches)
        {
            VerifyHoldStatus(item.Remark);
        }
    }

    private static void VerifyHoldStatus(RemarkSettingModel remark)
    {
        var conditionsToCheck = new[] { remark.IsMtdHold, remark.IsUndecided, remark.IsPending, remark.IsUnderSettle, remark.IsKeyHold };

        if (Array.Exists(conditionsToCheck, (item => item)))
        {
            throw new HoldStatusException();
        }
    }

    private async Task<List<MatchTicketInfo>> GetAllTicketInfo(Match match)
    {
        var listTicketInfo = new List<MatchTicketInfo>();

        foreach (var server in appSettingsKey.Value.SiteDbServer)
        {
            var criteria = new MatchTicketInfoCriteria
            {
                MatchId = match.MatchId,
                DbServer = server
            };

            var ticketInfo = await repository.GetAsync<MatchTicketInfo>(criteria) ?? new MatchTicketInfo();
            listTicketInfo.Add(ticketInfo);
        }

        return listTicketInfo;
    }
}