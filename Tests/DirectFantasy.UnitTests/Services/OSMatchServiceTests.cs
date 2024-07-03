using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Environment = DirectFantasy.Constants.Environment;

namespace DirectFantasy.UnitTests.Services
{
    public class OSMatchServiceTests
    {
        private IDynamicRepository repository;
        private ICache cache;
        private IOptions<AppSettingsKey> appSettingsKey;
        private OSMatchServiceImp matchService;

        public OSMatchServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            cache = Substitute.For<ICache>();
            appSettingsKey = Options.Create(new AppSettingsKey());
            matchService = new OSMatchServiceImp(repository, cache, appSettingsKey);
        }

        [Fact]
        public void Remove_ShouldRemoveMatchFromPoolOsMatches()
        {
            // Arrange
            var match = new Match { MatchId = 1, TypeId = 2 };
            matchService.PoolOsMatches.Add(match);

            // Act
            matchService.Remove(match);

            // Assert
            Assert.DoesNotContain(matchService.PoolOsMatches, m => m.MatchId == match.MatchId && m.TypeId == match.TypeId);
        }

        [Fact]
        public async Task PeekNewMatch_ShouldReturnNewMatchesAndAddToPoolOsMatches()
        {
            // Arrange
            var fantasyMatches = new List<Match>
                {
                    new() { MatchId = 1, TypeId = 2 },
                    new () { MatchId = 3, TypeId = 4 },
                    new () { MatchId = 5, TypeId = 6 }
                };
            matchService.PoolOsMatches.Add(new Match { MatchId = 1, TypeId = 2 });

            repository.FetchAsync<Match>(Arg.Any<OSMatchesSelCriteria>()).Returns(fantasyMatches);

            // Act
            var newMatches = await matchService.PeekNewMatch();

            // Assert
            Assert.Equal(2, newMatches.Count);
            Assert.Contains(matchService.PoolOsMatches, m => m.MatchId == 3 && m.TypeId == 4);
            Assert.Contains(matchService.PoolOsMatches, m => m.MatchId == 5 && m.TypeId == 6);
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
            repository.FetchAsync<Match>(Arg.Any<OSMatchesSelCriteria>()).Returns(matches);
            var settings = new AppSettingsKey()
            {
                EnvironmentName = Environment.Pro,
                SiteDbServer = new List<string> { "DBTXN" }
            };
            var appSettingsKey = Options.Create(settings);
            matchService = new OSMatchServiceImp(repository, cache, appSettingsKey);
            cache.TryGetAsync<string>(CacheKey.OsLeagueId).Returns((true, "1"));

            // Act
            await matchService.PeekNewMatch();

            // Assert
            Assert.Single(matchService.PoolOsMatches);
            Assert.Equal(2, matchService.PoolOsMatches.FirstOrDefault()?.MatchId);
        }

        [Fact]
        public async Task UpdateStatusResultOS_WithSettleCriteria_ShouldExecuteRepositoryWithCorrectCriteria()
        {
            // Arrange
            var settleCriteria = new SettleCriteria { MatchId = 1, SelectedResources = "resource1,resource2" };
            var status = 2;
            var remark = "Test remark";

            // Act
            await matchService.UpdateStatusResultOS(settleCriteria, status, remark);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Is<ResultOSUpdateCriteria>(criteria =>
                criteria.MatchId == settleCriteria.MatchId &&
                criteria.SelectedResources == settleCriteria.SelectedResources &&
                criteria.StatusId == status &&
                criteria.Remark == remark));
        }

        [Fact]
        public async Task UpdateStatusResultOS_WithMatch_ShouldExecuteRepositoryWithCorrectCriteria()
        {
            // Arrange
            var match = new Match { MatchId = 1 };
            var status = 2;
            var remark = "Test remark";

            // Act
            await matchService.UpdateStatusResultOS(match, status, remark);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Is<ResultOSUpdateCriteria>(criteria =>
                criteria.MatchId == match.MatchId &&
                criteria.StatusId == status &&
                criteria.Remark == remark));
        }

        [Fact]
        public async Task GetRelatedMatches_ShouldReturnFantasyMatchWithRelatedMatches()
        {
            // Arrange
            var matchId = 1;
            var firstResultSet = new List<FantasyMatchDetail> { new() { FantasyHomeTeam = "1" } };
            var secondResultSet = new List<FantasyRelatedMatch>
                {
                    new () { MatchId = 1, ResourceId = "M1" },
                    new () { MatchId = 1, ResourceId = "M2" },
                    new () { MatchId = 1, ResourceId = "M3" }
                };

            repository.FetchMultipleAsync<FantasyMatchDetail, FantasyRelatedMatch>(Arg.Any<RelatedMatchesSelCriteria>())
                .Returns((firstResultSet, secondResultSet));

            // Act
            var fantasyMatch = await matchService.GetRelatedMatches(matchId);

            // Assert
            Assert.NotNull(fantasyMatch);
            Assert.Equal("1", fantasyMatch.FantasyMatchDetail.FantasyHomeTeam);
            Assert.Equal(3, fantasyMatch.RelatedMatches[0].Results.Count);
            Assert.Equal("M1", fantasyMatch.RelatedMatches[0].Results[0].ResourceId);
            Assert.Equal("M2", fantasyMatch.RelatedMatches[0].Results[1].ResourceId);
            Assert.Equal("M3", fantasyMatch.RelatedMatches[0].Results[2].ResourceId);
        }
    }
}
