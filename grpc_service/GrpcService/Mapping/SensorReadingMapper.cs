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

    public static SensorReadingCreateInput FromCreateProto(Proto.SensorReadingCreateInput proto)
    {
        if (proto.Timestamp is null)
        {
            throw new ArgumentException("timestamp is required.");
        }

        if (string.IsNullOrWhiteSpace(proto.DeviceId))
        {
            throw new ArgumentException("device_id is required.");
        }

        return new SensorReadingCreateInput
        {
            Timestamp = proto.Timestamp.ToDateTimeOffset(),
            DeviceId = proto.DeviceId,
            Location = proto.Location,
            CropType = proto.CropType,
            Season = proto.Season,
            Temperature = (decimal)proto.Temperature,
            Humidity = (decimal)proto.Humidity,
            Rainfall = (decimal)proto.Rainfall,
            SoilMoisture = (decimal)proto.SoilMoisture,
            SoilPh = (decimal)proto.SoilPh,
            LightIntensity = (decimal)proto.LightIntensity,
            FertilizerUsed = (decimal)proto.FertilizerUsed,
            IrrigationNeeded = proto.IrrigationNeeded,
            CropHealth = proto.CropHealth,
            YieldEstimate = (decimal)proto.YieldEstimate,
            PestRisk = proto.PestRisk,
            AnomalyFlag = proto.AnomalyFlag,
        };
    }

    public static SensorReadingPatch FromUpdateProto(Proto.SensorReadingUpdateInput proto)
    {
        var patch = new SensorReadingPatch();

        if (proto.Timestamp is not null)
        {
            patch.Fields["timestamp"] = proto.Timestamp.ToDateTimeOffset();
        }

        if (proto.HasDeviceId)
        {
            patch.Fields["device_id"] = proto.DeviceId;
        }

        if (proto.HasLocation)
        {
            patch.Fields["location"] = proto.Location;
        }

        if (proto.HasCropType)
        {
            patch.Fields["crop_type"] = proto.CropType;
        }

        if (proto.HasSeason)
        {
            patch.Fields["season"] = proto.Season;
        }

        if (proto.HasTemperature)
        {
            patch.Fields["temperature"] = (decimal)proto.Temperature;
        }

        if (proto.HasHumidity)
        {
            patch.Fields["humidity"] = (decimal)proto.Humidity;
        }

        if (proto.HasRainfall)
        {
            patch.Fields["rainfall"] = (decimal)proto.Rainfall;
        }

        if (proto.HasSoilMoisture)
        {
            patch.Fields["soil_moisture"] = (decimal)proto.SoilMoisture;
        }

        if (proto.HasSoilPh)
        {
            patch.Fields["soil_ph"] = (decimal)proto.SoilPh;
        }

        if (proto.HasLightIntensity)
        {
            patch.Fields["light_intensity"] = (decimal)proto.LightIntensity;
        }

        if (proto.HasFertilizerUsed)
        {
            patch.Fields["fertilizer_used"] = (decimal)proto.FertilizerUsed;
        }

        if (proto.HasIrrigationNeeded)
        {
            patch.Fields["irrigation_needed"] = proto.IrrigationNeeded;
        }

        if (proto.HasCropHealth)
        {
            patch.Fields["crop_health"] = proto.CropHealth;
        }

        if (proto.HasYieldEstimate)
        {
            patch.Fields["yield_estimate"] = (decimal)proto.YieldEstimate;
        }

        if (proto.HasPestRisk)
        {
            patch.Fields["pest_risk"] = proto.PestRisk;
        }

        if (proto.HasAnomalyFlag)
        {
            patch.Fields["anomaly_flag"] = proto.AnomalyFlag;
        }

        return patch;
    }
}
