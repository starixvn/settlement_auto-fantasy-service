using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services;

public class CreateCriteriaServiceTests
{
    private readonly CreateCriteriaService createCriteriaService;

    private readonly XmlConvertService xmlConvertService;

    public CreateCriteriaServiceTests()
    {
        xmlConvertService = Substitute.For<XmlConvertService>();
        createCriteriaService = new CreateCriteriaServiceImp(xmlConvertService);
    }

    [Fact]
    public void CreateAutoPayoutResult_MatchHalftime_ReturnsHalftimeScore()
    {
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

        var value = createCriteriaService.CreateAutoPayoutResult(match, sourceId, statusId);

        var expected = new AddAutoPayoutResultCriteria
        {
            MatchId = 1,
            TypeId = SettleType.Halftime,
            SourceId = sourceId,
            StatusId = statusId,
            HomeValue = 2,
            AwayValue = 1,
        };
        AssertExtension.AreEqual(expected, value);
    }

    [Fact]
    public void CreateAutoPayoutResult_MatchFullTime_ReturnsFinalScore()
    {
        var match = new Match
        {
            MatchId = 1,
            TypeId = SettleType.FullTime,
            HtHomeScore = 2,
            HtAwayScore = 1,
            FinalHomeScore = 4,
            FinalAwayScore = 2,
        };
        const int sourceId = 100;
        const int statusId = 200;

        var value = createCriteriaService.CreateAutoPayoutResult(match, sourceId, statusId);

        var expected = new AddAutoPayoutResultCriteria
        {
            MatchId = 1,
            TypeId = SettleType.FullTime,
            SourceId = sourceId,
            StatusId = statusId,
            HomeValue = 4,
            AwayValue = 2,
        };
        AssertExtension.AreEqual(expected, value);
    }

    [Theory]
    [InlineData(1, "CAL-HT")]
    [InlineData(2, "CAL-FT")]
    public void CreateLogCalcCriteria_InputTypeId_ReturnsCorrectActionName(int typeId, string expectedActionName)
    {
        var match = new Match
        {
            MatchId = 1,
            TypeId = typeId,
            HtHomeScore = 2,
            HtAwayScore = 1,
            FinalHomeScore = 4,
            FinalAwayScore = 2,
        };
        var value = createCriteriaService.CreateLogCalcCriteria(match);
        Assert.Equal(value.Action, expectedActionName);
    }
}