using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace PC.RigaElevatorStatusDownloader.FunctionApp.Models;

internal record class SourceResponse
{
    public required ImmutableArray<JObject> Features { get; init; }
}
