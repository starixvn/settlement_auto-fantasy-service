using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace DirectFantasy.UnitTests.Services
{
    public class CalcOSServiceTests
    {
        private readonly IDynamicRepository repository;
        private readonly ScaleOutSettleService scaleOutSettleService;
        private readonly LogService logService;
        private readonly OSMatchService matchService;
        private readonly List<string> siteDbs = ["DBTXN", "DBSID01", "DBSID02"];

        private readonly CalcOSServiceImp calcOSService;

        public CalcOSServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
            logService = Substitute.For<LogService>();
            matchService = Substitute.For<OSMatchService>();

            var settings = new AppSettingsKey()
            {
                EnvironmentName = "Local",
                SiteDbServer = siteDbs
            };
            var appSettingsKey = Options.Create(settings);

            calcOSService = new CalcOSServiceImp(
                scaleOutSettleService,
                repository,
                appSettingsKey,
                logService,
                matchService
            );
        }

        [Fact]
        public async Task Settle_ShouldUpdateUnderSettlementFlag()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();
            matchService.UpdateStatusResultOS(Arg.Any<SettleCriteria>(), Arg.Any<int>());
            scaleOutSettleService.UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
            repository.Execute(Arg.Any<SettleCriteria>());
            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            scaleOutSettleService.Received(1).UpdateUnderSettlementFlag(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Settle_ShouldUpdateStatusResultOS()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            matchService.Received(1).UpdateStatusResultOS(settleCriteria, MatchStatus.Waiting);
        }

        [Fact]
        public async Task Settle_ShouldUpdateMatchInfo()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            scaleOutSettleService.Received(1).UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
        }

        [Fact]
        public async Task Settle_ShouldWriteLog()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            logService.Received(1).WriteLogResettle(Arg.Any<InsertLogCriteria>());

            logService.Received(1).WriteLogDirectly(match.MatchId, Arg.Any<string>());
        }

        [Fact]
        public async Task Settle_ShouldExecuteAllSiteDb()
        {
            // Arrange
            var match = new Match();
            var settleCriteria = new SettleCriteria();

            // Act
            await calcOSService.Settle(match, settleCriteria);

            // Assert
            repository.Received(siteDbs.Count).Execute(Arg.Any<SettleCriteria>());
        }
    }
}