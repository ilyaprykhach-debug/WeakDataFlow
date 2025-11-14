namespace GraphQL.ApiGateway.GraphQL.Types;

public class AggregationResultType : ObjectType<AggregationResult>
{
    protected override void Configure(IObjectTypeDescriptor<AggregationResult> descriptor)
    {
        descriptor
            .Field(f => f.GroupBy)
            .Description("The value used for grouping (e.g., location, type, or time period)");

        descriptor
            .Field(f => f.Count)
            .Description("Number of readings in this group");

        descriptor
            .Field(f => f.AverageEnergyConsumption)
            .Description("Average energy consumption for this group");

        descriptor
            .Field(f => f.AverageCo2)
            .Description("Average CO2 level for this group");

        descriptor
            .Field(f => f.AveragePm25)
            .Description("Average PM2.5 level for this group");

        descriptor
            .Field(f => f.AverageHumidity)
            .Description("Average humidity for this group");

        descriptor
            .Field(f => f.TotalEnergyConsumption)
            .Description("Total energy consumption for this group");
    }
}

public class AggregationResult
{
    public string GroupBy { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal? AverageEnergyConsumption { get; set; }
    public decimal? AverageCo2 { get; set; }
    public decimal? AveragePm25 { get; set; }
    public decimal? AverageHumidity { get; set; }
    public decimal? TotalEnergyConsumption { get; set; }
}

