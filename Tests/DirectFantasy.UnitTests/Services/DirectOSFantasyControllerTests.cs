using DirectFantasy.Models;
using DirectFantasy.Services;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services
{
    public class DirectOSFantasyControllerTests
    {
        private readonly OSMatchSettlementHandler matchSettlementHandler;
        private readonly OSMatchService matchService;

        private readonly DirectOSFantasyController directOSFantasyController;

        public DirectOSFantasyControllerTests()
        {
            matchSettlementHandler = Substitute.For<OSMatchSettlementHandler>();
            matchService = Substitute.For<OSMatchService>();

            directOSFantasyController = new DirectOSFantasyControllerImp(matchService, matchSettlementHandler);
        }

        [Fact]
        public async Task ProcessAsync_ShouldCallMatchServicePeekNewMatch()
        {
            // Arrange
            var newMatches = new List<Match> { new(), new(), new() };
            matchService.PeekNewMatch().Returns(Task.FromResult(newMatches));

            // Act
            await directOSFantasyController.ProcessAsync();

            // Assert
            await matchService.Received(1).PeekNewMatch();
        }

        [Fact]
        public async Task ProcessAsync_ShouldCallMatchSettlementHandlerProgressForEachMatch()
        {
            // Arrange
            var newMatches = new List<Match> { new(), new() };
            matchService.PeekNewMatch().Returns(Task.FromResult(newMatches));

            // Act
            await directOSFantasyController.ProcessAsync();

            // Assert
            await matchSettlementHandler.Received(newMatches.Count).Progress(Arg.Any<Match>());
        }
    }
}
