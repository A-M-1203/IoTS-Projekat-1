import { parseResolveInfo } from "graphql-parse-resolve-info";

export const FIELD_TO_COLUMN = {
  id: "id",
  timestamp: "timestamp",
  deviceId: "device_id",
  location: "location",
  cropType: "crop_type",
  season: "season",
  temperature: "temperature",
  humidity: "humidity",
  rainfall: "rainfall",
  soilMoisture: "soil_moisture",
  soilPh: "soil_ph",
  lightIntensity: "light_intensity",
  fertilizerUsed: "fertilizer_used",
  irrigationNeeded: "irrigation_needed",
  cropHealth: "crop_health",
  yieldEstimate: "yield_estimate",
  pestRisk: "pest_risk",
  anomalyFlag: "anomaly_flag",
};

export const ALL_COLUMNS = Object.values(FIELD_TO_COLUMN);

const COLUMN_TO_FIELD = Object.fromEntries(
  Object.entries(FIELD_TO_COLUMN).map(([field, column]) => [column, field]),
);

export function getSelectedColumns(info) {
  const parsed = parseResolveInfo(info);
  if (!parsed?.fieldsByTypeName?.SensorReading) {
    return ALL_COLUMNS;
  }

  const selectedFields = Object.keys(parsed.fieldsByTypeName.SensorReading).filter(
    (field) => FIELD_TO_COLUMN[field],
  );

  if (selectedFields.length === 0) {
    return ALL_COLUMNS;
  }

  const columns = new Set(selectedFields.map((field) => FIELD_TO_COLUMN[field]));
  columns.add("id");
  return Array.from(columns);
}

export function columnsToSelectClause(columns) {
  return columns.join(", ");
}

export function mapRowToGraphQL(row) {
  if (!row) {
    return null;
  }

  const result = {};

  for (const [column, field] of Object.entries(COLUMN_TO_FIELD)) {
    if (!(column in row)) {
      continue;
    }

    const value = row[column];
    if (value === null || value === undefined) {
      result[field] = null;
      continue;
    }

    if (field === "id") {
      result.id = String(value);
    } else if (field === "timestamp") {
      result.timestamp =
        value instanceof Date ? value.toISOString() : new Date(value).toISOString();
    } else if (typeof value === "object" && value instanceof Date) {
      result[field] = value.toISOString();
    } else {
      result[field] = value;
    }
  }

  return result;
}

export function mapInputToDb(input) {
  return {
    timestamp: new Date(input.timestamp),
    device_id: input.deviceId,
    location: input.location ?? null,
    crop_type: input.cropType ?? null,
    season: input.season ?? null,
    temperature: input.temperature ?? null,
    humidity: input.humidity ?? null,
    rainfall: input.rainfall ?? null,
    soil_moisture: input.soilMoisture ?? null,
    soil_ph: input.soilPh ?? null,
    light_intensity: input.lightIntensity ?? null,
    fertilizer_used: input.fertilizerUsed ?? null,
    irrigation_needed: input.irrigationNeeded ?? null,
    crop_health: input.cropHealth ?? null,
    yield_estimate: input.yieldEstimate ?? null,
    pest_risk: input.pestRisk ?? null,
    anomaly_flag: input.anomalyFlag ?? null,
  };
}
