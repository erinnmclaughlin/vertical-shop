namespace ContextDrivenDevelopment.Api.Validation;

public static class DependencyInjectionExtensions
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
