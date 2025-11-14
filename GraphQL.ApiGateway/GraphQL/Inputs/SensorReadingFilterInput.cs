using GraphQL.ApiGateway.Models;
using HotChocolate.Data.Filters;

namespace GraphQL.ApiGateway.GraphQL.Inputs;

public class SensorReadingFilterInput : FilterInputType<SensorReading>
{
    protected override void Configure(IFilterInputTypeDescriptor<SensorReading> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor
            .Field(f => f.SensorId)
            .Description("Filter by sensor ID");

        descriptor
            .Field(f => f.Type)
            .Description("Filter by sensor type");

        descriptor
            .Field(f => f.Location)
            .Description("Filter by location");

        descriptor
            .Field(f => f.Timestamp)
            .Description("Filter by timestamp range");

        descriptor
            .Field(f => f.EnergyConsumption)
            .Description("Filter by energy consumption value");

        descriptor
            .Field(f => f.Co2)
            .Description("Filter by CO2 level");

        descriptor
            .Field(f => f.Pm25)
            .Description("Filter by PM2.5 level");

        descriptor
            .Field(f => f.Humidity)
            .Description("Filter by humidity level");

        descriptor
            .Field(f => f.MotionDetected)
            .Description("Filter by motion detection status");
    }
}

