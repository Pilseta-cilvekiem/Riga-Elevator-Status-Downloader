using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PC.RigaElevatorStatusDownloader.FunctionApp.Models;

internal record class ElevatorStatus
{
    public required DateOnly Date { get; init; }

    public required JObject Details { get; init; }

    [JsonProperty("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    public required TimeOnly Time { get; init; }
}
