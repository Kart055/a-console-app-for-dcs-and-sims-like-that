namespace DcsFlightCalculator.Models;

public class EnginePowerSetting
{
    public EnginePowerMode PowerMode { get; set; }

    /*
     * Reference thrust for ONE engine.
     * unit: kgf
     */
    public double ReferenceThrustKgf { get; set; }

    /*
     * specific fuel consumption
     *
     * unit: kg / (kgf * hour)
     *
     * example:
     0.76 means 0.76 kg of fuel per hour for every kgf of thrust
     */
    public double? TsfcKgPerKgfHour { get; set; }

    /*
     * Direct fuel-flow value for ONE engine.
     unit: kg/hour
     This is useful for idle or other operating conditions where TSFC/thrust data is not sufficiently reliable
     */
    public double? DirectFuelFlowKgPerHour { get; set; }

    /*
     Indicates that this value is an estimate rather than a directly measured value
     */
    public bool IsEstimate { get; set; }

    /*
     Data source / methodology
     */
    public string Source { get; set; } = "";

    /*
     * Calculates total aircraft fuel flow
     *
     * DirectFuelFlowKgPerHour is PER ENGINE
     *
     * Therefore:
     * total flow = per-engine flow × engine count
     * if direct flow is unavailable, use thrust × TSFC × engine count
     */
    public double CalculateTotalFuelFlow(int engineCount)
    {
        if (engineCount <= 0)
        {
            return 0;
        }

        if (DirectFuelFlowKgPerHour.HasValue)
        {
            return DirectFuelFlowKgPerHour.Value *
                   engineCount;
        }

        if (!TsfcKgPerKgfHour.HasValue)
        {
            return 0;
        }

        if (ReferenceThrustKgf <= 0)
        {
            return 0;
        }

        return ReferenceThrustKgf *
               TsfcKgPerKgfHour.Value *
               engineCount;
    }
}