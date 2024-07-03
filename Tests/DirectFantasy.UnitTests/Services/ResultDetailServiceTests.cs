using DirectFantasy.Constants;
using DirectFantasy.Models;
using DirectFantasy.Services;

namespace DirectFantasy.UnitTests.Services
{
    public class ResultDetailServiceTests
    {
        private readonly ResultDetailService resultDetailService = new ResultDetailServiceImp();

        [Fact]
        public void CreateRefundResults_ShouldReturnNewResults()
        {
            // Arrange

            // Act
            var results = resultDetailService.CreateRefundResults();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(ResourceId.Basketball.Length, results.Count);
        }

        [Fact]
        public void GenerateResults_ShouldReturnResults()
        {
            // Arrange
            var relatedMatches = new FantasyMatch
            {
                FantasyMatchDetail = new FantasyMatchDetail
                {
                    FantasyHomeTeam = "1,2,3",
                    FantasyAwayTeam = "4,5,6"
                },
                RelatedMatches = new List<RelatedMatch>
                    {
                        new ()
                        {
                            HomeId = 1,
                            AwayId = 4,
                            Results = new List<ResultDetail>
                            {
                                new () { ResourceId = "1", HomeValue = "10", AwayValue = "20" },
                                new () { ResourceId = "2", HomeValue = "30", AwayValue = "40" }
                            }
                        },
                        new ()
                        {
                            HomeId = 2,
                            AwayId = 5,
                            Results = new List<ResultDetail>
                            {
                                new () { ResourceId = "1", HomeValue = "50", AwayValue = "60" },
                                new () { ResourceId = "2", HomeValue = "70", AwayValue = "80" }
                            }
                        },
                        new ()
                        {
                            HomeId = 4,
                            AwayId = 7,
                            Results = new List<ResultDetail>
                            {
                                new () { ResourceId = "1", HomeValue = "90", AwayValue = "100" },
                                new () { ResourceId = "2", HomeValue = "110", AwayValue = "120" }
                            }
                        }
                    }
            };

            // Act
            var results = resultDetailService.GenerateResults(relatedMatches);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("60", results[0].HomeValue);
            Assert.Equal("170", results[0].AwayValue);
            Assert.Equal("100", results[1].HomeValue);
            Assert.Equal("230", results[1].AwayValue);
        }
    }
}
