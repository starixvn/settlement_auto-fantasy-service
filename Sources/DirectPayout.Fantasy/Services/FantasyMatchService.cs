using AutoPayout.Constants;
using AutoPayout.Models;
using DirectPayout.Fantasy.Criteria;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using Match = DirectPayout.Fantasy.Models.Match;

namespace DirectPayout.Fantasy.Services;

public interface FantasyMatchService
{
    Task<List<Match>> PeekNewMatch();

    void Remove(Match match);
}

public class FantasyMatchServiceImp : FantasyMatchService
{
    private readonly object matchesLock = new();

    public List<Match> PoolMatches { get; } = new();

    private readonly IDynamicRepository repository;

    private readonly ICache cache;

    private readonly AppSettingsKey appSettingsKey;

    public FantasyMatchServiceImp(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.cache = cache;
        this.appSettingsKey = appSettingsKey.Value;
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

    private async Task<List<Match>> GetMatchesByLeagues()
    {
        var fantasyMatches = await GetFantasyMatches();

        var (_, leagueId) = await cache.TryGetAsync<int>(CacheKey.LeagueId);

        return appSettingsKey.EnvironmentName switch
        {
            "PRO" => leagueId > 0 ? fantasyMatches.Where(x => x.LeagueId != leagueId).ToList() : fantasyMatches,
            "UAT" => fantasyMatches.Where(x => x.LeagueId == leagueId).ToList(),
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