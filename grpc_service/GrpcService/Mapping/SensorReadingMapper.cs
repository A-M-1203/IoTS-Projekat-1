using Google.Protobuf.WellKnownTypes;
using GrpcService.Models;
using Proto = GrpcService.Protos;

namespace GrpcService.Mapping;

public static class SensorReadingMapper
{
    public static Proto.SensorReading ToProto(Models.SensorReading reading)
    {
        var proto = new Proto.SensorReading
        {
            Id = reading.Id,
            Timestamp = Timestamp.FromDateTimeOffset(reading.Timestamp),
            DeviceId = reading.DeviceId,
        };

        if (reading.Location is not null)
        {
            proto.Location = reading.Location;
        }

        if (reading.CropType is not null)
        {
            proto.CropType = reading.CropType;
        }

        if (reading.Season is not null)
        {
            proto.Season = reading.Season;
        }

        if (reading.Temperature is not null)
        {
            proto.Temperature = (double)reading.Temperature;
        }

        if (reading.Humidity is not null)
        {
            proto.Humidity = (double)reading.Humidity;
        }

        if (reading.Rainfall is not null)
        {
            proto.Rainfall = (double)reading.Rainfall;
        }

        if (reading.SoilMoisture is not null)
        {
            proto.SoilMoisture = (double)reading.SoilMoisture;
        }

        if (reading.SoilPh is not null)
        {
            proto.SoilPh = (double)reading.SoilPh;
        }

        if (reading.LightIntensity is not null)
        {
            proto.LightIntensity = (double)reading.LightIntensity;
        }

        if (reading.FertilizerUsed is not null)
        {
            proto.FertilizerUsed = (double)reading.FertilizerUsed;
        }

        if (reading.IrrigationNeeded is not null)
        {
            proto.IrrigationNeeded = reading.IrrigationNeeded.Value;
        }

        if (reading.CropHealth is not null)
        {
            proto.CropHealth = reading.CropHealth;
        }

        if (reading.YieldEstimate is not null)
        {
            proto.YieldEstimate = (double)reading.YieldEstimate;
        }

        if (reading.PestRisk is not null)
        {
            proto.PestRisk = reading.PestRisk;
        }

        if (reading.AnomalyFlag is not null)
        {
            proto.AnomalyFlag = reading.AnomalyFlag.Value;
        }

        return proto;
    }

    public static SensorReadingInput FromProto(Proto.SensorReadingInput proto)
    {
        if (proto.Timestamp is null)
        {
            throw new ArgumentException("timestamp is required.");
        }

        if (string.IsNullOrWhiteSpace(proto.DeviceId))
        {
            throw new ArgumentException("device_id is required.");
        }

        return new SensorReadingInput
        {
            Timestamp = proto.Timestamp.ToDateTimeOffset(),
            DeviceId = proto.DeviceId,
            Location = proto.HasLocation ? proto.Location : null,
            CropType = proto.HasCropType ? proto.CropType : null,
            Season = proto.HasSeason ? proto.Season : null,
            Temperature = proto.HasTemperature ? (decimal)proto.Temperature : null,
            Humidity = proto.HasHumidity ? (decimal)proto.Humidity : null,
            Rainfall = proto.HasRainfall ? (decimal)proto.Rainfall : null,
            SoilMoisture = proto.HasSoilMoisture ? (decimal)proto.SoilMoisture : null,
            SoilPh = proto.HasSoilPh ? (decimal)proto.SoilPh : null,
            LightIntensity = proto.HasLightIntensity ? (decimal)proto.LightIntensity : null,
            FertilizerUsed = proto.HasFertilizerUsed ? (decimal)proto.FertilizerUsed : null,
            IrrigationNeeded = proto.HasIrrigationNeeded ? proto.IrrigationNeeded : null,
            CropHealth = proto.HasCropHealth ? proto.CropHealth : null,
            YieldEstimate = proto.HasYieldEstimate ? (decimal)proto.YieldEstimate : null,
            PestRisk = proto.HasPestRisk ? proto.PestRisk : null,
            AnomalyFlag = proto.HasAnomalyFlag ? proto.AnomalyFlag : null,
        };
    }
}
