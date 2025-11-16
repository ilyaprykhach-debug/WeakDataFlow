namespace GraphQL.ApiGateway.GraphQL.Inputs;

public class PaginationInput : InputObjectType<PaginationInputData>
{
    protected override void Configure(IInputObjectTypeDescriptor<PaginationInputData> descriptor)
    {
        descriptor
            .Field(f => f.Skip)
            .Description("Number of items to skip")
            .Type<IntType>()
            .DefaultValue(0);

        descriptor
            .Field(f => f.Take)
            .Description("Number of items to take")
            .Type<IntType>()
            .DefaultValue(10);
    }
}

public class PaginationInputData
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

