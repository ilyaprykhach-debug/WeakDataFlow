using FluentAssertions;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace GraphQL.ApiGateway.IntegrationTests;

public class GraphQLQueriesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GraphQLQueriesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SensorDataDbContext>();

        if (!context.SensorReadings.Any())
        {
            context.SensorReadings.AddRange(new List<SensorReading>
            {
                new SensorReading
                {
                    Id = "test-1",
                    SensorId = "sensor-1",
                    Type = "energy",
                    Location = "Office A",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    EnergyConsumption = 100.5m
                },
                new SensorReading
                {
                    Id = "test-2",
                    SensorId = "sensor-2",
                    Type = "air_quality",
                    Location = "Office B",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Co2 = 400,
                    Pm25 = 25,
                    Humidity = 60
                },
                new SensorReading
                {
                    Id = "test-3",
                    SensorId = "sensor-3",
                    Type = "energy",
                    Location = "Office A",
                    Timestamp = DateTime.UtcNow.AddHours(-3),
                    EnergyConsumption = 200.0m
                }
            });
            context.SaveChanges();
        }
    }

    [Fact]
    public async Task GraphQL_GetSensorReadings_ShouldReturnData()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    sensorReadings(first: 10) {
                        nodes {
                            id
                            sensorId
                            type
                            location
                            timestamp
                            energyConsumption
                        }
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("sensorReadings");
        content.Should().Contain("test-1");
    }

    [Fact]
    public async Task GraphQL_GetSensorReadingById_ShouldReturnSingleReading()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    sensorReadingById(id: ""test-1"") {
                        id
                        sensorId
                        type
                        location
                        energyConsumption
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("test-1");
        content.Should().Contain("sensor-1");
        content.Should().Contain("energy");
    }

    [Fact]
    public async Task GraphQL_GetSensorReadingsByLocation_ShouldFilterByLocation()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    sensorReadingsByLocation(location: ""Office A"") {
                        id
                        location
                        type
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Office A");
    }

    [Fact]
    public async Task GraphQL_GetSensorReadingsByType_ShouldFilterByType()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    sensorReadingsByType(type: ""energy"") {
                        id
                        type
                        energyConsumption
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("energy");
    }

    [Fact]
    public async Task GraphQL_GetAggregationsByLocation_ShouldReturnAggregations()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    aggregationsByLocation {
                        groupBy
                        count
                        averageEnergyConsumption
                        totalEnergyConsumption
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("aggregationsByLocation");
        content.Should().Contain("groupBy");
        content.Should().Contain("count");
    }

    [Fact]
    public async Task GraphQL_GetAggregationsByType_ShouldReturnAggregations()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    aggregationsByType {
                        groupBy
                        count
                        averageCo2
                        averagePm25
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("aggregationsByType");
        content.Should().Contain("groupBy");
    }

    [Fact]
    public async Task GraphQL_GetAggregationsByTimePeriod_ShouldReturnAggregations()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    aggregationsByTimePeriod(period: ""day"") {
                        groupBy
                        count
                        averageEnergyConsumption
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("aggregationsByTimePeriod");
        content.Should().Contain("groupBy");
    }

    [Fact]
    public async Task GraphQL_InvalidQuery_ShouldReturnError()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    nonExistentQuery {
                        field
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            content.Should().Contain("errors");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GraphQL_WithFiltering_ShouldFilterResults()
    {
        // Arrange
        var query = new
        {
            query = @"
                query {
                    sensorReadings(
                        where: {
                            type: { eq: ""energy"" }
                        }
                        first: 10
                    ) {
                        nodes {
                            id
                            type
                            energyConsumption
                        }
                    }
                }"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/graphql", query);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("energy");
    }
}

