using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Exceptions;
using DirectFantasy.Models;
using DirectFantasy.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DirectFantasy.UnitTests.Services
{
    public class OSMatchSettlementHandlerTests
    {
        private readonly OSMatchService matchService;
        private readonly ValidationMatchService validationMatchService;
        private readonly CriteriaService criteriaService;
        private readonly CalcOSService calcOSService;
        private readonly LogService logService;
        private readonly RefundOSService refundOSService;
        private readonly ResultDetailService resultDetailService;

        private readonly OSMatchSettlementHandlerImp settlementHandler;

        public OSMatchSettlementHandlerTests()
        {
            matchService = Substitute.For<OSMatchService>();
            validationMatchService = Substitute.For<ValidationMatchService>();
            criteriaService = Substitute.For<CriteriaService>();
            calcOSService = Substitute.For<CalcOSService>();
            logService = Substitute.For<LogService>();
            refundOSService = Substitute.For<RefundOSService>();
            resultDetailService = Substitute.For<ResultDetailService>();

            settlementHandler = new OSMatchSettlementHandlerImp(
                matchService,
                criteriaService,
                validationMatchService,
                calcOSService,
                logService,
                refundOSService,
                resultDetailService);
        }

        [Fact]
        public async Task HandleCalc_WhenValidationIsSuccessful_ShouldCallSettle()
        {
            // Arrange
            var match = new Match();
            var relatedMatch = new FantasyMatch();
            var settledCriteria = new SettleCriteria();
            var validateResult = new Result();

            matchService.GetRelatedMatches(match.MatchId).Returns(relatedMatch);
            validationMatchService.ValidateRelatedMatch(relatedMatch);
            validationMatchService.ValidateEventTicketOS(settledCriteria).Returns(validateResult);
            criteriaService.CreateOSSettleCriteria(match).Returns(settledCriteria);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            resultDetailService.Received(1).GenerateResults(relatedMatch);
            await calcOSService.Received(1).Settle(match, settledCriteria);
            await matchService.DidNotReceive().UpdateStatusResultOS(settledCriteria, MatchStatus.Failed, Arg.Any<string>());
            await logService.DidNotReceive().WriteLog(match, Arg.Any<string>());
            matchService.Received(1).Remove(match);
        }

        [Fact]
        public async Task HandleCalc_WhenValidationFails_ShouldUpdateStatusAndLogError()
        {
            // Arrange
            var match = new Match();
            var relatedMatch = new FantasyMatch();
            var settledCriteria = new SettleCriteria();
            var validateResult = new Result { ErrMsg = "Validation failed" };

            matchService.GetRelatedMatches(match.MatchId).Returns(relatedMatch);
            validationMatchService.ValidateRelatedMatch(relatedMatch);
            validationMatchService.ValidateEventTicketOS(Arg.Any<SettleCriteria>()).Returns(validateResult);
            criteriaService.CreateOSSettleCriteria(match).Returns(settledCriteria);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            matchService.Received(1).Remove(match);
            resultDetailService.Received(1).GenerateResults(relatedMatch);
            await calcOSService.DidNotReceive().Settle(match, settledCriteria);
            await matchService.Received(1).UpdateStatusResultOS(settledCriteria, MatchStatus.Failed, Arg.Any<string>());
            await logService.Received(1).WriteLog(match, Arg.Any<string>());
        }

        [Fact]
        public async Task ProgressCalc_WhenHandleThrowsException_ShouldCallWriteLogAndUpdateStatusResultOS()
        {
            // Arrange
            var match = new Match();
            var appException = new AppException("App exception");

            validationMatchService.When(x => x.ValidateRelatedMatch(Arg.Any<FantasyMatch>())).Throw(appException);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            await logService.Received(1).WriteLog(match, appException.Description);
            await matchService.Received(1).UpdateStatusResultOS(match, MatchStatus.Failed, appException.Description);
            matchService.Received(1).Remove(match);
        }

        [Fact]
        public async Task HandleRefund_WhenValidationIsSuccessful_ShouldCallHandleRefundPush()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            var settledCriteria = new SettleCriteria();
            var validateResult = new Result();

            validationMatchService.ValidateEventTicketOS(settledCriteria).Returns(validateResult);
            resultDetailService.CreateRefundResults().Returns(new List<ResultDetail>());
            criteriaService.CreateOSSettleCriteria(match).Returns(settledCriteria);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            await refundOSService.Received(1).InsResultRefundOS(match, Arg.Any<string>());
            await validationMatchService.Received(1).ValidateTicketMatch(match);
            await refundOSService.Received(1).HandleRefundPush(match, settledCriteria);
            await logService.DidNotReceive().WriteLog(match, Arg.Any<string>());
            await matchService.DidNotReceive().UpdateStatusResultOS(settledCriteria, MatchStatus.Failed);
        }

        [Fact]
        public async Task HandleRefund_WhenValidationFails_ShouldUpdateStatusAndLogError()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            var settledCriteria = new SettleCriteria();
            var validateResult = new Result { ErrMsg = "Validation failed" };

            validationMatchService.ValidateEventTicketOS(settledCriteria).Returns(validateResult);
            resultDetailService.CreateRefundResults().Returns(new List<ResultDetail>());
            criteriaService.CreateOSSettleCriteria(match).Returns(settledCriteria);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            await refundOSService.Received(1).InsResultRefundOS(match, Arg.Any<string>());
            await validationMatchService.Received(1).ValidateTicketMatch(match);
            await refundOSService.DidNotReceive().HandleRefundPush(match, settledCriteria);
            await logService.Received(1).WriteLog(match, Arg.Any<string>());
            await matchService.Received(1).UpdateStatusResultOS(settledCriteria, MatchStatus.Failed);
        }

        [Fact]
        public async Task ProgressRefund_WhenHandleThrowsException_ShouldCallWriteLogAndUpdateStatusResultOS()
        {
            // Arrange
            var match = new Match { Action = SettleAction.Refund };
            var appException = new MarkedTicketException("App exception");
            var settledCriteria = new SettleCriteria();

            validationMatchService.ValidateEventTicketOS(Arg.Any<SettleCriteria>()).Throws(appException);
            resultDetailService.CreateRefundResults().Returns(new List<ResultDetail>());
            criteriaService.CreateOSSettleCriteria(match).Returns(settledCriteria);

            // Act
            await settlementHandler.Progress(match);

            // Assert
            await logService.Received(1).WriteLog(match, appException.Description);
            await matchService.Received(1).UpdateStatusResultOS(settledCriteria, MatchStatus.Failed);
            matchService.Received(1).Remove(match);
        }
    }
}
