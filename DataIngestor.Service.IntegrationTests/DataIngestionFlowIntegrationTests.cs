using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DataIngestor.Service.IntegrationTests;

public class DataIngestionFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly Mock<IExternalApiService> _mockApiService;

    public DataIngestionFlowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _mockApiService = new Mock<IExternalApiService>();
    }

    [Fact]
    public void DataIngestionWorker_ShouldBeRegistered_WithMockedServices()
    {
        // Arrange
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IExternalApiService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton(_mockApiService.Object);
            });
        });

        // Act
        using var scope = factory.Services.CreateScope();
        var worker = scope.ServiceProvider.GetService<Microsoft.Extensions.Hosting.IHostedService>();

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public void SensorDataProcessor_ShouldBeRegistered_WithMockedServices()
    {
        // Arrange
        var mockQueueService = new Mock<IQueueService>();
        mockQueueService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);

        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var apiDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IExternalApiService));
                if (apiDescriptor != null)
                {
                    services.Remove(apiDescriptor);
                }
                var queueDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IQueueService));
                if (queueDescriptor != null)
                {
                    services.Remove(queueDescriptor);
                }
                services.AddSingleton(_mockApiService.Object);
                services.AddSingleton(mockQueueService.Object);
            });
        });

        // Act
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetService<ISensorDataProcessor>();

        // Assert
        processor.Should().NotBeNull();
    }
}


