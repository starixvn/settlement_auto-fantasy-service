using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Criteria;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace DirectFantasy.Services.Services;

public interface MatchService
{
    Task<List<Match>> PeekNewMatch();

    void Remove(Match match);

    Task UpdateMatchRemark(Match match, string remark, int statusId);

    void RemoveAndUpdateStatus(Match match, int statusId);
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

    public void RemoveAndUpdateStatus(Match match, int statusId)
    {
        lock (matchesLock)
        {
            Remove(match);
            UpdateMatchStatus(match, statusId);
        }
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