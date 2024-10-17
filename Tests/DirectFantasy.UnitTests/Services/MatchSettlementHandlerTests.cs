using DirectFantasy.Constants;
using DirectFantasy.Models;
using DirectFantasy.Services;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services
{
    public class MatchSettlementHandlerTests
    {
        private readonly MatchService fantasyMatchService;
        private readonly CalcService calcService;
        private readonly LogService logService;
        private readonly RefundService refundService;
        private readonly ValidationMatchService validationMatchService;
        private readonly MatchService matchService;

        private readonly MatchSettlementHandler matchSettlementHandler;

        public MatchSettlementHandlerTests()
        {
            fantasyMatchService = Substitute.For<MatchService>();
            calcService = Substitute.For<CalcService>();
            logService = Substitute.For<LogService>();
            refundService = Substitute.For<RefundService>();
            validationMatchService = Substitute.For<ValidationMatchService>();
            matchService = Substitute.For<MatchService>();

            matchSettlementHandler = new MatchSettlementHandlerImp(fantasyMatchService, calcService, logService, refundService, matchService, validationMatchService); // Fixed spelling of 'Calc'
        }

        [Fact]
        public async Task Progress_WhenMatchIsRefund_ShouldCallProgressRefund()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            validationMatchService.ValidateEventTicket(match).Returns(new Result());

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await refundService.Received(1).InsertRefundResult(match);
            await refundService.Received(1).HandleRefundPush(match);
            fantasyMatchService.Received(1).Remove(match);
        }

        [Fact]
        public async Task Progress_WhenMatchIsNotRefund_ShouldCallProgressCalc()
        {
            // Arrange
            var match = new Match { Action = string.Empty };
            validationMatchService.ValidateEventTicket(match).Returns(new Result());

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await calcService.Received(1).Settle(match);
            fantasyMatchService.Received(1).Remove(match);
        }

        [Fact]
        public async Task HandleCalc_WhenValidationIsSuccessful_ShouldCallSettleAndInsertAutoPayoutResult()
        {
            // Arrange
            var match = new Match();
            var validateResult = new Result();
            validationMatchService.ValidateEventTicket(match).Returns(validateResult);

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await calcService.Received(1).Settle(match);
            await calcService.DidNotReceive().InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed, Arg.Any<string>()); // Fixed spelling of 'Calc'
        }

        [Fact]
        public async Task HandleCalc_WhenValidationFails_ShouldCallWriteLogAndInsertAutoPayoutResult()
        {
            // Arrange
            var match = new Match();
            var validateResult = new Result { ErrMsg = "Validation failed" };
            validationMatchService.ValidateEventTicket(match).Returns(validateResult);

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await logService.Received(1).WriteLog(match, $"Failed Direct Payout: {validateResult.ErrMsg}");
            await calcService.Received(1).InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Failed, validateResult.ErrMsg);
        }

        [Fact]
        public async Task HandleRefund_WhenValidationIsSuccessful_ShouldCallHandleRefundPush()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            var result = new Result();
            validationMatchService.ValidateEventTicket(match).Returns(result);
            validationMatchService.When(x => x.ValidateTicketMatch(Arg.Any<Match>())).Do(_ => { });

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await refundService.Received(1).HandleRefundPush(match);
        }

        [Fact]
        public async Task HandleRefund_WhenValidationFails_ShouldCallWriteLogAndUpdateMatchRemark()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            var result = new Result { ErrMsg = "Validation failed" };
            validationMatchService.ValidateEventTicket(match).Returns(result);

            // Act
            await matchSettlementHandler.Progress(match);

            // Assert
            await logService.Received(1).WriteLog(match, $"Auto Refund failed: {result.ErrMsg}");
            await fantasyMatchService.Received(1).UpdateMatchRemark(match, result.ErrMsg, MatchStatus.Failed);
        }
    }
}
