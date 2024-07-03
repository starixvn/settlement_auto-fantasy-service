using DirectFantasy.Models;
using DirectFantasy.Services;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services;

public class DirectFantasyControllerTests
{
    private readonly MatchSettlementHandler matchSettlementHandler;

    private readonly MatchService matchService;

    private readonly DirectFantasyController controller;

    public DirectFantasyControllerTests()
    {
        matchSettlementHandler = Substitute.For<MatchSettlementHandler>();
        matchService = Substitute.For<MatchService>();
        controller = new DirectFantasyControllerImp(matchSettlementHandler, matchService);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallMatchSettlementHandlerProgressForEachMatch()
    {
        // Arrange
        var newMatches = new List<Match> { new(), new(), new() };
        matchService.PeekNewMatch().Returns(Task.FromResult(newMatches));

        // Act
        await controller.ProcessAsync();

        // Assert
        await matchSettlementHandler.Received(newMatches.Count).Progress(Arg.Any<Match>());
    }
}