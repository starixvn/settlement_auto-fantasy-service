using AutoMapper;
using DirectFantasy.Core.Models;
using DirectFantasy.Services.Services;
using Fanex.Data.Repository;
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
        // Arrange
        //var match = new Match
        //{
        //    MatchId = 1,
        //    Action = "REFUND"
        //};

        //mapper.Map<ValidateTicketCriteria>(Arg.Any<EventTicketRequest>()).Returns(new ValidateTicketCriteria());
        //generalService.ExecuteAsync(Arg.Any<ValidateTicketCriteria>()).Returns(new ResultMessage
        //{
        //    ErrorCode = 0,
        //    ErrorMessage = ""
        //});

        //// Act
        //var result = await validationMatchService.ValidateEventTicket(match);

        //// Assert
        //Assert.NotNull(result);
    }
}