using AutoMapper;
using DirectFantasy.Constants;
using DirectFantasy.Criteria;
using DirectFantasy.Criteria.OtherSports;
using DirectFantasy.Exceptions;
using DirectFantasy.Models;
using DirectFantasy.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.UnitTests.Services;

public class ValidationMatchServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly IMapper mapper;
    private readonly GeneralService generalService;
    private readonly ValidationMatchServiceImp validationMatchService;

    public ValidationMatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        generalService = Substitute.For<GeneralService>();
        mapper = Substitute.For<IMapper>();

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local",
            SiteDbServer = ["DBTXN", "DBSITE01", "DBSITE02"]
        };
        var appSettingsKey = Options.Create(settings);

        validationMatchService = new ValidationMatchServiceImp(appSettingsKey, repository, mapper, generalService);
    }

    [Theory]
    [InlineData("REFUND")]
    [InlineData("CALC")]
    public async Task ValidateEventTicket_ValidMatch_ReturnResultSuccess(string actionName)
    {
        var match = new Match
        {
            MatchId = 1,
            Action = actionName
        };

        mapper.Map<ValidateTicketCriteria>(Arg.Any<EventTicketRequest>()).Returns(new ValidateTicketCriteria());
        generalService.ExecuteAsync(Arg.Any<ValidateTicketCriteria>()).Returns(new ResultMessage
        {
            ErrorCode = 0,
            ErrorMessage = ""
        });

        var result = await validationMatchService.ValidateEventTicket(match);

        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ValidateEventTicketOS_ValidMatch_ReturnResultSuccess()
    {
        var criteria = new SettleCriteria
        {
            MatchId = 1,
            Action = "REFUND"
        };

        mapper.Map<ValidateTicketCriteria>(Arg.Any<EventTicketRequest>()).Returns(new ValidateTicketCriteria());
        generalService.ExecuteAsync(Arg.Any<ValidateTicketCriteria>()).Returns(new ResultMessage
        {
            ErrorCode = 0,
            ErrorMessage = ""
        });

        var result = await validationMatchService.ValidateEventTicketOS(criteria);

        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public void ValidateTicketMatch_NotExistMarkedTicket_DoNotReturnOrThrowAnything()
    {
        var match = new Match
        {
            MatchId = 1,
            Action = "REFUND"
        };
        repository.GetAsync<MatchTicketInfo>(Arg.Any<MatchTicketInfoCriteria>()).Returns(new MatchTicketInfo());

        var exception = Record.ExceptionAsync(async () => await validationMatchService.ValidateTicketMatch(match));

        Assert.Null(exception?.Result);
    }

    [Fact]
    public async Task ValidateTicketMatch_ExistMarkedTicket_ThrowException()
    {
        var match = new Match
        {
            MatchId = 1,
            Action = "REFUND"
        };
        repository.GetAsync<MatchTicketInfo>(Arg.Is<MatchTicketInfoCriteria>(c => c.DbServer == "DBTXN")).Returns(new MatchTicketInfo
        {
            IsExistMarkedTicket = true
        });

        repository.GetAsync<MatchTicketInfo>(Arg.Is<MatchTicketInfoCriteria>(c => c.DbServer != "DBTXN")).Returns(new MatchTicketInfo());

        await Assert.ThrowsAsync<MarkedTicketException>(async () => await validationMatchService.ValidateTicketMatch(match));
    }

    [Fact]
    public void ValidateRelatedMatch_Insufficient_ThrowException()
    {
        var match = new FantasyMatch
        {
            FantasyMatchDetail = new FantasyMatchDetail
            {
                FantasyHomeTeam = "123",
                FantasyAwayTeam = "456"
            },
            RelatedMatches = []
        };

        Assert.Throws<RelatedMatchesInsufficientException>(() => validationMatchService.ValidateRelatedMatch(match));
    }

    [Fact]
    public void ValidateRelatedMatch_SufficientAndHoldStatus_ThrowException()
    {
        var remark = new RemarkSettingModel
        {
            SettlementStatus = "11111"
        };
        var match = new FantasyMatch
        {
            FantasyMatchDetail = new FantasyMatchDetail
            {
                FantasyHomeTeam = "1",
                FantasyAwayTeam = "2"
            },
            RelatedMatches = [new RelatedMatch { HomeId = 1, AwayId = 2, Remark = remark }]
        };

        Assert.Throws<HoldStatusException>(() => validationMatchService.ValidateRelatedMatch(match));
    }

    [Fact]
    public void ValidateRelatedMatch_SufficientNotHoldAndNotCompleted_ThrowException()
    {
        var remark = new RemarkSettingModel
        {
            SettlementStatus = "0"
        };
        var match = new FantasyMatch
        {
            FantasyMatchDetail = new FantasyMatchDetail
            {
                FantasyHomeTeam = "1",
                FantasyAwayTeam = "2"
            },
            RelatedMatches = [new RelatedMatch { HomeId = 1, AwayId = 2, Remark = remark, EventStatus = EventStatus.Running }]
        };

        Assert.Throws<RelatedMatchesNotCalculatedException>(() => validationMatchService.ValidateRelatedMatch(match));
    }

    [Fact]
    public void ValidateRelatedMatch_SufficientNotHoldAndCompleted_DoNotReturnOrThrowAnything()
    {
        var remark = new RemarkSettingModel
        {
            SettlementStatus = "0"
        };
        var match = new FantasyMatch
        {
            FantasyMatchDetail = new FantasyMatchDetail
            {
                FantasyHomeTeam = "1",
                FantasyAwayTeam = "2"
            },
            RelatedMatches = [new RelatedMatch { HomeId = 1, AwayId = 2, Remark = remark, EventStatus = EventStatus.Completed }]
        };

        var exception = Record.Exception(() => validationMatchService.ValidateRelatedMatch(match));

        Assert.Null(exception);
    }
}