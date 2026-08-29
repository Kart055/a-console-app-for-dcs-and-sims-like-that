namespace DcsFlightCalculator.Models;

public enum FuelFlowCalculationSource
{
    None,

    ExactFuelFlowPoint,

    TasInterpolation,

    AltitudeInterpolation,

    BilinearInterpolation,

    ClampedFuelFlowPoint,

    EngineReferenceFallback


}