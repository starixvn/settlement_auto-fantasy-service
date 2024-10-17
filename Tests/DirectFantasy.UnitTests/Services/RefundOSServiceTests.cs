using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services
{
    public class RefundOSServiceTests
    {
        private readonly ScaleOutSettleService scaleOutSettleService;
        private readonly IDynamicRepository repository;
        private readonly LogService logService;
        private readonly OSMatchService matchService;


        private readonly RefundOSService refundOSService;

        private readonly AppSettingsKey settings = new()
        {
            EnvironmentName = "Local",
            SiteDbServer = new List<string> { "DBTXN", "DBSITE01", "DBSITE02" }
        };

        public RefundOSServiceTests()
        {
            scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
            repository = Substitute.For<IDynamicRepository>();
            logService = Substitute.For<LogService>();
            matchService = Substitute.For<OSMatchService>();

            var appSettingsKey = Options.Create(settings);

            refundOSService = new RefundOSServiceImp(repository, scaleOutSettleService, logService, appSettingsKey, matchService);
        }

        [Fact]
        public async Task InsResultRefundOS_ShouldExecuteRepository()
        {
            // Arrange
            var match = new Match();
            var resourceIds = "12345";

            // Act
            await refundOSService.InsResultRefundOS(match, resourceIds);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Is<ResultRefundOSInsCriteria>(criteria =>
                criteria.MatchId == match.MatchId &&
                criteria.ResourceIds == resourceIds));
        }

        [Fact]
        public async Task HandleRefundPush_ShouldUpdateUnderSettlementFlag_UpdateMatchInfo_WriteLogSettle_ExecuteAllSiteDb()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await refundOSService.HandleRefundPush(match, settleCriteria);

            // Assert
            await scaleOutSettleService.Received(1).UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);
            await logService.Received(1).WriteLogSettle(match, settleCriteria);
            repository.Received(settings.SiteDbServer.Count).Execute(Arg.Any<SettleCriteria>());
        }
    }
}
