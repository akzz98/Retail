using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // Read configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            .Build();

        // Register FileStorageService
        services.AddSingleton<FileStorageService>(sp =>
        {
            var connectionString = config.GetConnectionString("FileStorageConnection");
            var shareName = config["ConnectionStrings:EmployeeContractsFileShare"];
            var logger = sp.GetRequiredService<ILogger<FileStorageService>>();
            return new FileStorageService(connectionString, shareName, logger);
        });
    })
    .Build();

await host.RunAsync();
