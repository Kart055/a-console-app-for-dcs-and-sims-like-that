using System.Text.Json.Serialization;

namespace DcsFlightCalculator.Models;

public class FuelProfile
{
    /// internal fuel capacity
    /// unit: kg
    public double InternalFuelKg { get; set; }

    /// external/drop-tank fuel capacity
    /// unit: kg
    public double ExternalFuelKg { get; set; }

    /// total usable fuel represented by this profile
    [JsonIgnore]
    public double MaximumFuelKg =>
        Math.Max(0, InternalFuelKg) +
        Math.Max(0, ExternalFuelKg);


}