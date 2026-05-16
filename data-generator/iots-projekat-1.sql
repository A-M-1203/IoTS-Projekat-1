CREATE TABLE sensor_readings (
    id                  BIGSERIAL       PRIMARY KEY,
    timestamp           TIMESTAMPTZ     NOT NULL,
    device_id           VARCHAR(20)     NOT NULL,
    location            VARCHAR(50),
    crop_type           VARCHAR(20),
    season              VARCHAR(10),
    temperature         NUMERIC(5,2),
    humidity            NUMERIC(5,2),
    rainfall            NUMERIC(6,2),
    soil_moisture       NUMERIC(5,2),
    soil_ph             NUMERIC(4,2),
    light_intensity     NUMERIC(8,2),
    fertilizer_used     NUMERIC(8,2),
    irrigation_needed   BOOLEAN,
    crop_health         VARCHAR(20),
    yield_estimate      NUMERIC(8,2),
    pest_risk           VARCHAR(10),
    anomaly_flag        BOOLEAN
);
 
CREATE INDEX idx_timestamp ON sensor_readings (timestamp DESC);
CREATE INDEX idx_device_id ON sensor_readings (device_id);
CREATE INDEX idx_device_time ON sensor_readings (device_id, timestamp DESC);