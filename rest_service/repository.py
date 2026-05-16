from psycopg import Connection

from schemas import SensorReadingCreate, SensorReadingUpdate

COLUMNS = """
    id, timestamp, device_id, location, crop_type, season,
    temperature, humidity, rainfall, soil_moisture, soil_ph,
    light_intensity, fertilizer_used, irrigation_needed,
    crop_health, yield_estimate, pest_risk, anomaly_flag
"""

INSERT_COLUMNS = """
    timestamp, device_id, location, crop_type, season,
    temperature, humidity, rainfall, soil_moisture, soil_ph,
    light_intensity, fertilizer_used, irrigation_needed,
    crop_health, yield_estimate, pest_risk, anomaly_flag
"""


def _row_params(data: SensorReadingCreate | SensorReadingUpdate) -> tuple:
    return (
        data.timestamp,
        data.device_id,
        data.location,
        data.crop_type,
        data.season,
        data.temperature,
        data.humidity,
        data.rainfall,
        data.soil_moisture,
        data.soil_ph,
        data.light_intensity,
        data.fertilizer_used,
        data.irrigation_needed,
        data.crop_health,
        data.yield_estimate,
        data.pest_risk,
        data.anomaly_flag,
    )


def list_readings(conn: Connection, limit: int, offset: int) -> list[dict]:
    with conn.cursor() as cur:
        cur.execute(
            f"SELECT {COLUMNS} FROM sensor_readings ORDER BY id LIMIT %s OFFSET %s",
            (limit, offset),
        )
        return cur.fetchall()


def get_reading(conn: Connection, reading_id: int) -> dict | None:
    with conn.cursor() as cur:
        cur.execute(
            f"SELECT {COLUMNS} FROM sensor_readings WHERE id = %s",
            (reading_id,),
        )
        return cur.fetchone()


def create_reading(conn: Connection, data: SensorReadingCreate) -> dict:
    placeholders = ", ".join(["%s"] * 17)
    with conn.cursor() as cur:
        cur.execute(
            f"""
            INSERT INTO sensor_readings ({INSERT_COLUMNS})
            VALUES ({placeholders})
            RETURNING {COLUMNS}
            """,
            _row_params(data),
        )
        row = cur.fetchone()
    conn.commit()
    return row


def update_reading(
    conn: Connection, reading_id: int, data: SensorReadingUpdate
) -> dict | None:
    with conn.cursor() as cur:
        cur.execute(
            f"""
            UPDATE sensor_readings SET
                timestamp = %s,
                device_id = %s,
                location = %s,
                crop_type = %s,
                season = %s,
                temperature = %s,
                humidity = %s,
                rainfall = %s,
                soil_moisture = %s,
                soil_ph = %s,
                light_intensity = %s,
                fertilizer_used = %s,
                irrigation_needed = %s,
                crop_health = %s,
                yield_estimate = %s,
                pest_risk = %s,
                anomaly_flag = %s
            WHERE id = %s
            RETURNING {COLUMNS}
            """,
            (*_row_params(data), reading_id),
        )
        row = cur.fetchone()
    conn.commit()
    return row


def delete_reading(conn: Connection, reading_id: int) -> bool:
    with conn.cursor() as cur:
        cur.execute("DELETE FROM sensor_readings WHERE id = %s", (reading_id,))
        deleted = cur.rowcount > 0
    conn.commit()
    return deleted
