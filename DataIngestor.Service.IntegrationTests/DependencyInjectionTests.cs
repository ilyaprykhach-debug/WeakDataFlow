using DataIngestor.Service.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DataIngestor.Service.IntegrationTests;

public class DependencyInjectionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DependencyInjectionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Services_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert
        var apiService = serviceProvider.GetService<IExternalApiService>();
        apiService.Should().NotBeNull();

        var queueService = serviceProvider.GetService<IQueueService>();
        queueService.Should().NotBeNull();

        var dataProcessor = serviceProvider.GetService<ISensorDataProcessor>();
        dataProcessor.Should().NotBeNull();
    }

    [Fact]
    public void Services_ShouldBeSingleton()
    {
        // Arrange
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        // Act
        var service1 = scope1.ServiceProvider.GetService<IExternalApiService>();
        var service2 = scope2.ServiceProvider.GetService<IExternalApiService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
    }

    [Fact]
    public void Configuration_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var apiConfig = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DataIngestor.Service.Configuration.ExternalApiConfig>>();
        var queueConfig = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DataIngestor.Service.Configuration.QueueConfig>>();
        var ingestionConfig = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DataIngestor.Service.Configuration.DataIngestionConfig>>();

        // Assert
        apiConfig.Should().NotBeNull();
        apiConfig!.Value.BaseUrl.Should().Be("http://localhost:8080");

        queueConfig.Should().NotBeNull();
        queueConfig!.Value.Host.Should().Be("localhost");

        ingestionConfig.Should().NotBeNull();
        ingestionConfig!.Value.IntervalSeconds.Should().Be(60);
    }
}

