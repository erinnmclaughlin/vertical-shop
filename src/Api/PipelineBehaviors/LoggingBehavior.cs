using MediatR;

namespace VerticalShop.Api;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {RequestName} request. Request: {@RequestContent}", typeof(TRequest).Name, request);
        var result = await next(cancellationToken);
        logger.LogInformation("Finished handling {RequestName} request. Response: {@RequestContent}", typeof(TRequest).Name, result);
        return result;
    }
}
