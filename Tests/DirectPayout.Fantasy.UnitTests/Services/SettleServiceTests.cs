//using AutoMapper;
//using AutoPayout.Constants;
//using AutoPayout.Criteria;
//using AutoPayout.Models;
//using DirectPayout.Fantasy.Models;
//using DirectPayout.Fantasy.Services;
//using Fanex.Data.Repository;
//using Fanex.Settlement.ScaleOut.Models;
//using Fanex.Settlement.ScaleOut.Request;
//using Fanex.Settlement.ScaleOut.Services;
//using Microsoft.Extensions.Options;
//using NSubstitute;

//namespace DirectPayout.Fantasy.UnitTests.Services;

//#nullable disable

//public class SettleServiceTests
//{
//    private readonly SettleService settleService;
//    private readonly IDynamicRepository repository;
//    private readonly ScaleOutSettleService scaleOutSettleService;
//    private readonly IMapper mapper;

//    public SettleServiceTests()
//    {
//        var settings = new AppSettingsKey()
//        {
//            RetryInterval = 1000,
//            RetryCount = 1,
//            EnvironmentName = "Local",
//            SiteDbServer = new List<string> { "S1", "S2" }
//        };

//        var appSettingsKey = Options.Create(settings);

//        repository = Substitute.For<IDynamicRepository>();

//        scaleOutSettleService = Substitute.For<ScaleOutSettleService>();

//        mapper = Substitute.For<IMapper>();

//        settleService = new SettleServiceImp(scaleOutSettleService, repository, mapper, appSettingsKey);
//    }

//    [Fact]
//    public async Task VerifyMatch_ValidMatch_ReturnSuccess()
//    {
//        // Arrange

//        scaleOutSettleService.ValidateEventAndTicket(Arg.Any<EventTicketRequest>()).Returns(new ResultMessage());

//        // Act
//        var result = await settleService.VerifyMatch(new Match());

//        // Assert
//        Assert.True(result.IsSuccessful);
//    }

//    [Fact]
//    public async Task InsertAutoPayoutResult_ValidInput_InvokeExecuteAsync()
//    {
//        // Arrange
//        var match = new Match
//        {
//            MatchId = 1
//        };

//        // Act
//        await settleService.InsertAutoPayoutResult(match, Source.InfoSite, MatchStatus.Completed);

//        // Assert
//        await repository.Received(1).ExecuteAsync(Arg.Any<AddAutoPayoutResultCriteria>());
//    }

//    [Fact]
//    public async Task Settle_ValidMatch_InvokeCorrectStepSettle()
//    {
//        // Arrange
//        var match = new Match
//        {
//            MatchId = 1
//        };
//        mapper.Map<MatchSettleCriteria, UpdateMatchInfoRequest>(Arg.Any<MatchSettleCriteria>())
//            .Returns(new UpdateMatchInfoRequest());

//        // Act
//        await settleService.Settle(match);

//        // Assert
//        await scaleOutSettleService.Received(1).UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);
//        await repository.Received(1).ExecuteAsync(Arg.Any<AddAutoPayoutResultCriteria>());
//        await scaleOutSettleService.Received(1).UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
//        repository.Received(2).Execute(Arg.Any<MatchSettleCriteria>());
//    }
//}