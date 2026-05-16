import csv
import sys
import time
from datetime import datetime
from pathlib import Path

import psycopg

DB_HOST = "localhost"
DB_PORT = 5432
DB_NAME = "iots-projekat-1"
DB_USER = "postgres"
DB_PASSWORD = "vobo1234"

BATCH_SIZE = 10
INTERVAL_SECONDS = 10

CSV_FILENAME = "BIED_Smart_Agriculture_Dataset.csv"
TIMESTAMP_FORMAT = "%Y-%m-%d %H:%M:%S"

NUMERIC_COLUMNS = (
    "temperature",
    "humidity",
    "rainfall",
    "soil_moisture",
    "soil_ph",
    "light_intensity",
    "fertilizer_used",
    "yield_estimate",
)
BOOLEAN_COLUMNS = ("irrigation_needed", "anomaly_flag")
TEXT_COLUMNS = ("device_id", "location", "crop_type", "season", "crop_health", "pest_risk")

INSERT_SQL = """
INSERT INTO sensor_readings (
    timestamp, device_id, location, crop_type, season,
    temperature, humidity, rainfall, soil_moisture, soil_ph,
    light_intensity, fertilizer_used, irrigation_needed,
    crop_health, yield_estimate, pest_risk, anomaly_flag
) VALUES (
    %s, %s, %s, %s, %s,
    %s, %s, %s, %s, %s,
    %s, %s, %s,
    %s, %s, %s, %s
)
"""


def parse_row(row: dict[str, str]) -> tuple:
    timestamp = datetime.strptime(row["timestamp"], TIMESTAMP_FORMAT)

    text_values = {col: row[col] or None for col in TEXT_COLUMNS}
    numeric_values = {
        col: float(row[col]) if row[col] != "" else None for col in NUMERIC_COLUMNS
    }
    boolean_values = {
        col: bool(int(row[col])) if row[col] != "" else None for col in BOOLEAN_COLUMNS
    }

    return (
        timestamp,
        text_values["device_id"],
        text_values["location"],
        text_values["crop_type"],
        text_values["season"],
        numeric_values["temperature"],
        numeric_values["humidity"],
        numeric_values["rainfall"],
        numeric_values["soil_moisture"],
        numeric_values["soil_ph"],
        numeric_values["light_intensity"],
        numeric_values["fertilizer_used"],
        boolean_values["irrigation_needed"],
        text_values["crop_health"],
        numeric_values["yield_estimate"],
        text_values["pest_risk"],
        boolean_values["anomaly_flag"],
    )


def clear_existing_data(conn: psycopg.Connection) -> None:
    with conn.cursor() as cur:
        cur.execute("SELECT EXISTS (SELECT 1 FROM sensor_readings LIMIT 1)")
        has_data = cur.fetchone()[0]
        if has_data:
            cur.execute("DELETE FROM sensor_readings")
    conn.commit()
    if has_data:
        print("Obrisani postojeci podaci iz sensor_readings.")


def insert_batch(conn: psycopg.Connection, batch: list[tuple]) -> None:
    with conn.cursor() as cur:
        cur.executemany(INSERT_SQL, batch)
    conn.commit()


def stream_csv_batches(csv_path: Path):
    with csv_path.open(newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        batch: list[tuple] = []
        for row in reader:
            batch.append(parse_row(row))
            if len(batch) == BATCH_SIZE:
                yield batch
                batch = []
        if batch:
            yield batch


def main() -> int:
    csv_path = Path(__file__).resolve().parent / CSV_FILENAME
    if not csv_path.is_file():
        print(f"CSV fajl nije pronadjen: {csv_path}", file=sys.stderr)
        return 1

    conn = None
    try:
        conn = psycopg.connect(
            host=DB_HOST,
            port=DB_PORT,
            dbname=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD,
        )
        clear_existing_data(conn)

        total_inserted = 0
        batch_number = 0
        for batch in stream_csv_batches(csv_path):
            batch_number += 1
            insert_batch(conn, batch)
            total_inserted += len(batch)
            print(f"Batch {batch_number}: upisano {len(batch)} redova (ukupno {total_inserted})")
            time.sleep(INTERVAL_SECONDS)

        print(f"Zavrseno. Ukupno upisano {total_inserted} redova.")
        return 0
    except KeyboardInterrupt:
        print("\nPrekinuto od strane korisnika.")
        return 130
    except psycopg.Error as exc:
        print(f"Greska baze podataka: {exc}", file=sys.stderr)
        return 1
    finally:
        if conn is not None:
            conn.close()


if __name__ == "__main__":
    sys.exit(main())
