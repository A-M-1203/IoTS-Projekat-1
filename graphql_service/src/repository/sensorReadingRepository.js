import {
  columnsToSelectClause,
  mapCreateInputToDb,
  mapRowToGraphQL,
  mapUpdateInputToDb,
} from "../utils/fieldSelection.js";

const INSERT_COLUMNS = `
  timestamp, device_id, location, crop_type, season,
  temperature, humidity, rainfall, soil_moisture, soil_ph,
  light_intensity, fertilizer_used, irrigation_needed,
  crop_health, yield_estimate, pest_risk, anomaly_flag
`;

function buildInsertValues() {
  return Array.from({ length: 17 }, (_, index) => `$${index + 1}`).join(", ");
}

export async function listReadings(pool, limit, offset, columns) {
  const selectClause = columnsToSelectClause(columns);
  const result = await pool.query(
    `
    SELECT ${selectClause}
    FROM sensor_readings
    ORDER BY id
    LIMIT $1 OFFSET $2
    `,
    [limit, offset],
  );

  return result.rows.map(mapRowToGraphQL);
}

export async function getReadingById(pool, id, columns) {
  const selectClause = columnsToSelectClause(columns);
  const result = await pool.query(
    `
    SELECT ${selectClause}
    FROM sensor_readings
    WHERE id = $1
    `,
    [id],
  );

  return mapRowToGraphQL(result.rows[0]);
}

export async function createReading(pool, input, columns) {
  const dbInput = mapCreateInputToDb(input);
  const values = [
    dbInput.timestamp,
    dbInput.device_id,
    dbInput.location,
    dbInput.crop_type,
    dbInput.season,
    dbInput.temperature,
    dbInput.humidity,
    dbInput.rainfall,
    dbInput.soil_moisture,
    dbInput.soil_ph,
    dbInput.light_intensity,
    dbInput.fertilizer_used,
    dbInput.irrigation_needed,
    dbInput.crop_health,
    dbInput.yield_estimate,
    dbInput.pest_risk,
    dbInput.anomaly_flag,
  ];

  const selectClause = columnsToSelectClause(columns);
  const result = await pool.query(
    `
    INSERT INTO sensor_readings (${INSERT_COLUMNS})
    VALUES (${buildInsertValues()})
    RETURNING ${selectClause}
    `,
    values,
  );

  return mapRowToGraphQL(result.rows[0]);
}

export async function updateReading(pool, id, input, columns) {
  const patch = mapUpdateInputToDb(input);
  const patchColumns = Object.keys(patch);

  if (patchColumns.length === 0) {
    throw new Error("No fields to update");
  }

  const setClauses = patchColumns.map((column, index) => `${column} = $${index + 1}`);
  const values = patchColumns.map((column) => patch[column]);
  values.push(id);

  const selectClause = columnsToSelectClause(columns);
  const result = await pool.query(
    `
    UPDATE sensor_readings SET
      ${setClauses.join(", ")}
    WHERE id = $${values.length}
    RETURNING ${selectClause}
    `,
    values,
  );

  return mapRowToGraphQL(result.rows[0]);
}

export async function deleteReading(pool, id) {
  const result = await pool.query("DELETE FROM sensor_readings WHERE id = $1", [id]);
  return result.rowCount > 0;
}
