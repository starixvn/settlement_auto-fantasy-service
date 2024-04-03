using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using Fanex.Caching;
using Environment = DirectFantasy.Constants.Environment;
using LogServiceSentry = Fanex.Logging.LogService;

namespace DirectFantasy.Services;

public interface OSMatchService
{
    Task<FantasyMatch> GetRelatedMatches(int matchId);

    Task<List<Match>> PeekNewMatch();

    void Remove(Match match);

    Task UpdateStatusResultOS(SettleCriteria settleCriteria, int status);

    Task UpdateStatusResultOS(Match match, int status);
}

public class OSMatchServiceImp : OSMatchService
{
    private readonly object osMatchesLock = new();

    public List<Match> PoolOsMatches { get; } = new();

    private readonly IDynamicRepository repository;

    private readonly ICache cache;

    private readonly AppSettingsKey appSettingsKey;

    public OSMatchServiceImp(
        IDynamicRepository repository,
        ICache cache,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.cache = cache;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public void Remove(Match match)
    {
        lock (osMatchesLock)
        {
            var count = PoolOsMatches.RemoveAll(m => m.MatchId == match.MatchId && m.TypeId == match.TypeId);
        }
    }

    public async Task<List<Match>> PeekNewMatch()
    {
        var fantasyMatches = await GetMatchesByLeagues();

        lock (osMatchesLock)
        {
            var newMatches = fantasyMatches.Except(PoolOsMatches).ToList();
            PoolOsMatches.AddRange(newMatches);
            return newMatches;
        }
    }

    public async Task UpdateStatusResultOS(SettleCriteria settleCriteria, int status)
    {
        var criteria = new ResultOSUpdateCriteria
        {
            MatchId = settleCriteria.MatchId,
            SelectedResources = settleCriteria.SelectedResources,
            StatusId = status,
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task UpdateStatusResultOS(Match match, int status)
    {
        var criteria = new ResultOSUpdateCriteria
        {
            MatchId = match.MatchId,
            StatusId = status,
        };

        await repository.ExecuteAsync(criteria);
    }

    private async Task<List<Match>> GetMatchesByLeagues()
    {
        var fantasyMatches = await GetOSMatches();

        var (_, response) = await cache.TryGetAsync<string>(CacheKey.OsLeagueId);
        var leagueIds = (response ?? "").Split(",");

        return appSettingsKey.EnvironmentName switch
        {
            Environment.Pro => leagueIds.Length > 0
                ? fantasyMatches.Where(m => !leagueIds.Contains(m.LeagueId.ToString())).ToList()
                : fantasyMatches,
            Environment.Uat => fantasyMatches.Where(m => leagueIds.Contains(m.LeagueId.ToString())).ToList(),
            _ => fantasyMatches
        };
    }

    private async Task<List<Match>> GetOSMatches()
    {
        var criteria = new OSMatchesSelCriteria();
        var fantasyMatches = await repository.FetchAsync<Match>(criteria);
        return fantasyMatches.ToList();
    }

    public async Task<FantasyMatch> GetRelatedMatches(int matchId)
    {
        var criteria = new RelatedMatchesSelCriteria
        {
            MatchId = matchId
        };

        var (firstResultSet, secondResultSet) = await repository.FetchMultipleAsync<FantasyMatchDetail, FantasyRelatedMatch>(criteria);

        var fansFantasyRelatedMatches = secondResultSet.OrderBy(match => match.MatchCode).ToList();

        var relatedMatches = GetRelatedMatchesFromFantasyMatches(fansFantasyRelatedMatches);

        return new FantasyMatch
        {
            FantasyMatchDetail = firstResultSet.FirstOrDefault(),
            RelatedMatches = relatedMatches,
        };
    }

    private List<RelatedMatch> GetRelatedMatchesFromFantasyMatches(List<FantasyRelatedMatch> relatedMatches)
    {
        var groupMatches = relatedMatches.GroupBy(match => match.MatchId);

        var newMatches = groupMatches.Select(group =>
        {
            var firstMatch = group.FirstOrDefault() ?? new FantasyRelatedMatch();

            var results = group.Select(match => new ResultDetail
            {
                HomeValue = match.HomeValue,
                AwayValue = match.AwayValue,
                ResourceId = match.ResourceId,
                ResultStatusId = match.ResultStatusId,
                SettlementStatusId = match.SettlementStatusId,
            }).ToList();

            return new RelatedMatch
            {
                MatchId = group.Key,
                MatchCode = firstMatch.MatchCode,
                KickOffTime = firstMatch.KickOffTime,
                Remark = firstMatch.Remark,
                EventStatus = firstMatch.EventStatus,
                BetStatus = firstMatch.BetStatus,
                HomeId = firstMatch.HomeId,
                AwayId = firstMatch.AwayId,
                IsDeleted = firstMatch.IsDeleted,
                Results = results
            };
        }).ToList();

        return newMatches;
    }
}