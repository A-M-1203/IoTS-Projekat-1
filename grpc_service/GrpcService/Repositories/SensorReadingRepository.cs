using GrpcService.Models;
using Npgsql;

namespace GrpcService.Repositories;

public class SensorReadingRepository
{
    private const string Columns = """
        id, timestamp, device_id, location, crop_type, season,
        temperature, humidity, rainfall, soil_moisture, soil_ph,
        light_intensity, fertilizer_used, irrigation_needed,
        crop_health, yield_estimate, pest_risk, anomaly_flag
        """;

    private const string InsertColumns = """
        timestamp, device_id, location, crop_type, season,
        temperature, humidity, rainfall, soil_moisture, soil_ph,
        light_intensity, fertilizer_used, irrigation_needed,
        crop_health, yield_estimate, pest_risk, anomaly_flag
        """;

    private readonly string _connectionString;

    public SensorReadingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    public async Task<IReadOnlyList<SensorReading>> ListAsync(int limit, int offset, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"""
            SELECT {Columns}
            FROM sensor_readings
            ORDER BY id
            LIMIT @limit OFFSET @offset
            """,
            connection);
        command.Parameters.AddWithValue("limit", limit);
        command.Parameters.AddWithValue("offset", offset);

        var results = new List<SensorReading>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapReading(reader));
        }

        return results;
    }

    public async Task<SensorReading?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"""
            SELECT {Columns}
            FROM sensor_readings
            WHERE id = @id
            """,
            connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapReading(reader) : null;
    }

    public async Task<SensorReading> CreateAsync(SensorReadingInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"""
            INSERT INTO sensor_readings ({InsertColumns})
            VALUES (
                @timestamp, @device_id, @location, @crop_type, @season,
                @temperature, @humidity, @rainfall, @soil_moisture, @soil_ph,
                @light_intensity, @fertilizer_used, @irrigation_needed,
                @crop_health, @yield_estimate, @pest_risk, @anomaly_flag
            )
            RETURNING {Columns}
            """,
            connection);
        AddInputParameters(command, input);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("INSERT did not return a row.");
        }

        return MapReading(reader);
    }

    public async Task<SensorReading?> UpdateAsync(long id, SensorReadingInput input, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            $"""
            UPDATE sensor_readings SET
                timestamp = @timestamp,
                device_id = @device_id,
                location = @location,
                crop_type = @crop_type,
                season = @season,
                temperature = @temperature,
                humidity = @humidity,
                rainfall = @rainfall,
                soil_moisture = @soil_moisture,
                soil_ph = @soil_ph,
                light_intensity = @light_intensity,
                fertilizer_used = @fertilizer_used,
                irrigation_needed = @irrigation_needed,
                crop_health = @crop_health,
                yield_estimate = @yield_estimate,
                pest_risk = @pest_risk,
                anomaly_flag = @anomaly_flag
            WHERE id = @id
            RETURNING {Columns}
            """,
            connection);
        AddInputParameters(command, input);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapReading(reader) : null;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            "DELETE FROM sensor_readings WHERE id = @id",
            connection);
        command.Parameters.AddWithValue("id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    private static void AddInputParameters(NpgsqlCommand command, SensorReadingInput input)
    {
        command.Parameters.AddWithValue("timestamp", input.Timestamp);
        command.Parameters.AddWithValue("device_id", input.DeviceId);
        command.Parameters.AddWithValue("location", (object?)input.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("crop_type", (object?)input.CropType ?? DBNull.Value);
        command.Parameters.AddWithValue("season", (object?)input.Season ?? DBNull.Value);
        command.Parameters.AddWithValue("temperature", (object?)input.Temperature ?? DBNull.Value);
        command.Parameters.AddWithValue("humidity", (object?)input.Humidity ?? DBNull.Value);
        command.Parameters.AddWithValue("rainfall", (object?)input.Rainfall ?? DBNull.Value);
        command.Parameters.AddWithValue("soil_moisture", (object?)input.SoilMoisture ?? DBNull.Value);
        command.Parameters.AddWithValue("soil_ph", (object?)input.SoilPh ?? DBNull.Value);
        command.Parameters.AddWithValue("light_intensity", (object?)input.LightIntensity ?? DBNull.Value);
        command.Parameters.AddWithValue("fertilizer_used", (object?)input.FertilizerUsed ?? DBNull.Value);
        command.Parameters.AddWithValue("irrigation_needed", (object?)input.IrrigationNeeded ?? DBNull.Value);
        command.Parameters.AddWithValue("crop_health", (object?)input.CropHealth ?? DBNull.Value);
        command.Parameters.AddWithValue("yield_estimate", (object?)input.YieldEstimate ?? DBNull.Value);
        command.Parameters.AddWithValue("pest_risk", (object?)input.PestRisk ?? DBNull.Value);
        command.Parameters.AddWithValue("anomaly_flag", (object?)input.AnomalyFlag ?? DBNull.Value);
    }

    private static SensorReading MapReading(NpgsqlDataReader reader)
    {
        return new SensorReading
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Timestamp = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("timestamp")),
            DeviceId = reader.GetString(reader.GetOrdinal("device_id")),
            Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString(reader.GetOrdinal("location")),
            CropType = reader.IsDBNull(reader.GetOrdinal("crop_type")) ? null : reader.GetString(reader.GetOrdinal("crop_type")),
            Season = reader.IsDBNull(reader.GetOrdinal("season")) ? null : reader.GetString(reader.GetOrdinal("season")),
            Temperature = reader.IsDBNull(reader.GetOrdinal("temperature")) ? null : reader.GetDecimal(reader.GetOrdinal("temperature")),
            Humidity = reader.IsDBNull(reader.GetOrdinal("humidity")) ? null : reader.GetDecimal(reader.GetOrdinal("humidity")),
            Rainfall = reader.IsDBNull(reader.GetOrdinal("rainfall")) ? null : reader.GetDecimal(reader.GetOrdinal("rainfall")),
            SoilMoisture = reader.IsDBNull(reader.GetOrdinal("soil_moisture")) ? null : reader.GetDecimal(reader.GetOrdinal("soil_moisture")),
            SoilPh = reader.IsDBNull(reader.GetOrdinal("soil_ph")) ? null : reader.GetDecimal(reader.GetOrdinal("soil_ph")),
            LightIntensity = reader.IsDBNull(reader.GetOrdinal("light_intensity")) ? null : reader.GetDecimal(reader.GetOrdinal("light_intensity")),
            FertilizerUsed = reader.IsDBNull(reader.GetOrdinal("fertilizer_used")) ? null : reader.GetDecimal(reader.GetOrdinal("fertilizer_used")),
            IrrigationNeeded = reader.IsDBNull(reader.GetOrdinal("irrigation_needed")) ? null : reader.GetBoolean(reader.GetOrdinal("irrigation_needed")),
            CropHealth = reader.IsDBNull(reader.GetOrdinal("crop_health")) ? null : reader.GetString(reader.GetOrdinal("crop_health")),
            YieldEstimate = reader.IsDBNull(reader.GetOrdinal("yield_estimate")) ? null : reader.GetDecimal(reader.GetOrdinal("yield_estimate")),
            PestRisk = reader.IsDBNull(reader.GetOrdinal("pest_risk")) ? null : reader.GetString(reader.GetOrdinal("pest_risk")),
            AnomalyFlag = reader.IsDBNull(reader.GetOrdinal("anomaly_flag")) ? null : reader.GetBoolean(reader.GetOrdinal("anomaly_flag")),
        };
    }
}
