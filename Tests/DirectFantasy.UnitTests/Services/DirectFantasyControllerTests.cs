using AutoFixture;
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
    public async Task ProcessAsync_HasMatches_InvokeProcessMethod()
    {
        // Arrange
        var fixture = new Fixture();
        var matches = fixture.CreateMany<Match>(1).ToList();
        matches.ForEach(m => m.IsChecking = false);

        matchService.PeekNewMatch().Returns(matches);

        // Act
        await controller.ProcessAsync();

        // Assert
        await matchSettlementHandler.Received().Progress(Arg.Any<Match>());
    }
}