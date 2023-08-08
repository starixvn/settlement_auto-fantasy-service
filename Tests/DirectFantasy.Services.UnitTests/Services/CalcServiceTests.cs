using AutoMapper;
using DirectFantasy.Core.Constants;
using DirectFantasy.Core.Models;
using DirectFantasy.Core.Services;
using DirectFantasy.Services.Criteria;
using DirectFantasy.Services.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.Services.UnitTests.Services;

public class CalcServiceTests
{
    private readonly ScaleOutSettleService scaleOutService;
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly LogService logService;

    private readonly CalcService calcService;

    public CalcServiceTests()
    {
        scaleOutService = Substitute.For<ScaleOutSettleService>();
        repository = Substitute.For<IDynamicRepository>();
        mapper = Substitute.For<IMapper>();
        logService = Substitute.For<LogService>();

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local",
            SiteDbServer = new List<string> { "DBTXN", "DBSID01", "DBSID02" }
        };
        var appSettingsKey = Options.Create(settings);

        calcService = new CalcServiceImp(scaleOutService, repository, mapper, appSettingsKey, logService);
    }

    [Fact]
    public async Task InsertAutoPayoutResult_MatchHalftime_ShouldExecuteAsyncWithCorrectCriteria()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = SettleType.Halftime, // Assuming this is the halftime type
            HtHomeScore = 2,
            HtAwayScore = 1,
            FinalHomeScore = 4,
            FinalAwayScore = 2,
        };
        var sourceId = 100;
        var statusId = 200;

        // Act
        await calcService.InsertAutoPayoutResult(match, sourceId, statusId);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<AddAutoPayoutResultCriteria>(c =>
            c.MatchId == match.MatchId &&
            c.TypeId == match.TypeId &&
            c.SourceId == sourceId &&
            c.StatusId == statusId &&
            c.HomeValue == 2 &&
            c.AwayValue == 1
        ));
    }

    [Fact]
    public async Task InsertAutoPayoutResult_MatchFullTime_ShouldExecuteAsyncWithCorrectCriteria()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = SettleType.FullTime, // Assuming this is the halftime type
            HtHomeScore = 2,
            HtAwayScore = 1,
            FinalHomeScore = 4,
            FinalAwayScore = 2,
        };
        var sourceId = 100;
        var statusId = 200;

        // Act
        await calcService.InsertAutoPayoutResult(match, sourceId, statusId);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<AddAutoPayoutResultCriteria>(c =>
            c.MatchId == match.MatchId &&
            c.TypeId == match.TypeId &&
            c.SourceId == sourceId &&
            c.StatusId == statusId &&
            c.HomeValue == 4 &&
            c.AwayValue == 2
        ));
    }

    [Fact]
    public async Task Settle_MatchValid_ShouldPerformSettlementActions()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            Action = "CAL",
            HtAwayScore = 1,
            HtHomeScore = 1,
            FinalAwayScore = 1,
            FinalHomeScore = 1,
            TypeId = 2
        };

        var matchInfoRequest = new UpdateMatchInfoRequest
        {
            HalfTimeAway = 1,
            HalfTimeHome = 1,
            FullTimeAway = 1,
            FullTimeHome = 1,
            MatchId = 1,
            Action = "CAL"
        };
        mapper.Map<UpdateMatchInfoRequest>(Arg.Any<MatchSettleCriteria>()).Returns(matchInfoRequest);

        // Act
        await calcService.Settle(match);

        // Assert
        await scaleOutService.Received(1).UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await repository.Received(1).ExecuteAsync(Arg.Any<AddAutoPayoutResultCriteria>());

        await scaleOutService.Received(1).UpdateMatchInfo(Arg.Is<UpdateMatchInfoRequest>(request =>
            request.MatchId == match.MatchId &&
            request.HalfTimeAway == match.HtAwayScore &&
            request.HalfTimeHome == match.HtHomeScore &&
            request.FullTimeAway == match.FinalAwayScore &&
            request.FullTimeHome == match.FinalHomeScore &&
            request.Action == match.Action &&
            request.IsAuto));

        logService.Received(1).WriteDirectly(Arg.Any<int>(), Arg.Any<string>());

        repository.Received(3).Execute(Arg.Any<MatchSettleCriteria>());
    }
}