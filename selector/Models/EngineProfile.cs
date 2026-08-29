namespace DcsFlightCalculator.Models;

public class EngineProfile
{
    /// engine model designation
    public string Model { get; set; } = "";

    /// number of engines installed
    public int EngineCount { get; set; }

    /// reference engine operating points
    public List<EnginePowerSetting> PowerSettings { get; set; } = new();

    /// gets the reference data for a particular engine power mode
    public EnginePowerSetting? GetPowerSetting(
        EnginePowerMode powerMode)
    {
        return PowerSettings.FirstOrDefault(
            x => x.PowerMode == powerMode);
    }


}