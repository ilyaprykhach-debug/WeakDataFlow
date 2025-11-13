using DataProcessor.Service.Configuration;
using FluentAssertions;

namespace DataProcessor.Service.UnitTests.Configuration;

public class DataProcessingConfigTests
{
    [Fact]
    public void DataProcessingConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new DataProcessingConfig();

        // Assert
        config.BatchSize.Should().Be(10);
        config.ProcessingIntervalSeconds.Should().Be(5);
        config.InitialDelaySeconds.Should().Be(2);
    }

    [Fact]
    public void DataProcessingConfig_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var config = new DataProcessingConfig
        {
            BatchSize = 20,
            ProcessingIntervalSeconds = 10,
            InitialDelaySeconds = 5
        };

        // Assert
        config.BatchSize.Should().Be(20);
        config.ProcessingIntervalSeconds.Should().Be(10);
        config.InitialDelaySeconds.Should().Be(5);
    }
}

public class QueueConfigTests
{
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
    public void QueueConfig_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var config = new QueueConfig
        {
            Host = "custom-host",
            Port = 5673,
            QueueName = "custom-queue",
            Username = "custom-user",
            Password = "custom-password"
        };

        // Assert
        config.Host.Should().Be("custom-host");
        config.Port.Should().Be(5673);
        config.QueueName.Should().Be("custom-queue");
        config.Username.Should().Be("custom-user");
        config.Password.Should().Be("custom-password");
    }
}

public class DatabaseConfigTests
{
    [Fact]
    public void DatabaseConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new DatabaseConfig();

        // Assert
        config.Host.Should().Be("host.docker.internal");
        config.Port.Should().Be(5432);
        config.Database.Should().Be("sensordata");
        config.Username.Should().Be("postgres");
        config.Password.Should().Be("postgres");
    }

    [Fact]
    public void DatabaseConfig_ShouldGenerateConnectionString()
    {
        // Arrange & Act
        var config = new DatabaseConfig
        {
            Host = "localhost",
            Port = 5432,
            Database = "testdb",
            Username = "testuser",
            Password = "testpass"
        };

        // Assert
        config.ConnectionString.Should().Be("Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass");
    }

    [Fact]
    public void DatabaseConfig_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var config = new DatabaseConfig
        {
            Host = "custom-host",
            Port = 5433,
            Database = "custom-db",
            Username = "custom-user",
            Password = "custom-password"
        };

        // Assert
        config.Host.Should().Be("custom-host");
        config.Port.Should().Be(5433);
        config.Database.Should().Be("custom-db");
        config.Username.Should().Be("custom-user");
        config.Password.Should().Be("custom-password");
        config.ConnectionString.Should().Contain("custom-host");
        config.ConnectionString.Should().Contain("custom-db");
        config.ConnectionString.Should().Contain("custom-user");
        config.ConnectionString.Should().Contain("custom-password");
    }
}

