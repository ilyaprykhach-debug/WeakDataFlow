using GraphQL.ApiGateway.Configuration;
using GraphQL.ApiGateway.Data;
using GraphQL.ApiGateway.GraphQL.Inputs;
using GraphQL.ApiGateway.GraphQL.Queries;
using GraphQL.ApiGateway.GraphQL.Types;
using HotChocolate.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("Database"));
var databaseConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>() ?? new DatabaseConfig();

builder.Services.AddDbContext<SensorDataDbContext>(options =>
    options.UseNpgsql(databaseConfig.ConnectionString));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddTypeExtension<SensorReadingQueries>()
    .AddType<SensorReadingType>()
    .AddType<AggregationResultType>()
    .AddType<PaginationInput>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGraphQL("/graphql")
    .WithOptions(new GraphQLServerOptions
    {
        Tool = { Enable = true }
    });

app.MapControllers();

app.Run();
