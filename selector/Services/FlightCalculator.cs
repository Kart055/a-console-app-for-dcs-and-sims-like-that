using DcsFlightCalculator.Models;

namespace DcsFlightCalculator.Services;

public class FlightCalculator
{
    /// <summary>
    /// calculates flight endurance and range using:
    ///
    ///     Altitude + TAS + Engine Power Mode
    ///
    /// fuel flow data from Performance.FuelFlow is the preferred
    /// calculation source
    ///
    /// if no TAS/altitude fuel-flow data exists, the calculator
    /// falls back to EnginePowerSetting. The fallback is explicitly
    /// marked as an estimate because it does not model TAS-dependent
    /// aircraft drag/thrust requirements
    ///
    /// wind info:
    ///
    ///     positive wind = headwind
    ///     negative wind = tailwind
    ///
    /// all fuel flow values are TOTAL aircraft fuel flow
    /// </summary>
    public FlightCalculationResult Calculate(
        AircraftProfile aircraft,
        double fuel,
        double altitude,
        double wind,
        double trueAirspeed,
        EnginePowerMode powerMode)
    {
        ArgumentNullException.ThrowIfNull(aircraft);

        if (fuel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fuel),
                "Fuel cannot be negative.");
        }

        if (altitude < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(altitude),
                "Altitude cannot be negative.");
        }

        if (trueAirspeed <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(trueAirspeed),
                "True airspeed must be greater than zero.");
        }

        /*
         TAS / ALTITUDE FUEL FLOW
         */

        FuelFlowCalculation fuelFlowCalculation =
            CalculateFuelFlow(
                aircraft,
                altitude,
                trueAirspeed,
                powerMode);

        double fuelFlow =
            fuelFlowCalculation.FuelFlowKgPerHour;

        /*
         ENGINE REFERENCE DATA
         */

        EnginePowerSetting? powerSetting =
            aircraft.Engine.GetPowerSetting(
                powerMode);

        /*
         * GROUND SPEED (not in imperial #LOL)
         *
         * positive wind = headwind
         *
         * TAS 800 + headwind 100
         * GS  = 700 km/h
         *
         * TAS 800 + tailwind -100
         * GS  = 900 km/h
         */

        double groundSpeed =
            trueAirspeed - wind;

        if (groundSpeed < 0)
        {
            groundSpeed = 0;
        }

        /*
         ENDURANCE
         */

        double enduranceHours =
            fuelFlow > 0
                ? fuel / fuelFlow
                : 0;

        /*
         RANGE
         */

        double rangeKm =
            groundSpeed > 0
                ? groundSpeed * enduranceHours
                : 0;

        /*
         FUEL PER 100 KM
         */

        double fuelPer100Km =
            groundSpeed > 0 && fuelFlow > 0
                ? fuelFlow / groundSpeed * 100.0
                : 0;

        return new FlightCalculationResult
        {
            Aircraft = aircraft,

            Fuel = fuel,

            Altitude = altitude,

            Wind = wind,

            TrueAirspeed = trueAirspeed,

            PowerMode = powerMode,

            FuelFlow = fuelFlow,

            EnduranceHours = enduranceHours,

            GroundSpeed = groundSpeed,

            RangeKm = rangeKm,

            FuelPer100Km = fuelPer100Km,

            DataAvailable = fuelFlow > 0,

            PowerSetting = powerSetting,

            FuelFlowSource =
                fuelFlowCalculation.Source,

            FuelFlowPoint =
                fuelFlowCalculation.ReferencePoint,

            FuelFlowIsEstimate =
                fuelFlowCalculation.IsEstimate
        };
    }


    // FUEL FLOW

    /// <summary>:
    /// calculates fuel flow from the aircraft's TAS/altitude database
    ///
    /// Priority:
    ///
    ///     1. exact TAS + altitude point
    ///     2. tAS interpolation at an altitude
    ///     3. altitude interpolation between two levels
    ///     4. clamping at the edge of the supplied data envelope
    ///     5. engine TSFC/direct-flow fallback
    ///
    /// !!!IMPORTANT!!!:
    ///
    /// the fallback does NOT make fuel flow TAS accurate
    /// TAS accuracy requires actual TAS/altitude fuel-flow data
    private static FuelFlowCalculation CalculateFuelFlow(
        AircraftProfile aircraft,
        double altitude,
        double trueAirspeed,
        EnginePowerMode powerMode)
    {
        List<FuelFlowPoint> points =
            aircraft.Performance.FuelFlow
                .Where(x =>
                    x.PowerMode == powerMode &&
                    x.FuelFlowKgPerHour > 0)
                .OrderBy(x => x.AltitudeMeters)
                .ThenBy(x => x.TrueAirspeedKmh)
                .ToList();

        /*
         nbo TAS-specific data
         */

        if (points.Count == 0)
        {
            double fallback =
                CalculateFallbackFuelFlow(
                    aircraft,
                    powerMode);

            EnginePowerSetting? setting =
                aircraft.Engine.GetPowerSetting(
                    powerMode);

            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour = fallback,

                ReferencePoint = null,

                IsEstimate =
                    setting?.IsEstimate ?? true,

                Source =
                    setting != null
                        ? "ENGINE POWER / TSFC FALLBACK — NOT TAS DEPENDENT"
                        : "NO FUEL-FLOW DATA AVAILABLE"
            };
        }

        /*
         exact point
         */

        FuelFlowPoint? exact =
            points.FirstOrDefault(x =>
                NearlyEqual(
                    x.AltitudeMeters,
                    altitude) &&
                NearlyEqual(
                    x.TrueAirspeedKmh,
                    trueAirspeed));

        if (exact != null)
        {
            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour =
                    exact.FuelFlowKgPerHour,

                ReferencePoint = exact,

                IsEstimate = exact.IsEstimate,

                Source =
                    "EXACT TAS / ALTITUDE DATA"
            };
        }

        /*
         available altitude levels
         */

        List<double> altitudes =
            points
                .Select(x => x.AltitudeMeters)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

        if (altitudes.Count == 0)
        {
            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour = 0,
                ReferencePoint = null,
                IsEstimate = true,
                Source = "NO VALID FUEL-FLOW DATA"
            };
        }

        /*
         below the lowest altitude
         clamp to lowest altitude
         */

        if (altitude <= altitudes[0])
        {
            FuelFlowInterpolationResult result =
                InterpolateAtAltitude(
                    points,
                    altitudes[0],
                    trueAirspeed);

            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour =
                    result.FuelFlowKgPerHour,

                ReferencePoint =
                    result.ReferencePoint,

                IsEstimate =
                    result.IsEstimate,

                Source =
                    "TAS INTERPOLATION AT LOWEST AVAILABLE ALTITUDE"
            };
        }

        /*
         above the highest altitude - clamp to highest altitude
         */

        if (altitude >= altitudes[^1])
        {
            FuelFlowInterpolationResult result =
                InterpolateAtAltitude(
                    points,
                    altitudes[^1],
                    trueAirspeed);

            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour =
                    result.FuelFlowKgPerHour,

                ReferencePoint =
                    result.ReferencePoint,

                IsEstimate =
                    result.IsEstimate,

                Source =
                    "TAS INTERPOLATION AT HIGHEST AVAILABLE ALTITUDE"
            };
        }

        /*
         find surrounding altitude levels
         */

        double lowerAltitude =
            altitudes
                .Where(x => x <= altitude)
                .Max();

        double upperAltitude =
            altitudes
                .Where(x => x >= altitude)
                .Min();

        /*
         fuel flow at lower altitude
         */

        FuelFlowInterpolationResult lower =
            InterpolateAtAltitude(
                points,
                lowerAltitude,
                trueAirspeed);

        /*
         fuel flow at upper altitude
         */

        FuelFlowInterpolationResult upper =
            InterpolateAtAltitude(
                points,
                upperAltitude,
                trueAirspeed);

        /*
         same altitude
         */

        if (NearlyEqual(
                lowerAltitude,
                upperAltitude))
        {
            return new FuelFlowCalculation
            {
                FuelFlowKgPerHour =
                    lower.FuelFlowKgPerHour,

                ReferencePoint =
                    lower.ReferencePoint,

                IsEstimate =
                    lower.IsEstimate,

                Source =
                    "TAS INTERPOLATED DATA"
            };
        }

        /*
         Interpolate vertically between altitude levels
         */

        double altitudeFraction =
            (altitude - lowerAltitude) /
            (upperAltitude - lowerAltitude);

        double fuelFlow =
            Lerp(
                lower.FuelFlowKgPerHour,
                upper.FuelFlowKgPerHour,
                altitudeFraction);

        return new FuelFlowCalculation
        {
            FuelFlowKgPerHour = fuelFlow,

            /*
             There isn't a single exact reference point after two-dimensional interpolation
             */
            ReferencePoint =
                SelectNearestReferencePoint(
                    points,
                    altitude,
                    trueAirspeed),

            IsEstimate =
                lower.IsEstimate ||
                upper.IsEstimate,

            Source =
                "BILINEAR / ALTITUDE + TAS INTERPOLATION"
        };
    }

    // TAS INTERPOLATION

    private static FuelFlowInterpolationResult
        InterpolateAtAltitude(
            List<FuelFlowPoint> points,
            double altitude,
            double trueAirspeed)
    {
        List<FuelFlowPoint> altitudePoints =
            points
                .Where(x =>
                    NearlyEqual(
                        x.AltitudeMeters,
                        altitude))
                .OrderBy(x => x.TrueAirspeedKmh)
                .ToList();

        /*
         no exact altitude
         this should normally not occur because the caller passes an altitude taken from the available altitude list
         */
        if (altitudePoints.Count == 0)
        {
            FuelFlowPoint nearest =
                points
                    .OrderBy(x =>
                        Math.Abs(
                            x.AltitudeMeters -
                            altitude))
                    .ThenBy(x =>
                        Math.Abs(
                            x.TrueAirspeedKmh -
                            trueAirspeed))
                    .First();

            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    nearest.FuelFlowKgPerHour,

                ReferencePoint = nearest,

                IsEstimate =
                    nearest.IsEstimate
            };
        }

        /*
         exact TAS
         */

        FuelFlowPoint? exact =
            altitudePoints.FirstOrDefault(x =>
                NearlyEqual(
                    x.TrueAirspeedKmh,
                    trueAirspeed));

        if (exact != null)
        {
            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    exact.FuelFlowKgPerHour,

                ReferencePoint = exact,

                IsEstimate =
                    exact.IsEstimate
            };
        }

        /*
         only one TAS point
         */

        if (altitudePoints.Count == 1)
        {
            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    altitudePoints[0]
                        .FuelFlowKgPerHour,

                ReferencePoint =
                    altitudePoints[0],

                IsEstimate =
                    altitudePoints[0].IsEstimate
            };
        }

        /*
         below minimum TAS clamp
         */

        if (trueAirspeed <=
            altitudePoints[0].TrueAirspeedKmh)
        {
            FuelFlowPoint point =
                altitudePoints[0];

            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    point.FuelFlowKgPerHour,

                ReferencePoint = point,

                IsEstimate = point.IsEstimate
            };
        }

        /*
         Above maximum TAS.
         clamp rather than extrapolate
         */

        if (trueAirspeed >=
            altitudePoints[^1].TrueAirspeedKmh)
        {
            FuelFlowPoint point =
                altitudePoints[^1];

            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    point.FuelFlowKgPerHour,

                ReferencePoint = point,

                IsEstimate = point.IsEstimate
            };
        }

        /*
         find surrounding TAS points
         */

        FuelFlowPoint lower =
            altitudePoints
                .Where(x =>
                    x.TrueAirspeedKmh <= trueAirspeed)
                .OrderByDescending(
                    x => x.TrueAirspeedKmh)
                .First();

        FuelFlowPoint upper =
            altitudePoints
                .Where(x =>
                    x.TrueAirspeedKmh >= trueAirspeed)
                .OrderBy(
                    x => x.TrueAirspeedKmh)
                .First();

        if (NearlyEqual(
                lower.TrueAirspeedKmh,
                upper.TrueAirspeedKmh))
        {
            return new FuelFlowInterpolationResult
            {
                FuelFlowKgPerHour =
                    lower.FuelFlowKgPerHour,

                ReferencePoint = lower,

                IsEstimate =
                    lower.IsEstimate
            };
        }

        /*
         linear interpolation in TAS
         */

        double fraction =
            (trueAirspeed -
             lower.TrueAirspeedKmh) /
            (upper.TrueAirspeedKmh -
             lower.TrueAirspeedKmh);

        double fuelFlow =
            Lerp(
                lower.FuelFlowKgPerHour,
                upper.FuelFlowKgPerHour,
                fraction);

        return new FuelFlowInterpolationResult
        {
            FuelFlowKgPerHour = fuelFlow,

            /*
             use the closer point as the displayed reference
             */
            ReferencePoint =
                Math.Abs(
                    trueAirspeed -
                    lower.TrueAirspeedKmh)
                <=
                Math.Abs(
                    upper.TrueAirspeedKmh -
                    trueAirspeed)
                    ? lower
                    : upper,

            IsEstimate =
                lower.IsEstimate ||
                upper.IsEstimate
        };
    }

    // FALLBACK

    private static double CalculateFallbackFuelFlow(
        AircraftProfile aircraft,
        EnginePowerMode powerMode)
    {
        EnginePowerSetting? setting =
            aircraft.Engine.GetPowerSetting(
                powerMode);

        if (setting == null)
        {
            return 0;
        }

        return setting.CalculateTotalFuelFlow(
            aircraft.Engine.EngineCount);
    }

    // REFERENCE POINT

    private static FuelFlowPoint?
        SelectNearestReferencePoint(
            List<FuelFlowPoint> points,
            double altitude,
            double trueAirspeed)
    {
        return points
            .OrderBy(x =>
                Math.Abs(
                    x.AltitudeMeters -
                    altitude))
            .ThenBy(x =>
                Math.Abs(
                    x.TrueAirspeedKmh -
                    trueAirspeed))
            .FirstOrDefault();
    }

    // MATH

    private static double Lerp(
        double first,
        double second,
        double fraction)
    {
        return first +
               (second - first) *
               fraction;
    }

    private static bool NearlyEqual(
        double first,
        double second,
        double tolerance = 0.001)
    {
        return Math.Abs(
                   first - second)
               <= tolerance;
    }
}


// CALCULATION RESULT

public class FlightCalculationResult
{
    public AircraftProfile Aircraft { get; set; } =
        new();

    public double Fuel { get; set; }

    public double Altitude { get; set; }

    public double Wind { get; set; }

    public double TrueAirspeed { get; set; }

    public EnginePowerMode PowerMode { get; set; }

    public double FuelFlow { get; set; }

    public double EnduranceHours { get; set; }

    public double GroundSpeed { get; set; }

    public double RangeKm { get; set; }

    public double FuelPer100Km { get; set; }

    public bool DataAvailable { get; set; }

    /*
     engine reference data
     */
    public EnginePowerSetting? PowerSetting { get; set; }

    /*
     description of how fuel flow was obtained
     */
    public string FuelFlowSource { get; set; } = "";

    /*
     closest source point used for UI/reference display
     */
    public FuelFlowPoint? FuelFlowPoint { get; set; }

    /*
     true if the selected/interpolated fuel-flow data contains estimated values
     */
    public bool FuelFlowIsEstimate { get; set; }
}


// INTERNAL CALCULATION TYPES

internal class FuelFlowCalculation
{
    public double FuelFlowKgPerHour { get; set; }

    public FuelFlowPoint? ReferencePoint { get; set; }

    public bool IsEstimate { get; set; }

    public string Source { get; set; } = "";
}


internal class FuelFlowInterpolationResult
{
    public double FuelFlowKgPerHour { get; set; }

    public FuelFlowPoint? ReferencePoint { get; set; }

    public bool IsEstimate { get; set; }
}
