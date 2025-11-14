using GraphQL.ApiGateway.Models;

namespace GraphQL.ApiGateway.GraphQL.Types;

public class SensorReadingType : ObjectType<SensorReading>
{
    protected override void Configure(IObjectTypeDescriptor<SensorReading> descriptor)
    {
        descriptor
            .Field(f => f.Id)
            .Description("Unique identifier of the sensor reading");

        descriptor
            .Field(f => f.SensorId)
            .Description("Identifier of the sensor");

        descriptor
            .Field(f => f.Type)
            .Description("Type of the sensor (e.g., energy, air_quality, motion)");

        descriptor
            .Field(f => f.Location)
            .Description("Location where the sensor reading was taken");

        descriptor
            .Field(f => f.Timestamp)
            .Description("Timestamp when the reading was recorded");

        descriptor
            .Field(f => f.EnergyConsumption)
            .Description("Energy consumption value (if applicable)");

        descriptor
            .Field(f => f.Co2)
            .Description("CO2 level in parts per million (if applicable)");

        descriptor
            .Field(f => f.Pm25)
            .Description("PM2.5 particulate matter level (if applicable)");

        descriptor
            .Field(f => f.Humidity)
            .Description("Humidity percentage (if applicable)");

        descriptor
            .Field(f => f.MotionDetected)
            .Description("Whether motion was detected (if applicable)");
    }
}

