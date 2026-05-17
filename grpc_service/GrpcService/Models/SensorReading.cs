namespace GrpcService.Models;

public class SensorReading
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? CropType { get; set; }
    public string? Season { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? Rainfall { get; set; }
    public decimal? SoilMoisture { get; set; }
    public decimal? SoilPh { get; set; }
    public decimal? LightIntensity { get; set; }
    public decimal? FertilizerUsed { get; set; }
    public bool? IrrigationNeeded { get; set; }
    public string? CropHealth { get; set; }
    public decimal? YieldEstimate { get; set; }
    public string? PestRisk { get; set; }
    public bool? AnomalyFlag { get; set; }
}

public class SensorReadingCreateInput
{
    public DateTimeOffset Timestamp { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CropType { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public decimal Rainfall { get; set; }
    public decimal SoilMoisture { get; set; }
    public decimal SoilPh { get; set; }
    public decimal LightIntensity { get; set; }
    public decimal FertilizerUsed { get; set; }
    public bool IrrigationNeeded { get; set; }
    public string CropHealth { get; set; } = string.Empty;
    public decimal YieldEstimate { get; set; }
    public string PestRisk { get; set; } = string.Empty;
    public bool AnomalyFlag { get; set; }
}

public class SensorReadingPatch
{
    public Dictionary<string, object?> Fields { get; } = new(StringComparer.Ordinal);
}
