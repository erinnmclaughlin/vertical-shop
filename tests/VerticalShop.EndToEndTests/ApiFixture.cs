using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace VerticalShop.EndToEndTests;

public sealed class ApiFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("KeepPostgresContainerAlive", "false");
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        
        _app = await builder.BuildAsync();
        await _app.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
    
    public HttpClient CreateHttpClient(string name) => _app!.CreateHttpClient(name);
}
