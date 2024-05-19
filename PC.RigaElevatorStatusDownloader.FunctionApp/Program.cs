using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        _ = services.AddApplicationInsightsTelemetryWorkerService();
        _ = services.ConfigureFunctionsApplicationInsights();
        _ = services.AddSingleton<HttpClient>();
        _ = services.AddSingleton(serviceProvider =>
        {
            string endpoint = GetConfigurationValue(context, "COSMOS_DB_ENDPOINT");
            DefaultAzureCredential credential = new();
            CosmosClient cosmosClient = new(endpoint, credential);
            string databaseId = GetConfigurationValue(context, "COSMOS_DB_DATABASE");
            Container container = cosmosClient.GetContainer(databaseId, "ElevatorStatuses");
            return container;
        });
    })
    .Build();
host.Run();

static string GetConfigurationValue(HostBuilderContext context, string key)
{
    string value = context.Configuration[key];
    return value ?? throw new KeyNotFoundException($"Environment variable {key} is not set.");
}
