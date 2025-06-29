using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;
using VerticalShop.Api.Tests;

[assembly: AssemblyFixture(typeof(DatabaseFixture))]
namespace VerticalShop.Api.Tests;

/// <summary>
/// Provides a fixture for managing the lifecycle of a PostgreSQL container to be used in integration tests.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    private static string DatabaseName => $"cdd_{TestContext.Current.TestClass?.TestClassSimpleName.ToLower() ?? "container"}";
    
    public string GetConnectionString()
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = DatabaseName
        };
        
        return connectionStringBuilder.ToString();
    }

    public async Task EnsureCreatedAsync()
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        try { await connection.ExecuteAsync($"create database {DatabaseName}"); }
        catch (PostgresException) { }
    }
    
    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
