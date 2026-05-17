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

FIELD_TO_COLUMN = {
    "timestamp": "timestamp",
    "device_id": "device_id",
    "location": "location",
    "crop_type": "crop_type",
    "season": "season",
    "temperature": "temperature",
    "humidity": "humidity",
    "rainfall": "rainfall",
    "soil_moisture": "soil_moisture",
    "soil_ph": "soil_ph",
    "light_intensity": "light_intensity",
    "fertilizer_used": "fertilizer_used",
    "irrigation_needed": "irrigation_needed",
    "crop_health": "crop_health",
    "yield_estimate": "yield_estimate",
    "pest_risk": "pest_risk",
    "anomaly_flag": "anomaly_flag",
}


def _row_params(data: SensorReadingCreate) -> tuple:
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
    updates = data.model_dump(exclude_unset=True)
    if not updates:
        raise ValueError("No fields to update")

    set_clauses = []
    values = []
    for field, value in updates.items():
        column = FIELD_TO_COLUMN[field]
        set_clauses.append(f"{column} = %s")
        values.append(value)

    values.append(reading_id)
    set_sql = ", ".join(set_clauses)

    with conn.cursor() as cur:
        cur.execute(
            f"""
            UPDATE sensor_readings SET
                {set_sql}
            WHERE id = %s
            RETURNING {COLUMNS}
            """,
            values,
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
