from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class SensorReadingCreate(BaseModel):
    timestamp: datetime
    device_id: str = Field(..., max_length=20)
    location: str = Field(..., max_length=50)
    crop_type: str = Field(..., max_length=20)
    season: str = Field(..., max_length=10)
    temperature: float
    humidity: float
    rainfall: float
    soil_moisture: float
    soil_ph: float
    light_intensity: float
    fertilizer_used: float
    irrigation_needed: bool
    crop_health: str = Field(..., max_length=20)
    yield_estimate: float
    pest_risk: str = Field(..., max_length=10)
    anomaly_flag: bool


class SensorReadingUpdate(BaseModel):
    timestamp: datetime | None = None
    device_id: str | None = Field(None, max_length=20)
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


class SensorReadingResponse(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    timestamp: datetime
    device_id: str
    location: str | None = None
    crop_type: str | None = None
    season: str | None = None
    temperature: float | None = None
    humidity: float | None = None
    rainfall: float | None = None
    soil_moisture: float | None = None
    soil_ph: float | None = None
    light_intensity: float | None = None
    fertilizer_used: float | None = None
    irrigation_needed: bool | None = None
    crop_health: str | None = None
    yield_estimate: float | None = None
    pest_risk: str | None = None
    anomaly_flag: bool | None = None
