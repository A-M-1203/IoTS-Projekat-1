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

export function mapCreateInputToDb(input) {
  return {
    timestamp: new Date(input.timestamp),
    device_id: input.deviceId,
    location: input.location,
    crop_type: input.cropType,
    season: input.season,
    temperature: input.temperature,
    humidity: input.humidity,
    rainfall: input.rainfall,
    soil_moisture: input.soilMoisture,
    soil_ph: input.soilPh,
    light_intensity: input.lightIntensity,
    fertilizer_used: input.fertilizerUsed,
    irrigation_needed: input.irrigationNeeded,
    crop_health: input.cropHealth,
    yield_estimate: input.yieldEstimate,
    pest_risk: input.pestRisk,
    anomaly_flag: input.anomalyFlag,
  };
}

const INPUT_FIELD_TO_COLUMN = Object.fromEntries(
  Object.entries(FIELD_TO_COLUMN).filter(([field]) => field !== "id"),
);

export function mapUpdateInputToDb(input) {
  const patch = {};

  for (const [field, column] of Object.entries(INPUT_FIELD_TO_COLUMN)) {
    if (input[field] === undefined) {
      continue;
    }

    if (field === "timestamp") {
      patch[column] = input.timestamp === null ? null : new Date(input.timestamp);
    } else {
      patch[column] = input[field];
    }
  }

  return patch;
}
