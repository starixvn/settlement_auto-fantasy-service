using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;
using Fanex.Caching;
using Environment = DirectFantasy.Constants.Environment;

namespace DirectFantasy.Services;

public interface MatchService
{
    Task<List<Match>> PeekNewMatch();

    void Remove(Match match);

    Task UpdateMatchRemark(Match match, string remark, int statusId);

    Task UpdateAutoRefundResult(Match match, int statusId);
}

public class MatchServiceImp(
    IDynamicRepository repository,
    ICache cache,
    IOptions<AppSettingsKey> appSettingsKey)
    : MatchService
{
    private readonly object matchesLock = new();

    public List<Match> PoolMatches { get; } = new();

    public async Task UpdateAutoRefundResult(Match match, int statusId)
    {
        var criteria = new AutoRefundResultUpdCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            StatusId = statusId
        };

        await repository.ExecuteAsync(criteria);
    }

    public void Remove(Match match)
    {
        lock (matchesLock)
        {
            PoolMatches.RemoveAll(m => m.MatchId == match.MatchId && m.TypeId == match.TypeId);
        }
    }

    public async Task<List<Match>> PeekNewMatch()
    {
        var fantasyMatches = await GetMatchesByLeagues();

        lock (matchesLock)
        {
            var newMatches = fantasyMatches.Except(PoolMatches).ToList();
            PoolMatches.AddRange(newMatches);

            return newMatches;
        }
    }

    public async Task UpdateMatchRemark(Match match, string remark, int statusId)
    {
        var criteria = new AutoRefundResultUpdCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            Remark = remark,
            StatusId = statusId
        };
        await repository.ExecuteAsync(criteria);
    }

    private async Task<List<Match>> GetMatchesByLeagues()
    {
        var fantasyMatches = await GetFantasyMatches();

        var (_, response) = await cache.TryGetAsync<string>(CacheKey.LeagueId);

        var leagueIds = (response ?? "").Split(",");

        return appSettingsKey.Value.EnvironmentName switch
        {
            Environment.Pro => leagueIds.Length > 0
                ? fantasyMatches.Where(m => !leagueIds.Contains(m.LeagueId.ToString())).ToList()
                : fantasyMatches.ToList(),
            Environment.Uat => fantasyMatches.Where(m => leagueIds.Contains(m.LeagueId.ToString())).ToList(),
            _ => fantasyMatches.ToList()
        };
    }

    private async Task<IEnumerable<Match>> GetFantasyMatches()
    {
        var criteria = new GetFantasyMatchesCriteria();
        var fantasyMatches = await repository.FetchAsync<Match>(criteria);
        return fantasyMatches;
    }
}