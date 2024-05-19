using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PC.RigaElevatorStatusDownloader.FunctionApp.Models;
using System.Net;

namespace PC.RigaElevatorStatusDownloader.FunctionApp;

public class Download(Container container, HttpClient httpClient, ILogger<Download> logger)
{
    private readonly Container _container = container;
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<Download> _logger = logger;

    [Function(nameof(Download))]
    [FixedDelayRetry(23, "01:00:00")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * *"
#if DEBUG
        , RunOnStartup = true
#endif
        )] TimerInfo timerInfo)
    {
        string sourceResponseJson = await _httpClient.GetStringAsync("https://services-eu1.arcgis.com/JzVauvrMa89dBJ9D/arcgis/rest/services/Lifti_uztureshana/FeatureServer/0/query?f=json&where=1%3D1&returnGeometry=true&spatialRel=esriSpatialRelIntersects&outFields=OBJECTID%2CSt%C4%81voklis%2CNosaukums%2CAdrese&orderByFields=OBJECTID%20ASC&outSR=102100");
        SourceResponse sourceResponse = JsonConvert.DeserializeObject<SourceResponse>(sourceResponseJson);
        DateTime dateTimeNow = DateTime.Now;
        foreach (JObject details in sourceResponse.Features)
        {
            ElevatorStatus elevatorStatus = new()
            {
                Date = DateOnly.FromDateTime(dateTimeNow),
                Details = details,
                Time = TimeOnly.FromDateTime(dateTimeNow)
            };
            _logger.LogInformation("Creating item {ElevatorStatus}.", elevatorStatus);
            try
            {
                _ = await _container.CreateItemAsync(elevatorStatus);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogInformation("Item already exists.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create item.");
            }
        }
    }
}
