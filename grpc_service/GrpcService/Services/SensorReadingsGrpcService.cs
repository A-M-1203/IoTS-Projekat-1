using Grpc.Core;
using GrpcService.Mapping;
using GrpcService.Protos;
using GrpcService.Repositories;
using Models = GrpcService.Models;

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

        Models.SensorReadingInput input;
        try
        {
            input = SensorReadingMapper.FromProto(request.Reading);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }

        var created = await _repository.CreateAsync(input, context.CancellationToken);
        _logger.LogInformation("Created sensor reading with id={Id}", created.Id);
        return SensorReadingMapper.ToProto(created);
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

        Models.SensorReadingInput input;
        try
        {
            input = SensorReadingMapper.FromProto(request.Reading);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }

        var updated = await _repository.UpdateAsync(request.Id, input, context.CancellationToken);
        if (updated is null)
        {
            throw NotFound(request.Id);
        }

        _logger.LogInformation("Updated sensor reading with id={Id}", request.Id);
        return SensorReadingMapper.ToProto(updated);
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
