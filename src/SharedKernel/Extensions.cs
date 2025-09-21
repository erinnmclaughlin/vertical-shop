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
        ex.SqlState == PostgresErrorCodes.UniqueViolation &&
        ex.ColumnName == columnName;
}
