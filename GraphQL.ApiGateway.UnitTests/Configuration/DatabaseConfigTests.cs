using FluentAssertions;
using GraphQL.ApiGateway.Configuration;

namespace GraphQL.ApiGateway.UnitTests.Configuration;

public class DatabaseConfigTests
{
    [Fact]
    public void DatabaseConfig_DefaultValues_ShouldBeSet()
    {
        // Act
        var config = new DatabaseConfig();

        // Assert
        config.Host.Should().Be("host.docker.internal");
        config.Port.Should().Be(5432);
        config.Database.Should().Be("sensordata");
        config.Username.Should().Be("postgres");
        config.Password.Should().Be("postgres");
    }

    [Fact]
    public void DatabaseConfig_ConnectionString_ShouldBeFormattedCorrectly()
    {
        // Arrange
        var config = new DatabaseConfig
        {
            Host = "localhost",
            Port = 5432,
            Database = "testdb",
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        var connectionString = config.ConnectionString;

        // Assert
        connectionString.Should().Be("Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass");
    }

    [Fact]
    public void DatabaseConfig_ConnectionString_WithCustomValues_ShouldIncludeAllParameters()
    {
        // Arrange
        var config = new DatabaseConfig
        {
            Host = "192.168.1.100",
            Port = 5433,
            Database = "production_db",
            Username = "admin",
            Password = "secret123"
        };

        // Act
        var connectionString = config.ConnectionString;

        // Assert
        connectionString.Should().Contain("Host=192.168.1.100");
        connectionString.Should().Contain("Port=5433");
        connectionString.Should().Contain("Database=production_db");
        connectionString.Should().Contain("Username=admin");
        connectionString.Should().Contain("Password=secret123");
    }
}

