namespace VerticalShop.Api.Validation;

/// <summary>
/// Provides methods to register validation services for the application.
/// </summary>
public static class ValidationModule
{
    /// <summary>
    /// Registers validation services by adding validators from the application's assembly.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the application's services.</param>
    public static void AddValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
    }   
}
