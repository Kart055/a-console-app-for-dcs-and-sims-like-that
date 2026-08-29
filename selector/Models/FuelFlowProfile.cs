namespace DcsFlightCalculator.Models;

public class FuelFlowProfile
{
    public double BaselineKgPerHour { get; set; }

    public List<AltitudeFactor> AltitudeFactors { get; set; } =
        new();
}
