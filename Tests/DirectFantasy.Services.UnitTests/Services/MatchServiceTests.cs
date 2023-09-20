using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Criteria;
using DirectFantasy.Services.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Environment = DirectFantasy.Core.Constants.Environment;

namespace DirectFantasy.Services.UnitTests.Services;

public class MatchServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly ICache cache;
    private MatchServiceImp matchService;

    public MatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        cache = new RedisCache(new RedisCacheOptions
        {
            ConnectionString = "10.18.200.131:16379,password=foo1@bared,defaultDatabase=13",
            ApplicationName = "Settlement-LOCAL"
        });

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local",
            SiteDbServer = new List<string> { "DBTXN", "DBSID01", "DBSID02" }
        };
        var appSettingsKey = Options.Create(settings);
        matchService = new MatchServiceImp(repository, cache, appSettingsKey);
    }

    [Fact]
    public async Task UpdateMatchRemark_ValidInput_CallCorrectExecuteAsync()
    {
        // Arrange
        const string remark = "";
        const int statusId = 4;
        var criteriaExpect = new UpdateStatusCriteria
        {
            MatchId = 1,
            TypeId = 2,
            Remark = remark,
            StatusId = statusId
        };
        var match = new Match
        {
            MatchId = 1,
            TypeId = 2,
        };

        // Act
        await matchService.UpdateMatchRemark(match, remark, statusId);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusCriteria>(criteria =>
            criteria.MatchId == criteriaExpect.MatchId &&
            criteria.TypeId == criteriaExpect.TypeId &&
            criteria.Remark == remark &&
            criteria.StatusId == statusId));
    }

    [Fact]
    public async Task UpdateMatchStatus_ValidInput_CallCorrectExecuteAsync()
    {
        const int statusId = 4;
        var criteriaExpect = new UpdateStatusCriteria
        {
            MatchId = 1,
            TypeId = 2,
            StatusId = statusId
        };
        var match = new Match
        {
            MatchId = 1,
            TypeId = 2,
        };

        // Act
        await matchService.UpdateMatchStatus(match, statusId);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusCriteria>(criteria =>
            criteria.MatchId == criteriaExpect.MatchId &&
            criteria.TypeId == criteriaExpect.TypeId &&
            criteria.StatusId == statusId));
    }

    [Fact]
    public async Task Remove_ExistedMatchId_ExistingMatchIsRemove()
    {
        // Arrange
        var matches = new List<Match>
        {
            new ()
            {
                MatchId = 1,
                TypeId = 2
            },
            new ()
            {
                MatchId = 2,
                TypeId = 2
            }
        };
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act
        await matchService.PeekNewMatch();
        matchService.Remove(matches[0]);

        // Assert
        Assert.Single(matchService.PoolMatches);
    }

    [Fact]
    public async Task Remove_NotExistedMatchId_NoChange()
    {
        // Arrange
        var matches = new List<Match>
        {
            new ()
            {
                MatchId = 1,
                TypeId = 2
            },
            new ()
            {
                MatchId = 2,
                TypeId = 2
            }
        };
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);

        // Act
        await matchService.PeekNewMatch();
        matchService.Remove(new Match());

        // Assert
        Assert.Equal(2, matchService.PoolMatches.Count);
    }

    [Fact]
    public async Task PeekNewMatch_EnvironmentNameUAT_ReturnCorrectMatches()
    {
        // Arrange
        var matches = new List<Match>
        {
            new ()
            {
                MatchId = 1,
                TypeId = 2,
                LeagueId = 1
            },
            new ()
            {
                MatchId = 2,
                TypeId = 2,
                LeagueId = 2
            }
        };
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);
        var settings = new AppSettingsKey()
        {
            EnvironmentName = Environment.Uat,
            SiteDbServer = new List<string> { "DBTXN", "DBSID01", "DBSID02" }
        };
        var appSettingsKey = Options.Create(settings);
        matchService = new MatchServiceImp(repository, cache, appSettingsKey);

        const int cacheExpiredInDays = 1;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheExpiredInDays));
        await cache.SetAsync(CacheKey.LeagueId, 1, cachePolicy);

        // Act
        await matchService.PeekNewMatch();

        // Assert
        Assert.Single(matchService.PoolMatches);
        Assert.Equal(1, matchService.PoolMatches.FirstOrDefault()?.MatchId);
    }

    [Fact]
    public async Task PeekNewMatch_EnvironmentNamePRO_ReturnCorrectMatches()
    {
        // Arrange
        var matches = new List<Match>
        {
            new ()
            {
                MatchId = 1,
                TypeId = 2,
                LeagueId = 1
            },
            new ()
            {
                MatchId = 2,
                TypeId = 2,
                LeagueId = 2
            }
        };
        repository.FetchAsync<Match>(Arg.Any<GetFantasyMatchesCriteria>()).Returns(matches);
        var settings = new AppSettingsKey()
        {
            EnvironmentName = Environment.Pro,
            SiteDbServer = new List<string> { "DBTXN", "DBSID01", "DBSID02" }
        };
        var appSettingsKey = Options.Create(settings);
        matchService = new MatchServiceImp(repository, cache, appSettingsKey);
        const int cacheExpiredInDays = 1;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheExpiredInDays));
        await cache.SetAsync(CacheKey.LeagueId, 1, cachePolicy);

        // Act
        await matchService.PeekNewMatch();

        // Assert
        Assert.Single(matchService.PoolMatches);
        Assert.Equal(2, matchService.PoolMatches.FirstOrDefault()?.MatchId);
    }
}