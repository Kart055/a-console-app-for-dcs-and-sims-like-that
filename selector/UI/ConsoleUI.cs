using System.Diagnostics;
using System.Globalization;
using System.Threading;
using DcsFlightCalculator.Models;
using DcsFlightCalculator.Services;
using ServiceFlightCalculationResult =
    DcsFlightCalculator.Services.FlightCalculationResult;

namespace DcsFlightCalculator;

public static class ConsoleUI
{
    private static readonly AircraftRepository Repository = new();
    private static readonly FlightCalculator Calculator = new();

    // DISPLAY ANIMATION

    /*
      result screen is intentionally printed progressively
     
      the target is approximately 2-3 seconds from the first result line appearing to the final "CALCULATION COMPLETE" message.
     
      adjust this value if desired:
     
      40 ms  = very fast
      60 ms  = fast
      70 ms  = approximately 2-3 seconds
      90 ms  = slower / more dramatic
     */
    private const int ResultPrintDelayMilliseconds = 200;

    /*
     *used for normal animated text anywhere in the UI
     *keep this small so menus/editors remain responsive
     */
    private const int NormalTextDelayMilliseconds = 5;

    // APPLICATION

    public static void Run()
    {
        Console.Title =
            "Flight Time & Range Calculator";

        while (true)
        {
            List<AircraftProfile> aircraft =
                Repository.LoadAircraftProfiles();

            if (aircraft.Count == 0)
            {
                Console.WriteLine(
                    "No aircraft profiles were found.");

                Console.WriteLine();

                Console.WriteLine(
                    $"Put .json files in: " +
                    $"{Repository.AircraftDirectory}");

                Pause();
                continue;
            }

            ConsoleGraphics.DrawTitle(
                "FLIGHT TIME & RANGE",
                "AIRCRAFT PERFORMANCE COMPUTER");

            Console.WriteLine();

            List<string> aircraftOptions =
                aircraft
                    .Select(x => x.Name)
                    .ToList();

            aircraftOptions.Add(
                "Reload aircraft database");

            aircraftOptions.Add(
                "Exit");

            int selected =
                ReadMenuChoice(
                    aircraftOptions,
                    allowEscape: false);

            if (selected == aircraft.Count)
            {
                continue;
            }

            if (selected == aircraft.Count + 1)
            {
                return;
            }

            AircraftMenu(
                aircraft[selected]);
        }
    }

    // AIRCRAFT MENU

    private static void AircraftMenu(
        AircraftProfile aircraft)
    {
        while (true)
        {
            Console.Clear();

            PrintAircraftSummary(aircraft);

            Console.WriteLine();

            List<string> options =
                new()
                {
                    "Calculate flight time & range",
                    "Edit aircraft profile",
                    "Open profile location",
                    "Back to aircraft selection"
                };

            int choice =
                ReadMenuChoice(options);

            switch (choice)
            {
                case 0:
                    CalculateFlight(aircraft);
                    break;

                case 1:
                    EditAircraftProfile(aircraft);
                    break;

                case 2:
                    OpenProfileLocation(aircraft);
                    break;

                case 3:
                    return;
            }
        }
    }

    // FLIGHT CALCULATION

    private static void CalculateFlight(
        AircraftProfile aircraft)
    {
        Console.Clear();

        ConsoleGraphics.DrawTitle(
            aircraft.Name.ToUpperInvariant(),
            "FLIGHT PERFORMANCE CALCULATION");

        ConsoleGraphics.DrawPanel(
            "FLIGHT PARAMETERS",
            72);

        double maximumFuel =
            aircraft.Fuel.MaximumFuelKg;

        Console.ForegroundColor =
            ConsoleGraphics.Secondary;

        Console.WriteLine(
            $"│ Fuel available: maximum {maximumFuel:N0} kg");

        ConsoleGraphics.ResetColor();

        double fuel =
            ReadDouble(
                "│ Fuel available [kg]: ",
                0,
                maximumFuel);

        double altitude =
            ReadDouble(
                "│ Altitude [m]: ",
                0,
                aircraft.Performance.ServiceCeilingMeters);

        double wind =
            ReadDouble(
                "│ Wind component [km/h] (+ head / - tail): ",
                -500,
                500);

        double trueAirspeed =
            ReadDouble(
                "│ True airspeed [km/h]: ",
                100,
                aircraft.Performance.MaximumSpeedKmh);

        Console.WriteLine(
            "└" + new string('─', 70));

        Console.WriteLine();

        ConsoleGraphics.DrawPanel(
            "ENGINE POWER",
            72);

        List<string> powerOptions =
            new()
            {
                "Idle",
                "Military power",
                "Afterburner"
            };

        int powerChoice =
            ReadMenuChoice(powerOptions);

        EnginePowerMode powerMode =
            powerChoice switch
            {
                0 => EnginePowerMode.Idle,
                1 => EnginePowerMode.Military,
                2 => EnginePowerMode.Afterburner,
                _ => EnginePowerMode.Military
            };

        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleGraphics.Warning;

        Console.WriteLine(
            $"  [ CALCULATING ] {powerMode}");

        ConsoleGraphics.ResetColor();

        Thread.Sleep(100);

        ServiceFlightCalculationResult result =
            Calculator.Calculate(
                aircraft,
                fuel,
                altitude,
                wind,
                trueAirspeed,
                powerMode);

        PrintResults(result);
    }

    // EDIT AIRCRAFT

    private static void EditAircraftProfile(
        AircraftProfile aircraft)
    {
        while (true)
        {
            Console.Clear();

            WriteAnimatedLine(
                "----------------------------------------------");

            WriteAnimatedLine(
                $"EDITING: {aircraft.Name}");

            WriteAnimatedLine(
                "----------------------------------------------");

            Console.WriteLine();

            List<string> options =
                new()
                {
                    $"Internal fuel:      " +
                    $"{aircraft.Fuel.InternalFuelKg:N0} kg",

                    $"External fuel:      " +
                    $"{aircraft.Fuel.ExternalFuelKg:N0} kg",

                    $"Engine:             " +
                    $"{aircraft.Engine.Model}",

                    "Edit fuel values",

                    "View engine data",

                    "View performance data",

                    "Edit fuel-flow data",

                    "Save changes",

                    "Reload profile",

                    "Return"
                };

            int choice =
                ReadMenuChoice(options);

            switch (choice)
            {
                case 0:
                    aircraft.Fuel.InternalFuelKg =
                        ReadDouble(
                            "Internal fuel [kg]: ",
                            0,
                            50000);
                    break;

                case 1:
                    aircraft.Fuel.ExternalFuelKg =
                        ReadDouble(
                            "External fuel [kg]: ",
                            0,
                            50000);
                    break;

                case 2:
                    Console.WriteLine();

                    Console.WriteLine(
                        $"Engine: {aircraft.Engine.Model}");

                    Pause();
                    break;

                case 3:
                    EditFuelValues(aircraft);
                    break;

                case 4:
                    PrintEngineData(aircraft);
                    break;

                case 5:
                    PrintPerformanceData(aircraft);
                    break;

                case 6:
                    EditFuelFlowData(aircraft);
                    break;

                case 7:
                    SaveAircraftProfile(aircraft);
                    break;

                case 8:
                    ReloadAircraftProfile(aircraft);
                    break;

                case 9:
                    return;
            }
        }
    }

    // FUEL EDITOR

    private static void EditFuelValues(
        AircraftProfile aircraft)
    {
        Console.Clear();

        WriteAnimatedLine(
            "----------------------------------------------");

        WriteAnimatedLine(
            "FUEL CONFIGURATION");

        WriteAnimatedLine(
            "----------------------------------------------");

        Console.WriteLine();

        WriteAnimatedLine(
            $"Internal fuel:      " +
            $"{aircraft.Fuel.InternalFuelKg:N0} kg");

        WriteAnimatedLine(
            $"External fuel:      " +
            $"{aircraft.Fuel.ExternalFuelKg:N0} kg");

        WriteAnimatedLine(
            $"Maximum fuel:       " +
            $"{aircraft.Fuel.MaximumFuelKg:N0} kg");

        Console.WriteLine();

        aircraft.Fuel.InternalFuelKg =
            ReadDouble(
                "Internal fuel [kg]: ",
                0,
                50000);

        aircraft.Fuel.ExternalFuelKg =
            ReadDouble(
                "External fuel [kg]: ",
                0,
                50000);

        Console.WriteLine();

        WriteAnimatedLine(
            "Fuel configuration updated.");

        Pause();
    }

    // ENGINE DATA

    private static void PrintEngineData(
        AircraftProfile aircraft)
    {
        Console.Clear();

        WriteAnimatedLine(
            "----------------------------------------------");

        WriteAnimatedLine(
            "ENGINE PERFORMANCE DATA");

        WriteAnimatedLine(
            "----------------------------------------------");

        Console.WriteLine();

        WriteAnimatedLine(
            $"Engine:             {aircraft.Engine.Model}");

        WriteAnimatedLine(
            $"Engines:            " +
            $"{aircraft.Engine.EngineCount}");

        Console.WriteLine();

        foreach (EnginePowerSetting setting
                 in aircraft.Engine.PowerSettings)
        {
            WriteAnimatedLine(
                $"Power mode:         {setting.PowerMode}");

            if (setting.ReferenceThrustKgf > 0)
            {
                WriteAnimatedLine(
                    $"Reference thrust:   " +
                    $"{setting.ReferenceThrustKgf:N0} kgf/engine");
            }

            if (setting.TsfcKgPerKgfHour.HasValue)
            {
                WriteAnimatedLine(
                    $"TSFC:               " +
                    $"{setting.TsfcKgPerKgfHour.Value:F3} " +
                    $"kg/(kgf·h)");
            }

            if (setting.DirectFuelFlowKgPerHour.HasValue)
            {
                WriteAnimatedLine(
                    $"Direct flow:        " +
                    $"{setting.DirectFuelFlowKgPerHour.Value:N0} " +
                    $"kg/h/engine");
            }

            WriteAnimatedLine(
                $"Estimated:          " +
                $"{(setting.IsEstimate ? "YES" : "NO")}");

            if (!string.IsNullOrWhiteSpace(setting.Source))
            {
                WriteAnimatedLine(
                    $"Source:             {setting.Source}");
            }

            Console.WriteLine();
        }

        Pause();
    }

    // PERFORMANCE DATA

    private static void PrintPerformanceData(
        AircraftProfile aircraft)
    {
        Console.Clear();

        WriteAnimatedLine(
            "----------------------------------------------");

        WriteAnimatedLine(
            "AIRCRAFT PERFORMANCE DATA");

        WriteAnimatedLine(
            "----------------------------------------------");

        Console.WriteLine();

        WriteAnimatedLine(
            $"Maximum speed:      " +
            $"{aircraft.Performance.MaximumSpeedKmh:N0} km/h");

        WriteAnimatedLine(
            $"Service ceiling:    " +
            $"{aircraft.Performance.ServiceCeilingMeters:N0} m");

        WriteAnimatedLine(
            $"Combat radius:      " +
            $"{FormatDistance(
                aircraft.Performance.CombatRadiusKm)}");

        WriteAnimatedLine(
            $"Ferry range:        " +
            $"{FormatDistance(
                aircraft.Performance.FerryRangeKm)}");

        Console.WriteLine();

        WriteAnimatedLine(
            $"Fuel-flow points:   " +
            $"{aircraft.Performance.FuelFlow.Count}");

        Console.WriteLine();

        foreach (FuelFlowPoint point
                 in aircraft.Performance.FuelFlow)
        {
            WriteAnimatedLine(
                $"{point.PowerMode,-12} " +
                $"{point.AltitudeMeters,7:N0} m | " +
                $"{point.TrueAirspeedKmh,6:N0} km/h | " +
                $"{point.FuelFlowKgPerHour,7:N0} kg/h | " +
                $"{(point.IsEstimate ? "EST" : "DATA")}");
        }

        Pause();
    }

    private static string FormatDistance(
        double distanceKm)
    {
        return distanceKm > 0
            ? $"{distanceKm:N0} km"
            : "N/A";
    }

    // FUEL-FLOW EDITOR

    private static void EditFuelFlowData(
        AircraftProfile aircraft)
    {
        while (true)
        {
            Console.Clear();

            WriteAnimatedLine(
                "----------------------------------------------");

            WriteAnimatedLine(
                "FUEL-FLOW DATA");

            WriteAnimatedLine(
                "----------------------------------------------");

            Console.WriteLine();

            List<FuelFlowPoint> points =
                aircraft.Performance.FuelFlow;

            if (points.Count == 0)
            {
                WriteAnimatedLine(
                    "No fuel-flow points configured.");

                Console.WriteLine();
            }
            else
            {
                for (int i = 0;
                     i < points.Count;
                     i++)
                {
                    FuelFlowPoint point =
                        points[i];

                    Console.WriteLine(
                        $"{i + 1}. " +
                        $"{point.AltitudeMeters:N0} m | " +
                        $"{point.TrueAirspeedKmh:N0} km/h | " +
                        $"{point.PowerMode,-12} | " +
                        $"{point.FuelFlowKgPerHour:N0} kg/h");
                }

                Console.WriteLine();
            }

            List<string> options =
                new()
                {
                    "Add fuel-flow point",
                    "Edit fuel-flow point",
                    "Delete fuel-flow point",
                    "Return"
                };

            int choice =
                ReadMenuChoice(options);

            switch (choice)
            {
                case 0:
                    AddFuelFlowPoint(aircraft);
                    break;

                case 1:
                    EditFuelFlowPoint(aircraft);
                    break;

                case 2:
                    DeleteFuelFlowPoint(aircraft);
                    break;

                case 3:
                    return;
            }
        }
    }

    private static void AddFuelFlowPoint(
        AircraftProfile aircraft)
    {
        Console.Clear();

        WriteAnimatedLine(
            "----------------------------------------------");

        WriteAnimatedLine(
            "ADD FUEL-FLOW POINT");

        WriteAnimatedLine(
            "----------------------------------------------");

        Console.WriteLine();

        double altitude =
            ReadDouble(
                "Altitude [m]: ",
                0,
                aircraft.Performance.ServiceCeilingMeters);

        double trueAirspeed =
            ReadDouble(
                "True airspeed [km/h]: ",
                0,
                aircraft.Performance.MaximumSpeedKmh);

        Console.WriteLine();

        List<string> powerOptions =
            new()
            {
                "Idle",
                "Military power",
                "Afterburner"
            };

        int powerChoice =
            ReadMenuChoice(powerOptions);

        EnginePowerMode powerMode =
            powerChoice switch
            {
                0 => EnginePowerMode.Idle,
                1 => EnginePowerMode.Military,
                2 => EnginePowerMode.Afterburner,
                _ => EnginePowerMode.Military
            };

        Console.WriteLine();

        double fuelFlow =
            ReadDouble(
                "Fuel flow [kg/h]: ",
                0,
                100000);

        Console.WriteLine();

        Console.Write(
            "Is this an estimate? [y/n]: ");

        string? estimateInput =
            Console.ReadLine();

        bool isEstimate =
            estimateInput?.Trim()
                .Equals(
                    "y",
                    StringComparison.OrdinalIgnoreCase)
            == true;

        Console.Write(
            "Source: ");

        string? source =
            Console.ReadLine();

        aircraft.Performance.FuelFlow.Add(
            new FuelFlowPoint
            {
                AltitudeMeters = altitude,
                TrueAirspeedKmh = trueAirspeed,
                PowerMode = powerMode,
                FuelFlowKgPerHour = fuelFlow,
                IsEstimate = isEstimate,
                Source = source?.Trim() ?? ""
            });

        SortFuelFlowPoints(aircraft);

        Console.WriteLine();

        WriteAnimatedLine(
            "Fuel-flow point added.");

        Pause();
    }

    private static void EditFuelFlowPoint(
        AircraftProfile aircraft)
    {
        List<FuelFlowPoint> points =
            aircraft.Performance.FuelFlow;

        if (points.Count == 0)
        {
            Console.WriteLine();

            WriteAnimatedLine(
                "There are no fuel-flow points to edit.");

            Pause();
            return;
        }

        Console.WriteLine();

        int index =
            ReadNumberSelection(
                "Point number: ",
                1,
                points.Count) - 1;

        FuelFlowPoint point =
            points[index];

        Console.WriteLine();

        point.AltitudeMeters =
            ReadDouble(
                $"Altitude [{point.AltitudeMeters:N0} m]: ",
                0,
                aircraft.Performance.ServiceCeilingMeters);

        point.TrueAirspeedKmh =
            ReadDouble(
                $"True airspeed [{point.TrueAirspeedKmh:N0} km/h]: ",
                0,
                aircraft.Performance.MaximumSpeedKmh);

        Console.WriteLine();

        List<string> powerOptions =
            new()
            {
                "Idle",
                "Military power",
                "Afterburner"
            };

        int powerChoice =
            ReadMenuChoice(powerOptions);

        point.PowerMode =
            powerChoice switch
            {
                0 => EnginePowerMode.Idle,
                1 => EnginePowerMode.Military,
                2 => EnginePowerMode.Afterburner,
                _ => point.PowerMode
            };

        Console.WriteLine();

        point.FuelFlowKgPerHour =
            ReadDouble(
                $"Fuel flow [{point.FuelFlowKgPerHour:N0} kg/h]: ",
                0,
                100000);

        Console.Write(
            $"Original fuel flow [lb/h] " +
            $"[{point.FuelFlowLbPerHour?.ToString("N0") ?? "none"}]: ");

        string? lbInput =
            Console.ReadLine();

        if (string.IsNullOrWhiteSpace(lbInput))
        {
        }
        else if (double.TryParse(
                     lbInput,
                     NumberStyles.Float,
                     CultureInfo.InvariantCulture,
                     out double lbValue) &&
                 lbValue >= 0)
        {
            point.FuelFlowLbPerHour =
                lbValue;
        }
        else
        {
            Console.WriteLine(
                "Invalid lb/h value. Existing value retained.");
        }

        Console.WriteLine();

        Console.Write(
            $"Is this an estimate? " +
            $"[{(point.IsEstimate ? "y" : "n")}]: ");

        string? estimateInput =
            Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(estimateInput))
        {
            point.IsEstimate =
                estimateInput.Trim()
                    .Equals(
                        "y",
                        StringComparison.OrdinalIgnoreCase);
        }

        Console.Write(
            $"Source [{point.Source}]: ");

        string? source =
            Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(source))
        {
            point.Source =
                source.Trim();
        }

        SortFuelFlowPoints(aircraft);

        Console.WriteLine();

        WriteAnimatedLine(
            "Fuel-flow point updated.");

        Pause();
    }

    private static void DeleteFuelFlowPoint(
        AircraftProfile aircraft)
    {
        List<FuelFlowPoint> points =
            aircraft.Performance.FuelFlow;

        if (points.Count == 0)
        {
            Console.WriteLine();

            WriteAnimatedLine(
                "There are no fuel-flow points to delete.");

            Pause();
            return;
        }

        Console.WriteLine();

        int index =
            ReadNumberSelection(
                "Point number: ",
                1,
                points.Count) - 1;

        FuelFlowPoint point =
            points[index];

        Console.WriteLine();

        Console.WriteLine(
            $"Altitude:       {point.AltitudeMeters:N0} m");

        Console.WriteLine(
            $"True airspeed:  {point.TrueAirspeedKmh:N0} km/h");

        Console.WriteLine(
            $"Power mode:     {point.PowerMode}");

        Console.WriteLine(
            $"Fuel flow:      {point.FuelFlowKgPerHour:N0} kg/h");

        Console.WriteLine();

        Console.Write(
            "Delete this point? [y/n]: ");

        string? input =
            Console.ReadLine();

        if (input?.Trim().Equals(
                "y",
                StringComparison.OrdinalIgnoreCase)
            == true)
        {
            points.RemoveAt(index);

            Console.WriteLine();

            WriteAnimatedLine(
                "Fuel-flow point deleted.");
        }
        else
        {
            Console.WriteLine();

            WriteAnimatedLine(
                "Deletion cancelled.");
        }

        Pause();
    }

    private static void SortFuelFlowPoints(
        AircraftProfile aircraft)
    {
        aircraft.Performance.FuelFlow =
            aircraft.Performance.FuelFlow
                .OrderBy(x => x.PowerMode)
                .ThenBy(x => x.AltitudeMeters)
                .ThenBy(x => x.TrueAirspeedKmh)
                .ToList();
    }

    // DATABASE OPERATIONS

    private static void SaveAircraftProfile(
        AircraftProfile aircraft)
    {
        bool success =
            Repository.SaveAircraftProfile(aircraft);

        Console.WriteLine();

        if (success)
        {
            Console.WriteLine(
                $"Saved: {aircraft.FilePath}");
        }
        else
        {
            Console.WriteLine(
                "Could not save aircraft profile.");
        }

        Pause();
    }

    private static void ReloadAircraftProfile(
        AircraftProfile aircraft)
    {
        AircraftProfile? reloaded =
            Repository.ReloadAircraftProfile(aircraft);

        if (reloaded == null)
        {
            Console.WriteLine(
                "Could not reload profile.");

            Pause();
            return;
        }

        aircraft.Id =
            reloaded.Id;

        aircraft.Name =
            reloaded.Name;

        aircraft.Fuel =
            reloaded.Fuel;

        aircraft.Engine =
            reloaded.Engine;

        aircraft.Performance =
            reloaded.Performance;

        if (!string.IsNullOrWhiteSpace(
                reloaded.FilePath))
        {
            aircraft.FilePath =
                reloaded.FilePath;
        }

        Console.WriteLine(
            "Profile reloaded from disk.");

        Pause();
    }

    // FILE LOCATION

    private static void OpenProfileLocation(
        AircraftProfile aircraft)
    {
        if (string.IsNullOrWhiteSpace(
                aircraft.FilePath))
        {
            Console.WriteLine(
                "No profile path available.");

            Pause();
            return;
        }

        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments =
                        $"/select,\"{aircraft.FilePath}\"",
                    UseShellExecute = true
                });

            Console.WriteLine();

            Console.WriteLine(
                "Opened the aircraft profile in File Explorer.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Could not open File Explorer: {ex.Message}");
        }

        Pause();
    }

    // AIRCRAFT SUMMARY

    private static void PrintAircraftSummary(
        AircraftProfile aircraft)
    {
        ConsoleGraphics.DrawPanel(
            "AIRCRAFT STATUS",
            72);

        ConsoleGraphics.WritePanelLine(
            "Aircraft",
            aircraft.Name,
            72,
            ConsoleGraphics.BrightPrimary);

        ConsoleGraphics.WritePanelLine(
            "Engine",
            aircraft.Engine.Model,
            72);

        ConsoleGraphics.WritePanelLine(
            "Engines",
            aircraft.Engine.EngineCount.ToString(),
            72);

        ConsoleGraphics.WritePanelLine(
            "Maximum fuel",
            $"{aircraft.Fuel.MaximumFuelKg:N0} kg",
            72);

        ConsoleGraphics.WritePanelLine(
            "Internal fuel",
            $"{aircraft.Fuel.InternalFuelKg:N0} kg",
            72);

        ConsoleGraphics.WritePanelLine(
            "External fuel",
            $"{aircraft.Fuel.ExternalFuelKg:N0} kg",
            72);

        ConsoleGraphics.WritePanelLine(
            "Maximum speed",
            $"{aircraft.Performance.MaximumSpeedKmh:N0} km/h",
            72);

        ConsoleGraphics.WritePanelLine(
            "Service ceiling",
            $"{aircraft.Performance.ServiceCeilingMeters:N0} m",
            72);

        ConsoleGraphics.DrawPanelBottom(72);

        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleGraphics.Success;

        Console.WriteLine(
            "  </>PROFILE LOADED");

        Console.ForegroundColor =
            ConsoleGraphics.Dim;

        Console.WriteLine(
            $"  {aircraft.Performance.FuelFlow.Count} fuel-flow data points available");

        ConsoleGraphics.ResetColor();

        Console.WriteLine();
    }


    private static void PrintResults(
        ServiceFlightCalculationResult result)
    {
        Console.Clear();

        AnimateResultStep(() =>
            ConsoleGraphics.DrawTitle(
                "FLIGHT RESULTS",
                result.Aircraft.Name.ToUpperInvariant()));

        // FLIGHT CONFIGURATION

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanel(
                "FLIGHT CONFIGURATION",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Aircraft",
                result.Aircraft.Name,
                72,
                ConsoleGraphics.BrightPrimary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Engine",
                result.Aircraft.Engine.Model,
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Power mode",
                result.PowerMode.ToString(),
                72,
                result.PowerMode == EnginePowerMode.Afterburner
                    ? ConsoleGraphics.Warning
                    : ConsoleGraphics.Primary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Fuel",
                $"{result.Fuel:N0} kg",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Altitude",
                $"{result.Altitude:N0} m",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "True airspeed",
                $"{result.TrueAirspeed:N0} km/h",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Wind component",
                $"{result.Wind:+0;-0;0} km/h",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanelBottom(72));

        Console.WriteLine();

        // CALCULATION STATUS

        if (!result.DataAvailable)
        {
            AnimateResultStep(() =>
                ConsoleGraphics.DrawPanel(
                    "CALCULATION STATUS",
                    72,
                    ConsoleGraphics.Error));

            AnimateResultStep(() =>
            {
                Console.ForegroundColor =
                    ConsoleGraphics.Error;

                Console.WriteLine(
                    "│ [ ERROR ] Fuel-flow data unavailable.");

                ConsoleGraphics.ResetColor();
            });

            AnimateResultStep(() =>
            {
                Console.ForegroundColor =
                    ConsoleGraphics.Error;

                Console.WriteLine(
                    $"│ No {result.PowerMode} fuel-flow data");

                ConsoleGraphics.ResetColor();
            });

            AnimateResultStep(() =>
            {
                Console.ForegroundColor =
                    ConsoleGraphics.Error;

                Console.WriteLine(
                    "│ covers the requested altitude/TAS.");

                ConsoleGraphics.ResetColor();
            });

            AnimateResultStep(() =>
            {
                Console.ForegroundColor =
                    ConsoleGraphics.Error;

                Console.WriteLine(
                    "└" + new string('─', 70));

                ConsoleGraphics.ResetColor();
            });

            Pause();

            return;
        }

        // CALCULATED PERFORMANCE

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanel(
                "CALCULATED PERFORMANCE",
                72,
                ConsoleGraphics.Success));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Fuel flow",
                $"{result.FuelFlow:N0} kg/h",
                72,
                ConsoleGraphics.BrightPrimary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Flight time",
                FormatFlightTime(
                    result.EnduranceHours),
                72,
                ConsoleGraphics.BrightPrimary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Ground speed",
                $"{result.GroundSpeed:N0} km/h",
                72,
                ConsoleGraphics.BrightPrimary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Estimated range",
                $"{result.RangeKm:N0} km",
                72,
                ConsoleGraphics.BrightPrimary));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Fuel / 100 km",
                $"{result.FuelPer100Km:N1} kg",
                72,
                ConsoleGraphics.Warning));

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanelBottom(
                72,
                ConsoleGraphics.Success));

        Console.WriteLine();

        // FUEL FLOW SOURCE

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanel(
                "FUEL-FLOW DATA",
                72));

        AnimateResultStep(() =>
            ConsoleGraphics.WritePanelLine(
                "Calculation method",
                result.FuelFlowSource,
                72));

        if (result.FuelFlowPoint != null)
        {
            FuelFlowPoint point =
                result.FuelFlowPoint;

            AnimateResultStep(() =>
                ConsoleGraphics.WritePanelLine(
                    "Reference altitude",
                    $"{point.AltitudeMeters:N0} m",
                    72));

            AnimateResultStep(() =>
                ConsoleGraphics.WritePanelLine(
                    "Reference TAS",
                    $"{point.TrueAirspeedKmh:N0} km/h",
                    72));

            AnimateResultStep(() =>
                ConsoleGraphics.WritePanelLine(
                    "Reference flow",
                    $"{point.FuelFlowKgPerHour:N0} kg/h",
                    72));

            AnimateResultStep(() =>
                ConsoleGraphics.WritePanelLine(
                    "Estimate",
                    point.IsEstimate
                        ? "YES"
                        : "NO",
                    72,
                    point.IsEstimate
                        ? ConsoleGraphics.Warning
                        : ConsoleGraphics.Success));

            if (!string.IsNullOrWhiteSpace(point.Source))
            {
                AnimateResultStep(() =>
                    ConsoleGraphics.WritePanelLine(
                        "Source",
                        point.Source,
                        72,
                        ConsoleGraphics.Dim));
            }
        }

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanelBottom(72));

        Console.WriteLine();

        // NOTICE

        AnimateResultStep(() =>
            ConsoleGraphics.DrawPanel(
                "CALCULATION NOTICE",
                72,
                ConsoleGraphics.Warning));

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ Fuel flow is calculated from the aircraft's");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ altitude/TAS fuel-flow database.");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ Values between database points are interpolated");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ where sufficient data exists.");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ Actual consumption varies with aircraft weight,");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Dim;

            Console.WriteLine(
                "│ drag, Mach, configuration and required thrust.");

            ConsoleGraphics.ResetColor();
        });

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Warning;

            Console.WriteLine(
                "└" + new string('─', 70));

            ConsoleGraphics.ResetColor();
        });

        Console.WriteLine();

        AnimateResultStep(() =>
        {
            Console.ForegroundColor =
                ConsoleGraphics.Success;

            Console.WriteLine(
                "CALCULATION COMPLETE :) DONT LET THE BAD MISSILES BITE");

            ConsoleGraphics.ResetColor();
        });

        Pause();
    }

    // RESULT ANIMATION

    private static void AnimateResultStep(
        Action output)
    {
        output();

        Thread.Sleep(
            ResultPrintDelayMilliseconds);
    }

    private static string FormatFlightTime(
        double hours)
    {
        int wholeHours =
            (int)Math.Floor(hours);

        int minutes =
            (int)Math.Round(
                (hours - wholeHours) * 60);

        if (minutes >= 60)
        {
            wholeHours++;
            minutes = 0;
        }

        return $"{wholeHours} h {minutes:00} min";
    }

    // ARROW-KEY MENU

    private static int ReadMenuChoice(
        List<string> options,
        bool allowEscape = true)
    {
        if (options.Count == 0)
            return -1;

        int selected = 0;

        int menuTop =
            Console.CursorTop;

        DrawSelectableMenu(
            options,
            selected,
            menuTop,
            animate: true);

        while (true)
        {
            ConsoleKeyInfo key =
                Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:

                    selected--;

                    if (selected < 0)
                    {
                        selected =
                            options.Count - 1;
                    }

                    DrawSelectableMenu(
                        options,
                        selected,
                        menuTop,
                        animate: false);

                    break;

                case ConsoleKey.DownArrow:

                    selected++;

                    if (selected >= options.Count)
                    {
                        selected = 0;
                    }

                    DrawSelectableMenu(
                        options,
                        selected,
                        menuTop,
                        animate: false);

                    break;

                case ConsoleKey.Enter:

                    return selected;

                case ConsoleKey.Escape:

                    if (allowEscape)
                    {
                        return options.Count - 1;
                    }

                    break;
            }
        }
    }

    private static void DrawSelectableMenu(
        List<string> options,
        int selected,
        int menuTop,
        bool animate)
    {
        for (int i = 0; i < options.Count; i++)
        {
            try
            {
                Console.SetCursorPosition(
                    0,
                    menuTop + i);
            }
            catch
            {
                return;
            }

            Console.ForegroundColor =
                i == selected
                    ? ConsoleGraphics.BrightPrimary
                    : ConsoleGraphics.Primary;

            Console.Write(
                i == selected
                    ? "> "
                    : "  ");

            Console.Write(
                $"{i + 1}. {options[i]}");

            int remaining =
                Console.WindowWidth -
                Console.CursorLeft;

            if (remaining > 0)
            {
                Console.Write(
                    new string(
                        ' ',
                        remaining));
            }

            if (animate &&
                i < options.Count - 1)
            {
                Thread.Sleep(150);
            }
        }

        Console.ForegroundColor =
            ConsoleGraphics.Primary;

        try
        {
            Console.SetCursorPosition(
                0,
                menuTop + options.Count);
        }
        catch
        {
        }
    }

    // INPUT

    private static string ReadString(
        string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            string? value =
                Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            Console.WriteLine(
                "Please enter a value.");
        }
    }

    private static double ReadDouble(
        string prompt,
        double minimum,
        double maximum)
    {
        while (true)
        {
            Console.Write(prompt);

            string? input =
                Console.ReadLine();

            if (double.TryParse(
                    input,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value))
            {
                if (value >= minimum &&
                    value <= maximum)
                {
                    return value;
                }
            }

            Console.WriteLine(
                $"Enter a value between " +
                $"{minimum} and {maximum}.");
        }
    }

    private static int ReadInt(
        string prompt,
        int minimum,
        int maximum)
    {
        while (true)
        {
            Console.Write(prompt);

            string? input =
                Console.ReadLine();

            if (int.TryParse(
                    input,
                    out int value))
            {
                if (value >= minimum &&
                    value <= maximum)
                {
                    return value;
                }
            }

            Console.WriteLine(
                $"Enter a number between " +
                $"{minimum} and {maximum}.");
        }
    }

    private static int ReadNumberSelection(
        string prompt,
        int minimum,
        int maximum)
    {
        while (true)
        {
            Console.Write(prompt);

            string? input =
                Console.ReadLine();

            if (int.TryParse(
                    input,
                    out int value) &&
                value >= minimum &&
                value <= maximum)
            {
                return value;
            }

            Console.WriteLine(
                $"Enter a number between " +
                $"{minimum} and {maximum}.");
        }
    }

    private static void Pause()
    {
        Console.WriteLine();

        Console.ForegroundColor =
            ConsoleGraphics.Secondary;

        Console.WriteLine(
            "  ─────────────────────────────────────────────────────────────");

        Console.ForegroundColor =
            ConsoleGraphics.Warning;

        Console.Write(
            "  [ PRESS ANY KEY TO CONTINUE ]");

        ConsoleGraphics.ResetColor();

        Console.ReadKey(true);
    }


    private static void WriteAnimatedLine(
        string text,
        int delayMilliseconds = NormalTextDelayMilliseconds)
    {
        foreach (char character in text)
        {
            Console.Write(character);

            Thread.Sleep(delayMilliseconds);
        }

        Console.WriteLine();
    }
}