using Grpc.Core;
using GrpcService.Mapping;
using GrpcService.Protos;
using GrpcService.Repositories;

namespace GrpcService.Services;

public class SensorReadingsGrpcService : SensorReadingService.SensorReadingServiceBase
{
    private const int DefaultLimit = 100;
    private const int MaxLimit = 1000;

    private readonly SensorReadingRepository _repository;
    private readonly ILogger<SensorReadingsGrpcService> _logger;

    public SensorReadingsGrpcService(
        SensorReadingRepository repository,
        ILogger<SensorReadingsGrpcService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<ListSensorReadingsResponse> ListSensorReadings(
        ListSensorReadingsRequest request,
        ServerCallContext context)
    {
        var limit = request.Limit == 0 ? DefaultLimit : request.Limit;
        var offset = request.Offset;

        if (limit < 1 || limit > MaxLimit)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, $"limit must be between 1 and {MaxLimit}."));
        }

        if (offset < 0)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "offset must be greater than or equal to 0."));
        }

        var readings = await _repository.ListAsync(limit, offset, context.CancellationToken);
        var response = new ListSensorReadingsResponse();
        response.Readings.AddRange(readings.Select(SensorReadingMapper.ToProto));
        return response;
    }

    public override async Task<Protos.SensorReading> GetSensorReading(
        GetSensorReadingRequest request,
        ServerCallContext context)
    {
        var reading = await _repository.GetByIdAsync(request.Id, context.CancellationToken);
        if (reading is null)
        {
            throw NotFound(request.Id);
        }

        return SensorReadingMapper.ToProto(reading);
    }

    public override async Task<Protos.SensorReading> CreateSensorReading(
        CreateSensorReadingRequest request,
        ServerCallContext context)
    {
        if (request.Reading is null)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "reading is required."));
        }

        try
        {
            var input = SensorReadingMapper.FromCreateProto(request.Reading);
            var created = await _repository.CreateAsync(input, context.CancellationToken);
            _logger.LogInformation("Created sensor reading with id={Id}", created.Id);
            return SensorReadingMapper.ToProto(created);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<Protos.SensorReading> UpdateSensorReading(
        UpdateSensorReadingRequest request,
        ServerCallContext context)
    {
        if (request.Reading is null)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "reading is required."));
        }

        var patch = SensorReadingMapper.FromUpdateProto(request.Reading);
        if (patch.Fields.Count == 0)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "At least one field must be provided for update."));
        }

        try
        {
            var updated = await _repository.UpdateAsync(request.Id, patch, context.CancellationToken);
            if (updated is null)
            {
                throw NotFound(request.Id);
            }

            _logger.LogInformation("Updated sensor reading with id={Id}", request.Id);
            return SensorReadingMapper.ToProto(updated);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<DeleteSensorReadingResponse> DeleteSensorReading(
        DeleteSensorReadingRequest request,
        ServerCallContext context)
    {
        var deleted = await _repository.DeleteAsync(request.Id, context.CancellationToken);
        if (!deleted)
        {
            throw NotFound(request.Id);
        }

        _logger.LogInformation("Deleted sensor reading with id={Id}", request.Id);
        return new DeleteSensorReadingResponse { Success = true };
    }

    private static RpcException NotFound(long id) =>
        new(new Status(StatusCode.NotFound, $"Očitavanje sa id={id} nije pronađeno."));
}
