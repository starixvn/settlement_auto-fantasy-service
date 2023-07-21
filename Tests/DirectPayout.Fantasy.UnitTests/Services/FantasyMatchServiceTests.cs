using AutoPayout.Constants;
using AutoPayout.Models;
using DirectPayout.Fantasy.Criteria;
using DirectPayout.Fantasy.Models;
using DirectPayout.Fantasy.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectPayout.Fantasy.UnitTests.Services;

public class FantasyMatchServiceTests
{
    private readonly IDynamicRepository repository;

    private readonly ICache cache;

    private FantasyMatchServiceImp fantasyMatchService;

    private void SetLeague(int leagueId)
    {
        const int cacheExpiredInDays = 1095;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
        cache.Set(CacheKey.LeagueId, leagueId, cachePolicy);
    }

    public FantasyMatchServiceTests()
    {
        cache = new RedisCache(new RedisCacheOptions
        {
            ConnectionString = "10.18.200.131:16379,password=foo1@bared,defaultDatabase=11",
            ApplicationName = "Settlement-LOCAL"
        });

        repository = Substitute.For<IDynamicRepository>();
    }

    [Fact]
    public async Task PeekNewMatch_NotExistLeagueTest_ReturnsAllMatches()
    {
        // Arrange
        var matches = new List<Match>
        {
            new()
            {
                MatchId = 1,
            }
        };

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 1,
            EnvironmentName = "LOCAL",
        };
        var appSettingsKey = Options.Create(settings);
        fantasyMatchService = new FantasyMatchServiceImp(repository, cache, appSettingsKey);

        SetLeague(1);
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act
        var result = await fantasyMatchService.PeekNewMatch();

        // Assert
        Assert.Equal(matches.Count, result.Count);
    }

    [Fact]
    public async Task PeekNewMatch_ExistLeagueTest_ReturnsOneMatches()
    {
        // Arrange
        var matches = new List<Match>
        {
            new()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 1,
            EnvironmentName = "UAT",
        };
        var appSettingsKey = Options.Create(settings);
        fantasyMatchService = new FantasyMatchServiceImp(repository, cache, appSettingsKey);

        SetLeague(1);
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act
        var result = await fantasyMatchService.PeekNewMatch();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task Remove_ExistedMatchId_ExistingMatchIsRemove()
    {
        // Arrange
        var matches = new List<Match>
        {
            new()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 1,
            EnvironmentName = "LOCAL",
        };
        var appSettingsKey = Options.Create(settings);
        fantasyMatchService = new FantasyMatchServiceImp(repository, cache, appSettingsKey);

        SetLeague(1);
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act
        await fantasyMatchService.PeekNewMatch();
        fantasyMatchService.Remove(matches[0]);

        // Assert
        Assert.Single(fantasyMatchService.PoolMatches);
    }

    [Fact]
    public async Task Remove_NotExistedMatchId_NoChange()
    {
        // Arrange
        var matches = new List<Match>
        {
            new()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 1,
            EnvironmentName = "LOCAL",
        };
        var appSettingsKey = Options.Create(settings);
        fantasyMatchService = new FantasyMatchServiceImp(repository, cache, appSettingsKey);

        SetLeague(1);
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act1
        await fantasyMatchService.PeekNewMatch();
        fantasyMatchService.Remove(new Match());

        // Assert
        Assert.Equal(2, fantasyMatchService.PoolMatches.Count);
    }
}