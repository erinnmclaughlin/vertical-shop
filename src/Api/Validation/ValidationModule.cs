using System.Reflection;

namespace VerticalShop.Api.Validation;

/// <summary>
/// Provides methods to register validation services for the application.
/// </summary>
public static class ValidationModule
{
    /// <summary>
    /// Registers validation services by adding validators from the specified assemblies
    /// to the application service collection.
    /// </summary>
    /// <param name="builder">The instance of <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    /// <param name="assemblies">An array of <see cref="System.Reflection.Assembly"/> objects from which validators will be scanned and added.</param>
    public static void AddValidation(this WebApplicationBuilder builder, Assembly[] assemblies)
    {
        builder.Services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);
    }   
}
