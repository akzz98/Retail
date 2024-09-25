using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Retail.Entities;
using Retail.Services;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                string connectionString = configuration.GetConnectionString("TableStorageConnection");
                string tableName = configuration.GetValue<string>("ConnectionStrings:CategoryTableName");

                services.AddSingleton(new CategoryStorageService(connectionString, tableName));
            });
}
