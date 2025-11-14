using FluentAssertions;
using GraphQL.ApiGateway.GraphQL.Types;

namespace GraphQL.ApiGateway.UnitTests.Types;

public class AggregationResultTests
{
    [Fact]
    public void AggregationResult_DefaultValues_ShouldBeSet()
    {
        // Act
        var result = new AggregationResult();

        // Assert
        result.GroupBy.Should().BeEmpty();
        result.Count.Should().Be(0);
        result.AverageEnergyConsumption.Should().BeNull();
        result.AverageCo2.Should().BeNull();
        result.AveragePm25.Should().BeNull();
        result.AverageHumidity.Should().BeNull();
        result.TotalEnergyConsumption.Should().BeNull();
    }

    [Fact]
    public void AggregationResult_Properties_ShouldBeSettable()
    {
        // Act
        var result = new AggregationResult
        {
            GroupBy = "Office A",
            Count = 10,
            AverageEnergyConsumption = 150.5m,
            AverageCo2 = 400.0m,
            AveragePm25 = 25.0m,
            AverageHumidity = 60.0m,
            TotalEnergyConsumption = 1505.0m
        };

        // Assert
        result.GroupBy.Should().Be("Office A");
        result.Count.Should().Be(10);
        result.AverageEnergyConsumption.Should().Be(150.5m);
        result.AverageCo2.Should().Be(400.0m);
        result.AveragePm25.Should().Be(25.0m);
        result.AverageHumidity.Should().Be(60.0m);
        result.TotalEnergyConsumption.Should().Be(1505.0m);
    }

    [Fact]
    public void AggregationResult_WithNullValues_ShouldAllowNull()
    {
        // Act
        var result = new AggregationResult
        {
            GroupBy = "Test",
            Count = 5
        };

        // Assert
        result.GroupBy.Should().Be("Test");
        result.Count.Should().Be(5);
        result.AverageEnergyConsumption.Should().BeNull();
        result.AverageCo2.Should().BeNull();
        result.AveragePm25.Should().BeNull();
        result.AverageHumidity.Should().BeNull();
        result.TotalEnergyConsumption.Should().BeNull();
    }
}

