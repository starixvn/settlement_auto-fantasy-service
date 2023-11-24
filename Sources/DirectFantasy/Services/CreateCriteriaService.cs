using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;
using Fanex.Settlement.ScaleOut.Constants;

namespace DirectFantasy.Services;

public interface CreateCriteriaService
{
    MatchSettleCriteria CreateMatchSettleCriteria(Match match);

    AddAutoPayoutResultCriteria CreateAutoPayoutResult(Match match, int sourceId, int statusId);

    InsertLogCriteria CreateLogCalcCriteria(Match match);

    InsertLogCriteria CreateLogRefundCriteria(Match match);
}

public class CreateCriteriaServiceImp : CreateCriteriaService
{
    private const int Soccer = 1;

    private readonly XmlConvertService xmlConvertService;

    public CreateCriteriaServiceImp(XmlConvertService xmlConvertService)
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

    public AddAutoPayoutResultCriteria CreateAutoPayoutResult(Match match, int sourceId, int statusId)
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

        return xmlConvertService.Serialize(new Resources
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

        var selectedResources = xmlConvertService.Serialize(new Resources
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
}