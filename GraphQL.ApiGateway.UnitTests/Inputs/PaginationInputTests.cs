using FluentAssertions;
using GraphQL.ApiGateway.GraphQL.Inputs;

namespace GraphQL.ApiGateway.UnitTests.Inputs;

public class PaginationInputTests
{
    [Fact]
    public void PaginationInputData_DefaultValues_ShouldBeSet()
    {
        // Act
        var pagination = new PaginationInputData();

        // Assert
        pagination.Skip.Should().Be(0);
        pagination.Take.Should().Be(10);
    }

    [Fact]
    public void PaginationInputData_Properties_ShouldBeSettable()
    {
        // Act
        var pagination = new PaginationInputData
        {
            Skip = 20,
            Take = 50
        };

        // Assert
        pagination.Skip.Should().Be(20);
        pagination.Take.Should().Be(50);
    }

    [Fact]
    public void PaginationInputData_WithZeroTake_ShouldAllowZero()
    {
        // Act
        var pagination = new PaginationInputData
        {
            Skip = 0,
            Take = 0
        };

        // Assert
        pagination.Skip.Should().Be(0);
        pagination.Take.Should().Be(0);
    }

    [Fact]
    public void PaginationInputData_WithNegativeValues_ShouldAccept()
    {
        // Act
        var pagination = new PaginationInputData
        {
            Skip = -10,
            Take = -5
        };

        // Assert
        pagination.Skip.Should().Be(-10);
        pagination.Take.Should().Be(-5);
    }
}

