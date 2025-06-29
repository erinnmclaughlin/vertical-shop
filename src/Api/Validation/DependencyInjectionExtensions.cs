namespace ContextDrivenDevelopment.Api.Validation;

public static class DependencyInjectionExtensions
{
    public static void AddValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
    }   
}
