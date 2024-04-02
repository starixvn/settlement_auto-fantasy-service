using System.Security.AccessControl;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using Fanex.Caching;
using Environment = DirectFantasy.Constants.Environment;

namespace DirectFantasy.Services;

public interface MatchService
{
    Task<List<Match>> PeekNewMatch();

    void Remove(Match match);

    Task UpdateMatchRemark(Match match, string remark, int statusId);

    Task UpdateMatchStatus(Match match, int statusId);
}

public class MatchServiceImp : MatchService
{
    private readonly object matchesLock = new();

    public List<Match> PoolMatches { get; } = new();

    private readonly IDynamicRepository repository;

    private readonly ICache cache;

    private readonly AppSettingsKey appSettingsKey;

    public MatchServiceImp(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.cache = cache;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public async Task UpdateMatchStatus(Match match, int statusId)
    {
        var criteria = new UpdateStatusCriteria
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
        var criteria = new UpdateStatusCriteria
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

        return appSettingsKey.EnvironmentName switch
        {
            Environment.Pro => leagueIds.Length > 0
                ? fantasyMatches.Where(m => !leagueIds.Contains(m.LeagueId.ToString())).ToList()
                : fantasyMatches,
            Environment.Uat => fantasyMatches.Where(m => leagueIds.Contains(m.LeagueId.ToString())).ToList(),
            _ => fantasyMatches
        };
    }

    private async Task<List<Match>> GetFantasyMatches()
    {
        var criteria = new GetFantasyMatchesCriteria();
        var fantasyMatches = await repository.FetchAsync<Match>(criteria);
        return fantasyMatches.ToList();
    }
}