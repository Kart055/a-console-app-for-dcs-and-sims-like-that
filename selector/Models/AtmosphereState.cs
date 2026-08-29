namespace DcsFlightCalculator.Models;

public class AtmosphereState
{
    /// Geometric altitude
    /// Unit: meters
    public double AltitudeMeters { get; set; }

    /// Temperature
    /// Unit: Kelvin
    public double TemperatureKelvin { get; set; }

    /// Static pressure
    /// Unit: Pa
    public double PressurePa { get; set; }

    /// Air density
    /// Unit: kg/m^3
    public double DensityKgPerM3 { get; set; }

    /// Speed of sound
    /// Unit: m/s
    public double SpeedOfSoundMps { get; set; }

    /// Speed of sound
    /// unit: km/h
    public double SpeedOfSoundKmh { get; set; }

    /// temperature in degrees Celsius
    public double TemperatureCelsius =>
        TemperatureKelvin - 273.15;


}