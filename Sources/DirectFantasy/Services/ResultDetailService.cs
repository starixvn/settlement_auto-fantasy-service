using DirectFantasy.Constants;
using DirectFantasy.Models;

namespace DirectFantasy.Services;

public record ResultTemp(string ResourceId, int Value);

public interface ResultDetailService
{
    List<ResultDetail> GenerateResults(FantasyMatch relatedMatches);

    List<ResultDetail> CreateRefundResults();
}

public class ResultDetailServiceImp : ResultDetailService
{
    public List<ResultDetail> CreateRefundResults()
    {
        var newResults = ResourceId.Basketball.Select(x => new ResultDetail
        {
            ResourceId = x,
            HomeValue = string.Empty,
            AwayValue = string.Empty,
        }).ToList();

        return newResults;
    }

    public List<ResultDetail> GenerateResults(FantasyMatch relatedMatches)
    {
        var homeTeamIds = relatedMatches.FantasyMatchDetail.FantasyHomeTeam.Split(",");
        var awayTeamIds = relatedMatches.FantasyMatchDetail.FantasyAwayTeam.Split(",");

        var homeValueResults = SumScore(relatedMatches.RelatedMatches, homeTeamIds);
        var awayValueResults = SumScore(relatedMatches.RelatedMatches, awayTeamIds);

        var results = homeValueResults.Join(awayValueResults, h => h.ResourceId, a => a.ResourceId,
            (h, a) => new ResultDetail
            {
                ResourceId = h.ResourceId,
                HomeValue = $"{h.Value}",
                AwayValue = $"{a.Value}",
            }).ToList();

        return results;
    }

    private List<ResultTemp> SumScore(List<RelatedMatch> relatedMatches, string[] teamIds)
    {
        var homes = relatedMatches
         .Where(x => teamIds.Contains(x.HomeId.ToString()))
         .Select(x => x.Results);

        var resultsOfHome = homes
            .SelectMany(x => x)
            .GroupBy(x => x.ResourceId)
            .Select(group => new ResultTemp(group.Key, group.Sum(x => int.TryParse(x.HomeValue, out int value) ? value : 0)));

        var aways = relatedMatches
            .Where(x => teamIds.Contains(x.AwayId.ToString()))
            .Select(x => x.Results);

        var resultsOfAway = aways.SelectMany(x => x)
             .GroupBy(x => x.ResourceId)
             .Select(group => new ResultTemp(group.Key, group.Sum(x => int.TryParse(x.AwayValue, out int value) ? value : 0)));

        var results = resultsOfHome
            .Concat(resultsOfAway)
            .GroupBy(x => x.ResourceId)
            .Select(group => new ResultTemp(group.Key, group.Sum(x => x.Value)))
            .ToList();

        return results;
    }
}