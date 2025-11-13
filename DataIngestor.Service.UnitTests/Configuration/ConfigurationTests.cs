using DataIngestor.Service.Configuration;
using FluentAssertions;

namespace DataIngestor.Service.UnitTests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void ExternalApiConnectionConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new ExternalApiConnectionConfig();

        // Assert
        config.BaseUrl.Should().Be("http://weakapp:8080");
        config.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void ExternalApiRetryConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new ExternalApiRetryConfig();

        // Assert
        config.RetryCount.Should().Be(3);
    }

    [Fact]
    public void ExternalApiHeadersConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new ExternalApiHeadersConfig();

        // Assert
        config.XApiKey.Should().Be("supersecret");
    }

    [Fact]
    public void QueueConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new QueueConfig();

        // Assert
        config.Host.Should().Be("rabbitmq");
        config.Port.Should().Be(5672);
        config.QueueName.Should().Be("sensor-data");
        config.Username.Should().Be("guest");
        config.Password.Should().Be("guest");
    }

    [Fact]
    public void DataIngestionConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new DataIngestionConfig();

        // Assert
        config.IntervalSeconds.Should().Be(15);
        config.InitialDelaySeconds.Should().Be(5);
    }
}

