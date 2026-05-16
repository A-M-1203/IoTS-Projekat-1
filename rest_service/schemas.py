from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class SensorReadingBase(BaseModel):
    timestamp: datetime
    device_id: str = Field(..., max_length=20)
    location: str | None = Field(None, max_length=50)
    crop_type: str | None = Field(None, max_length=20)
    season: str | None = Field(None, max_length=10)
    temperature: float | None = None
    humidity: float | None = None
    rainfall: float | None = None
    soil_moisture: float | None = None
    soil_ph: float | None = None
    light_intensity: float | None = None
    fertilizer_used: float | None = None
    irrigation_needed: bool | None = None
    crop_health: str | None = Field(None, max_length=20)
    yield_estimate: float | None = None
    pest_risk: str | None = Field(None, max_length=10)
    anomaly_flag: bool | None = None


class SensorReadingCreate(SensorReadingBase):
    pass


class SensorReadingUpdate(SensorReadingBase):
    pass


class SensorReadingResponse(SensorReadingBase):
    model_config = ConfigDict(from_attributes=True)

    id: int
