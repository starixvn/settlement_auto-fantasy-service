using DirectPayout.Fantasy.Models;
using DirectPayout.Fantasy.Services;
using NSubstitute;

namespace DirectPayout.Fantasy.UnitTests.Services;

public class DirectPayoutControllerTests
{
    private readonly MatchSettlementHandler matchService;
    private readonly FantasyMatchService fantasyMatchService;

    private readonly DirectPayoutControllerImp directPayoutController;

    public DirectPayoutControllerTests()
    {
        matchService = Substitute.For<MatchSettlementHandler>();
        fantasyMatchService = Substitute.For<FantasyMatchService>();

        directPayoutController = new DirectPayoutControllerImp(matchService, fantasyMatchService);
    }

    [Fact]
    public async void ProcessAsync_HasMatch_InvokeProgressMethod()
    {
        // Arrange
        fantasyMatchService.PeekNewMatch().Returns(new List<Match>()
        {
            new ()
            {
                MatchId = 1
            }
        });

        // Act

        await directPayoutController.ProcessAsync();

        // Arrange
        await matchService.Received(1).Progress(Arg.Any<Match>());
    }
}