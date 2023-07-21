using AutoPayout.Criteria;
using DirectPayout.Fantasy.Criteria;
using DirectPayout.Fantasy.Models;
using DirectPayout.Fantasy.Services;
using Fanex.Data.Repository;
using NSubstitute;

namespace DirectPayout.Fantasy.UnitTests.Services;

public class LogServiceTests
{
    private readonly LogService logService;
    private readonly IDynamicRepository repository;

    public LogServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        logService = new LogServiceImp(repository);
    }

    [Fact]
    public async void Write_CallExecuteAsync()
    {
        // Arrange
        var match = new Match();
        var message = string.Empty;

        // Act
        await logService.Write(match, message);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<SettleLogCriteria>());
    }

    [Fact]
    public void WriteDirectly_CallExecuteAsync()
    {
        // Arrange
        var match = new Match();
        var message = string.Empty;

        // Act
        logService.WriteDirectly(match, message);

        // Assert
        repository.Received(1).Execute(Arg.Any<LogDirectlyCriteria>());
    }
}