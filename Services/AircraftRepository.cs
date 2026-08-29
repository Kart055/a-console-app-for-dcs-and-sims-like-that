using DcsFlightCalculator.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DcsFlightCalculator.Services;

public class AircraftRepository
{
    private readonly string _aircraftDirectory;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public AircraftRepository()
    {
        _aircraftDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Aircraft");

        EnsureAircraftDirectory();
    }

    public string AircraftDirectory =>
        _aircraftDirectory;

    public List<AircraftProfile> LoadAircraftProfiles()
    {
        List<AircraftProfile> aircraft = new();

        if (!Directory.Exists(_aircraftDirectory))
        {
            return aircraft;
        }

        foreach (string file in Directory.GetFiles(
                     _aircraftDirectory,
                     "*.json"))
        {
            try
            {
                string json =
                    File.ReadAllText(file);

                AircraftProfile? profile =
                    JsonSerializer.Deserialize<AircraftProfile>(
                        json,
                        _jsonOptions);

                if (profile == null)
                {
                    continue;
                }

                profile.FilePath = file;

                aircraft.Add(profile);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"Could not load {Path.GetFileName(file)}");

                Console.WriteLine(
                    $"JSON error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Could not load {Path.GetFileName(file)}");

                Console.WriteLine(
                    ex.Message);
            }
        }

        return aircraft
            .OrderBy(x => x.Name)
            .ToList();
    }

    public bool SaveAircraftProfile(
        AircraftProfile aircraft)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(
                    aircraft.FilePath))
            {
                return false;
            }

            string json =
                JsonSerializer.Serialize(
                    aircraft,
                    _jsonOptions);

            File.WriteAllText(
                aircraft.FilePath,
                json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public AircraftProfile? ReloadAircraftProfile(
        AircraftProfile aircraft)
    {
        if (string.IsNullOrWhiteSpace(
                aircraft.FilePath))
        {
            return null;
        }

        try
        {
            string json =
                File.ReadAllText(
                    aircraft.FilePath);

            AircraftProfile? reloaded =
                JsonSerializer.Deserialize<AircraftProfile>(
                    json,
                    _jsonOptions);

            if (reloaded != null)
            {
                reloaded.FilePath =
                    aircraft.FilePath;
            }

            return reloaded;
        }
        catch
        {
            return null;
        }
    }

    private void EnsureAircraftDirectory()
    {
        Directory.CreateDirectory(
            _aircraftDirectory);
    }
}
