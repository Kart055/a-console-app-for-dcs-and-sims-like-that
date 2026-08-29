namespace DcsFlightCalculator.Models;

public class FuelFlowPoint
{
    /// ircraft pressure/geometric altitude represented by the source data
    /// unit: meters
    public double AltitudeMeters { get; set; }

    /// trvke airspeed represented by the source data
    ///
    /// unit: km/h
    public double TrueAirspeedKmh { get; set; }
    /// engine power mode associated with this data point
    public EnginePowerMode PowerMode { get; set; }

    /// total aircraft fuel flow
    ///
    /// Unit: kg/hour
    ///
    /// !!!IMPORTANT!!!:
    /// This is aircraft-total flow, not per-engine flow
    public double FuelFlowKgPerHour { get; set; }

    /// original source value, when supplied in lb/hour.
    /// this is retained for traceability
    public double? FuelFlowLbPerHour { get; set; }

    /// indicates that this point is estimated
    public bool IsEstimate { get; set; }

    /// source or methodology used to create this point
    public string Source { get; set; } = "";


}