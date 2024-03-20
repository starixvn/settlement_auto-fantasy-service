using System.Resources;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using Fanex.Settlement.ScaleOut.Constants;

namespace DirectFantasy.Services;

public interface CriteriaService
{
    MatchSettleCriteria CreateMatchSettleCriteria(Match match);

    AddAutoPayoutResultCriteria CreateAutoPayoutResult(Match match, int sourceId, int statusId, string remark = "");

    InsertLogCriteria CreateLogCalcCriteria(Match match);

    InsertLogCriteria CreateLogRefundCriteria(Match match);

    SettleCriteria CreateOSSettleCriteria(Match match);
}

public class CriteriaServiceImp : CriteriaService
{
    private const int Soccer = 1;

    private readonly XmlConvertService xmlConvertService;

    public CriteriaServiceImp(XmlConvertService xmlConvertService)
    {
        this.xmlConvertService = xmlConvertService;
    }

    public MatchSettleCriteria CreateMatchSettleCriteria(Match match)
    {
        return new MatchSettleCriteria
        {
            MatchId = match.MatchId,
            HalfTimeAway = match.HtAwayScore,
            HalfTimeHome = match.HtHomeScore,
            FullTimeAway = match.FinalAwayScore,
            FullTimeHome = match.FinalHomeScore,
            Action = match.Action,
            SourceId = Source.InfoSite,
        };
    }

    public AddAutoPayoutResultCriteria CreateAutoPayoutResult(Match match, int sourceId, int statusId, string remark = "")
    {
        var homeValue = match.HtHomeScore ?? 0;
        var awayValue = match.HtAwayScore ?? 0;

        if (match.TypeId == SettleType.FullTime)
        {
            homeValue = match.FinalHomeScore ?? 0;
            awayValue = match.FinalAwayScore ?? 0;
        }

        return new AddAutoPayoutResultCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            SourceId = sourceId,
            StatusId = statusId,
            Remark = remark,
            HomeValue = homeValue,
            AwayValue = awayValue
        };
    }

    public InsertLogCriteria CreateLogCalcCriteria(Match match)
    {
        var resources = GetLogResource(match);
        var actionName = match.TypeId == SettleType.Halftime ? ActionName.CalculateHalfTime : ActionName.CalculateFullTime;
        return new InsertLogCriteria
        {
            MatchId = match.MatchId,
            Action = actionName,
            SportType = Soccer,
            SelectedResources = resources
        };
    }

    private string GetLogResource(Match match)
    {
        var resources = new List<ResourceItem>();

        var homeValue = match.HtHomeScore ?? 0;
        var awayValue = match.HtAwayScore ?? 0;
        var resourceId = ResourceId.Halftime;

        if (match.TypeId == SettleType.FullTime)
        {
            homeValue = match.FinalHomeScore ?? 0;
            awayValue = match.FinalAwayScore ?? 0;
            resourceId = ResourceId.Final;
        }

        var item = new ResourceItem
        {
            ResourceId = resourceId,
            HomeValue = homeValue.ToString(),
            AwayValue = awayValue.ToString()
        };
        resources.Add(item);

        return xmlConvertService.Serialize(new Resources<ResourceItem>
        {
            ResourceItems = resources
        });
    }

    public InsertLogCriteria CreateLogRefundCriteria(Match match)
    {
        var resources = new List<ResourceItem>
        {
            new() { ResourceId = ResourceId.Halftime, HomeValue = string.Empty, AwayValue = string.Empty },
            new() { ResourceId = ResourceId.Final, HomeValue = string.Empty, AwayValue = string.Empty }
        };

        var selectedResources = xmlConvertService.Serialize(new Resources<ResourceItem>
        {
            ResourceItems = resources
        });

        return new InsertLogCriteria
        {
            MatchId = match.MatchId,
            Action = SettleAction.Refund,
            SportType = Soccer,
            SelectedResources = selectedResources
        };
    }

    public SettleCriteria CreateOSSettleCriteria(Match match)
    {
        var selectedResources = xmlConvertService.Serialize(new Resources<ResultDetail>
        {
            ResourceItems = match.ResultDetails.ToList()
        });

        return new SettleCriteria
        {
            MatchId = match.MatchId,
            Action = match.Action,
            SelectedResources = selectedResources,
            AutoType = match.IsRefund ? null : 2// direct
        };
    }
}