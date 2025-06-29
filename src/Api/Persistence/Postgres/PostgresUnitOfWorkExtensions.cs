using System.Data.Common;

namespace ContextDrivenDevelopment.Api.Persistence.Postgres;

public static class PostgresUnitOfWorkExtensions
{
    public static Task<List<dynamic>> QueryListAsync(
        this DbConnection connection,
        string sql,
        object? parameters,
        DbTransaction? transaction = null)
        => connection.QueryListAsync<dynamic>(sql, parameters, transaction);
    
    public static async Task<List<T>> QueryListAsync<T>(
        this DbConnection connection,
        string sql,
        object? parameters,
        DbTransaction? transaction = null)
    {
        var results = await connection.QueryAsync<T>(sql, parameters, transaction);
        return results.ToList();
    }
}
