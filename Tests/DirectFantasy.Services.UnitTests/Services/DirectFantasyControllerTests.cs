using AutoFixture;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Services;
using NSubstitute;

namespace DirectFantasy.Services.UnitTests.Services;

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
    public void ProcessAsync_HasMatches_InvokeProcessMethod()
    {
        // Arrange
        var fixture = new Fixture();
        var matches = fixture.CreateMany<Match>(2).ToList();

        matchService.PeekNewMatch().Returns(matches);

        // Act
        controller.ProcessAsync();
        Thread.Sleep(1000);

        // Assert
        matchSettlementHandler.Received().Progress(Arg.Any<Match>());
    }
}