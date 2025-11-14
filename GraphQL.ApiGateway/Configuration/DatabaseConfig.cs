namespace GraphQL.ApiGateway.Configuration;

public class DatabaseConfig
{
    public string Host { get; set; } = "host.docker.internal";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "sensordata";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";
    public string ConnectionString => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
}

