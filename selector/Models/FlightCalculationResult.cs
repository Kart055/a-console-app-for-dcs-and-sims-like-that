namespace DcsFlightCalculator.Models;

public class FlightCalculationResult
{
    public AircraftProfile Aircraft { get; set; } = new();

    public double Fuel { get; set; }

    public double Altitude { get; set; }

    public double Wind { get; set; }

    public double TrueAirspeed { get; set; }

    public EnginePowerMode PowerMode { get; set; }

    /*
     * Total aircraft fuel flow unit: kg/hour
     */
    public double FuelFlow { get; set; }

    /*
     * Endurance
     *
     * Unit: hours
     */
    public double EnduranceHours { get; set; }

    /*
     * Ground speed
     *
     * Unit: km/h
     */
    public double GroundSpeed { get; set; }

    /*
     * Estimated range
     * Unit: km
     */
    public double RangeKm { get; set; }

    /*
     fuel consumption unit: kg/100 km
     */
    public double FuelPer100Km { get; set; }

    /*
     true when usable fuel-flow data was found
     */
    public bool DataAvailable { get; set; }

    /*
     engine reference information used by the UI.
     */
    public EnginePowerSetting? PowerSetting { get; set; }

    /*
     * indicates how the fuel-flow result was obtained.
     * examples:
     * exact database point
     * TAS interpolation
     * altitude interpolation
     * TAS + altitude interpolation
       engine TSFC fallback
     */
    public string FuelFlowSource { get; set; } = "";

    /*
     The actual database point used as the nearest/reference point for display.
     */
    public FuelFlowPoint? FuelFlowPoint { get; set; }
}