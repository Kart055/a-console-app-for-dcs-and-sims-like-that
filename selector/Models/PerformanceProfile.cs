namespace DcsFlightCalculator.Models;

public class PerformanceProfile
{
    /// maximum/reference aircraft speed
    /// unit: km/h
    public double MaximumSpeedKmh { get; set; }

    /// referenced service ceiling
    /// unit: meters
    public double ServiceCeilingMeters { get; set; }

    /// published/referenced combat radius
    /// Unit: km
    public double CombatRadiusKm { get; set; }

    /// published/referenced ferry range
    /// Unit: km
    public double FerryRangeKm { get; set; }

    /// optional TAS/altitude/power fuel-flow data
    ///
    /// these points may be measured, sourced or estimated
    /// the calculator should identify their truthfulness/origin rather
    /// than treating every point as ground truth
    public List<FuelFlowPoint> FuelFlow { get; set; } = new();

    /// optional general performance-data source
    public string Source { get; set; } = "";


}