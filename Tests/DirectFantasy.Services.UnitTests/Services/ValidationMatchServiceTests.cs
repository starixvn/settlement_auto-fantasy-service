using AutoMapper;
using DirectFantasy.Core.Exceptions;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Criteria;
using DirectFantasy.Services.Models;
using DirectFantasy.Services.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DirectFantasy.Services.UnitTests.Services;

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
            SiteDbServer = new List<string> { "DBTXN", "DBSID01", "DBSID02" }
        };
        var appSettingsKey = Options.Create(settings);

        validationMatchService = new ValidationMatchServiceImp(appSettingsKey, repository, mapper, generalService);
    }

    [Fact]
    public async void ValidateTicket_ValidMatch_ReturnResultSuccess()
    {
        var match = new Match
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

        var result = await validationMatchService.ValidateEventTicket(match);

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

        var exception = Record.ExceptionAsync(() => validationMatchService.ValidateTicketMatch(match));

        Assert.Null(exception.Result);
    }

    [Fact]
    public void ValidateTicketMatch_ExistMarkedTicket_ThrowException()
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

        Assert.ThrowsAsync<MarkedTicketException>(() => validationMatchService.ValidateTicketMatch(match));
    }
}