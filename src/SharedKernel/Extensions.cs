using System.Text.Json;
using Dapper;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace VerticalShop;

public static class Extensions
{
    public static void AddMassTransit<T>(this T builder, Action<IBusRegistrationConfigurator>? configure = null) where T : IHostApplicationBuilder
    {
        builder.Services.AddMassTransit(options =>
        {
            options.UsingPostgres((context,o) => o.ConfigureEndpoints(context));
            configure?.Invoke(options);
        });
        
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("vertical-shop-db");
            options.Schema = "transport";
        });
    }

    public static bool IsUniqueConstraintViolationOnColumn(this PostgresException ex, string columnName) =>
        ex.ErrorCode.ToString() == PostgresErrorCodes.UniqueViolation &&
        ex.ColumnName == columnName;

    public static async Task InsertOutboxMessageAsync<T>(
        this NpgsqlConnection connection,
        T message, 
        NpgsqlTransaction transaction, 
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        await connection.ExecuteAsync(
            "insert into outbox_messages(id, type, payload) values (@id, @type, @payload::jsonb)", 
            new
            {
                id = Guid.CreateVersion7(),
                type = typeof(T).FullName,
                payload = JsonSerializer.Serialize(message)
            },
            transaction
        );
    }
}
