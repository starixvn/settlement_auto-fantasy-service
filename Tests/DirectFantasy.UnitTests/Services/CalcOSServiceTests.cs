using DirectFantasy.Constants;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services
{
    public class CalcOSServiceTests
    {
        private readonly ScaleOutSettleService scaleOutSettleService;
        private readonly IDynamicRepository repository;
        private readonly LogService logService;
        private readonly OSMatchService matchOSService;
        private readonly MatchService matchService;

        private readonly CalcOSServiceImp calcOSService;

        private readonly AppSettingsKey settings = new()
        {
            EnvironmentName = "Local",
            SiteDbServer = ["DBTXN", "DBSID01", "DBSID02"]
        };

        public CalcOSServiceTests()
        {
            scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
            repository = Substitute.For<IDynamicRepository>();
            logService = Substitute.For<LogService>();
            matchOSService = Substitute.For<OSMatchService>();
            matchService = Substitute.For<MatchService>();

            var appSettingsKey = Options.Create(settings);

            calcOSService = new CalcOSServiceImp(scaleOutSettleService, repository, appSettingsKey, logService, matchOSService, matchService);
        }

        [Fact]
        public async Task Settle_ShouldUpdateUnderSettlementFlag()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            await scaleOutSettleService.Received(1).UpdateUnderSettlementFlag(settleCriteria.MatchId, 0, settleCriteria.Action);
            await matchOSService.Received(1).UpdateStatusResultOS(settleCriteria, MatchStatus.Waiting);
            await matchService.Received(1).UpdateMatchInfo(settleCriteria);
            await logService.Received(1).WriteLogSettle(match, settleCriteria);
            repository.Received(settings.SiteDbServer.Count).Execute(Arg.Any<SettleCriteria>());
        }
    }
}