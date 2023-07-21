using AutoPayout.Constants;
using AutoPayout.Models;
using DirectPayout.Fantasy.Models;
using DirectPayout.Fantasy.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using LogService = DirectPayout.Fantasy.Services.LogService;

namespace DirectPayout.Fantasy.UnitTests.Services;

public class MatchSettlementHandlerTests
{
    private readonly MatchSettlementHandler matchSettlementHandler;
    private readonly SettleService settleService;
    private readonly LogService logService;

    public MatchSettlementHandlerTests()
    {
        var matchService = Substitute.For<FantasyMatchService>();
        settleService = Substitute.For<SettleService>();
        logService = Substitute.For<LogService>();

        matchSettlementHandler = new MatchSettlementHandlerImp(matchService, settleService, logService);
    }

    [Fact]
    public async void Progress_VerifyMatchSuccess_InvokeSettle()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            Action = "CAL"
        };
        settleService.VerifyMatch(Arg.Any<Match>()).Returns(new Result());

        // Act
        await matchSettlementHandler.Progress(match);

        // Assert
        logService.Received(1).WriteDirectly(match, match.Action);
        await settleService.Received(1).Settle(match);
    }

    [Fact]
    public async void Progress_VerifyMatchError_InvokeLog()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            Action = "CAL"
        };
        var verifyResult = new Result
        {
            ErrCode = 1,
            ErrMsg = "Error"
        };
        settleService.VerifyMatch(Arg.Any<Match>()).Returns(verifyResult);

        // Act
        await matchSettlementHandler.Progress(match);

        // Assert
        await logService.Received(1).Write(match, verifyResult.ErrMsg);
        await settleService.Received(1).InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed);
    }

    [Fact]
    public async void Progress_SettleThrowException_InvokeLog()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            Action = "CAL"
        };
        var verifyResult = new Result();
        var ex = new FileNotFoundException();
        settleService.VerifyMatch(Arg.Any<Match>()).Returns(verifyResult);
        settleService.Settle(match).Throws(ex);

        // Act
        await matchSettlementHandler.Progress(match);

        // Assert
        await logService.Received(1).Write(match, ex.Message);
        await settleService.Received(1).InsertAutoPayoutResult(match, 0, MatchStatus.Failed);
    }
}