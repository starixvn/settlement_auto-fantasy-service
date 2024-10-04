using DirectFantasy.Criteria;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services;

public class RefundServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly LogService logService;
    private readonly RefundServiceImp refundService;

    public RefundServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        logService = Substitute.For<LogService>();
        var createCriteriaService = Substitute.For<CriteriaService>();
        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local",
            SiteDbServer = new List<string> { "DBTXN", "DBSITE01", "DBSITE02" }
        };
        var appSettingsKey = Options.Create(settings);

        refundService = new RefundServiceImp(repository, scaleOutSettleService, appSettingsKey, logService, createCriteriaService);
    }

    [Fact]
    public async Task InsertRefundResult_ValidMatch_InvokeExecuteAsync()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = 2
        };

        var criteria = new RefundResultInsertCriteria
        {
            ParentId = match.MatchId,
            TypeId = match.TypeId,
        };

        // Act
        await refundService.InsertRefundResult(match);

        // Assert
        await repository.Received().ExecuteAsync(
            Arg.Is<RefundResultInsertCriteria>(param
                => param.ParentId == criteria.ParentId && param.TypeId == criteria.TypeId));
    }

    [Fact]
    public async Task HandleRefundPush_ValidMatch_InvokeActions()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = 2
        };

        // Act
        await refundService.HandleRefundPush(match);

        // Assert
        await scaleOutSettleService.Received(1).UpdateUnderSettlementFlag(match.MatchId, 0, match.Action);

        await scaleOutSettleService.Received(1).UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());

        await logService.Received(1).WriteLogDirectly(Arg.Is<int>(x => x == match.MatchId), Arg.Any<string>());

        repository.Received(3).Execute(Arg.Any<RefundPushCriteria>());
    }
}