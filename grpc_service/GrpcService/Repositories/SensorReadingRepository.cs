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

    public async Task<SensorReading> CreateAsync(
        SensorReadingCreateInput input,
        CancellationToken cancellationToken = default)
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
        AddCreateParameters(command, input);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("INSERT did not return a row.");
        }

        return MapReading(reader);
    }

    public async Task<SensorReading?> UpdateAsync(
        long id,
        SensorReadingPatch patch,
        CancellationToken cancellationToken = default)
    {
        if (patch.Fields.Count == 0)
        {
            throw new ArgumentException("No fields to update");
        }

        var setClauses = new List<string>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand { Connection = connection };
        var index = 1;
        foreach (var (column, value) in patch.Fields)
        {
            var parameterName = $"p{index}";
            setClauses.Add($"{column} = @{parameterName}");
            command.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
            index++;
        }

        command.Parameters.AddWithValue("id", id);
        command.CommandText = $"""
            UPDATE sensor_readings SET
                {string.Join(", ", setClauses)}
            WHERE id = @id
            RETURNING {Columns}
            """;

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

    private static void AddCreateParameters(NpgsqlCommand command, SensorReadingCreateInput input)
    {
        command.Parameters.AddWithValue("timestamp", input.Timestamp);
        command.Parameters.AddWithValue("device_id", input.DeviceId);
        command.Parameters.AddWithValue("location", input.Location);
        command.Parameters.AddWithValue("crop_type", input.CropType);
        command.Parameters.AddWithValue("season", input.Season);
        command.Parameters.AddWithValue("temperature", input.Temperature);
        command.Parameters.AddWithValue("humidity", input.Humidity);
        command.Parameters.AddWithValue("rainfall", input.Rainfall);
        command.Parameters.AddWithValue("soil_moisture", input.SoilMoisture);
        command.Parameters.AddWithValue("soil_ph", input.SoilPh);
        command.Parameters.AddWithValue("light_intensity", input.LightIntensity);
        command.Parameters.AddWithValue("fertilizer_used", input.FertilizerUsed);
        command.Parameters.AddWithValue("irrigation_needed", input.IrrigationNeeded);
        command.Parameters.AddWithValue("crop_health", input.CropHealth);
        command.Parameters.AddWithValue("yield_estimate", input.YieldEstimate);
        command.Parameters.AddWithValue("pest_risk", input.PestRisk);
        command.Parameters.AddWithValue("anomaly_flag", input.AnomalyFlag);
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
