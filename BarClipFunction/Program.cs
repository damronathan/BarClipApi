using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BarClip.Core;


var builder = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        configuration.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.RegisterFunctionServices();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    });

var app = builder.Build();

await app.RunAsync();
