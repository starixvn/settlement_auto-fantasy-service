using AutoMapper;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services;

public class CalcServiceTests
{
    private readonly ScaleOutSettleService scaleOutService;
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly LogService logService;

    private readonly CalcService calcService;

    private readonly CriteriaService createCriteriaService;

    public CalcServiceTests()
    {
        scaleOutService = Substitute.For<ScaleOutSettleService>();
        repository = Substitute.For<IDynamicRepository>();
        mapper = Substitute.For<IMapper>();
        logService = Substitute.For<LogService>();
        createCriteriaService = Substitute.For<CriteriaService>();

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local",
            SiteDbServer = new List<string> { "DBTXN", "DBSITE01", "DBSITE02" }
        };
        var appSettingsKey = Options.Create(settings);

        calcService = new CalcServiceImp(scaleOutService, repository, mapper, appSettingsKey, logService, createCriteriaService);
    }

    [Fact]
    public async Task InsertAutoPayoutResult_MatchHalftime_ShouldExecuteAsyncWithCorrectCriteria()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = SettleType.Halftime,
            HtHomeScore = 2,
            HtAwayScore = 1,
            FinalHomeScore = 4,
            FinalAwayScore = 2,
        };
        const int sourceId = 100;
        const int statusId = 200;

        var criteria = new AddAutoPayoutResultCriteria
        {
            MatchId = 1,
            TypeId = 1,
            SourceId = sourceId,
            StatusId = statusId,
            HomeValue = 2,
            AwayValue = 1
        };
        createCriteriaService.CreateAutoPayoutResult(Arg.Any<Match>(), Arg.Any<int>(), Arg.Any<int>()).Returns(criteria);

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

        createCriteriaService.CreateMatchSettleCriteria(match).Returns(new MatchSettleCriteria());
        createCriteriaService.CreateAutoPayoutResult(match, Arg.Any<int>(), Arg.Any<int>()).Returns(new AddAutoPayoutResultCriteria());
        createCriteriaService.CreateLogCalcCriteria(match).Returns(new InsertLogCriteria());

        await calcService.Settle(match);

        await scaleOutService.Received(1).UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await repository.Received(2).ExecuteAsync(Arg.Any<AddAutoPayoutResultCriteria>());

        await repository.Received(2).ExecuteAsync(Arg.Any<InsertLogCriteria>());

        await scaleOutService.Received(1).UpdateMatchInfo(Arg.Is<UpdateMatchInfoRequest>(request =>
            request.MatchId == match.MatchId &&
            request.HalfTimeAway == match.HtAwayScore &&
            request.HalfTimeHome == match.HtHomeScore &&
            request.FullTimeAway == match.FinalAwayScore &&
            request.FullTimeHome == match.FinalHomeScore &&
            request.Action == match.Action &&
            request.IsAuto));

        await logService.Received(1).WriteLogDirectly(Arg.Any<int>(), Arg.Any<string>());

        await repository.Received(4).ExecuteAsync(Arg.Any<MatchSettleCriteria>());
    }
}