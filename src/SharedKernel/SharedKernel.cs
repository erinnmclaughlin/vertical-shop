using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VerticalShop;

/// <summary>
/// A marker class to identify the assembly.
/// </summary>
public static class SharedKernel
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDateProvider, SystemDateProvider>();
        services.TryAddTransient<INpgsqlDataStore, NpgsqlDataStore>();
    }
}
