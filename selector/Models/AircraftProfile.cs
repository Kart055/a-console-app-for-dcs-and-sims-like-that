using System.Text.Json.Serialization;

namespace DcsFlightCalculator.Models;

public class AircraftProfile
{
    [JsonIgnore]
    public string FilePath { get; set; } = "";

    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public FuelProfile Fuel { get; set; } = new();

    public EngineProfile Engine { get; set; } = new();

    public PerformanceProfile Performance { get; set; } = new();


}