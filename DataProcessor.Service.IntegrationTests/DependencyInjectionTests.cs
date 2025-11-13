using DataProcessor.Service.Data;
using DataProcessor.Service.Interfaces;
using DataProcessor.Service.Services;
using DataProcessor.Service.Workers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DataProcessor.Service.IntegrationTests;

public class DependencyInjectionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DependencyInjectionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DataProcessorService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IDataProcessor>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IDataProcessor>();
        service.Should().BeOfType<DataProcessorService>();
    }

    [Fact]
    public void DatabaseService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IDatabaseService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IDatabaseService>();
        service.Should().BeOfType<DatabaseService>();
    }

    [Fact]
    public void QueueConsumerService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IQueueConsumerService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IQueueConsumerService>();
    }

    [Fact]
    public void DataProcessingWorker_ShouldBeRegistered()
    {
        // Arrange
        var services = _factory.Services.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

        // Assert
        services.Should().NotBeNull();
        services.Should().Contain(s => s is DataProcessingWorker);
        
        var worker = services.FirstOrDefault(s => s is DataProcessingWorker);
        worker.Should().NotBeNull();
        worker.Should().BeOfType<DataProcessingWorker>();
    }

    [Fact]
    public void SensorDataDbContext_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<SensorDataDbContext>();

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<SensorDataDbContext>();
    }

    [Fact]
    public void DatabaseService_ShouldBeScoped()
    {
        // Arrange & Act
        using var scope1 = _factory.Services.CreateScope();
        var service1 = scope1.ServiceProvider.GetRequiredService<IDatabaseService>();

        using var scope2 = _factory.Services.CreateScope();
        var service2 = scope2.ServiceProvider.GetRequiredService<IDatabaseService>();

        // Assert
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void DataProcessorService_ShouldBeSingleton()
    {
        // Arrange & Act
        using var scope1 = _factory.Services.CreateScope();
        var service1 = scope1.ServiceProvider.GetRequiredService<IDataProcessor>();

        using var scope2 = _factory.Services.CreateScope();
        var service2 = scope2.ServiceProvider.GetRequiredService<IDataProcessor>();

        // Assert
        service1.Should().BeSameAs(service2);
    }
}

