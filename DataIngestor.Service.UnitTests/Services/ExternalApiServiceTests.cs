using DataIngestor.Service.Configuration;
using DataIngestor.Service.Models;
using DataIngestor.Service.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DataIngestor.Service.UnitTests.Services;

public class ExternalApiServiceTests
{
    private readonly Mock<IOptions<ExternalApiConnectionConfig>> _mockConnectionConfig;
    private readonly Mock<IOptions<ExternalApiHeadersConfig>> _mockHeadersConfig;
    private readonly Mock<ILogger<ExternalApiService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ExternalApiService _service;

    public ExternalApiServiceTests()
    {
        _mockConnectionConfig = new Mock<IOptions<ExternalApiConnectionConfig>>();
        _mockConnectionConfig.Setup(x => x.Value).Returns(new ExternalApiConnectionConfig
        {
            BaseUrl = "http://test-api.com",
            TimeoutSeconds = 30
        });

        _mockHeadersConfig = new Mock<IOptions<ExternalApiHeadersConfig>>();
        _mockHeadersConfig.Setup(x => x.Value).Returns(new ExternalApiHeadersConfig
        {
            XApiKey = "test-api-key"
        });

        _mockLogger = new Mock<ILogger<ExternalApiService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://test-api.com")
        };

        _service = new ExternalApiService(_httpClient, _mockLogger.Object, _mockConnectionConfig.Object, _mockHeadersConfig.Object);
    }

    [Fact]
    public async Task FetchDataAsync_ShouldReturnSensorReadings_WhenApiReturnsValidData()
    {
        // Arrange
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

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Type.Should().Be("energy");
        result[0].Location.Should().Be("Test Meter");
        result[0].EnergyConsumption.Should().Be(123.45m);
    }

    [Fact]
    public async Task FetchDataAsync_ShouldReturnEmptyList_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal Server Error", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FetchDataAsync_ShouldIncludeApiKeyHeader()
    {
        // Arrange
        var responseData = new List<WeakApiResponse>();
        var jsonResponse = JsonSerializer.Serialize(responseData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _service.FetchDataAsync();

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().ContainKey("X-Api-Key");
        capturedRequest.Headers.GetValues("X-Api-Key").Should().Contain("test-api-key");
    }

    [Fact]
    public async Task FetchDataAsync_ShouldThrowException_WhenHttpClientThrows()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.FetchDataAsync());
    }

    [Fact]
    public async Task FetchDataAsync_ShouldHandleMultipleSensorTypes()
    {
        // Arrange
        var responseData = new List<WeakApiResponse>
        {
            new WeakApiResponse
            {
                Type = "energy",
                Name = "Energy Meter",
                Payload = JsonSerializer.SerializeToElement(new { energy = 100.5m })
            },
            new WeakApiResponse
            {
                Type = "air_quality",
                Name = "Air Quality Sensor",
                Payload = JsonSerializer.SerializeToElement(new { co2 = 400, pm25 = 20, humidity = 50 })
            },
            new WeakApiResponse
            {
                Type = "motion",
                Name = "Motion Sensor",
                Payload = JsonSerializer.SerializeToElement(new { motionDetected = true })
            }
        };

        var jsonResponse = JsonSerializer.Serialize(responseData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.FetchDataAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Type == "energy" && r.EnergyConsumption == 100.5m);
        result.Should().Contain(r => r.Type == "air_quality" && r.Co2 == 400 && r.Pm25 == 20 && r.Humidity == 50);
        result.Should().Contain(r => r.Type == "motion" && r.MotionDetected == true);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnTrue_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.CheckHealthAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnFalse_WhenApiReturnsNonSuccess()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _service.CheckHealthAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldReturnFalse_WhenExceptionOccurs()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        // Act
        var result = await _service.CheckHealthAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldIncludeApiKeyHeader()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) => capturedRequest = request)
            .ReturnsAsync(httpResponse);

        // Act
        await _service.CheckHealthAsync();

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().ContainKey("X-Api-Key");
        capturedRequest.Headers.GetValues("X-Api-Key").Should().Contain("test-api-key");
    }
}

