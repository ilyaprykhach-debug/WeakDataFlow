using DataIngestor.Service.Interfaces;
using DataIngestor.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DataIngestor.Service.IntegrationTests;

public class ExternalApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ExternalApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ExternalApiService_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IExternalApiService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IExternalApiService>();
    }

    [Fact]
    public async Task ExternalApiService_ShouldFetchData_WithMockedHttpClient()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseData = new List<WeakApiResponse>
        {
            new WeakApiResponse
            {
                Type = "energy",
                Name = "Test Meter",
                Payload = JsonSerializer.SerializeToElement(new { energy = 123.45m })
            }
        };

        var jsonResponse = JsonSerializer.Serialize(responseData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://test-api.com")
        };

        using var scope = _factory.Services.CreateScope();
        var connectionConfig = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DataIngestor.Service.Configuration.ExternalApiConnectionConfig>>();
        var headersConfig = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DataIngestor.Service.Configuration.ExternalApiHeadersConfig>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataIngestor.Service.Services.ExternalApiService>>();
        var service = new DataIngestor.Service.Services.ExternalApiService(httpClient, logger, connectionConfig, headersConfig);

        // Act
        var result = await service.FetchDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Type.Should().Be("energy");
    }
}

