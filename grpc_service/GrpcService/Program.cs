using GrpcService.Repositories;
using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<SensorReadingRepository>();

var app = builder.Build();

app.MapGrpcService<SensorReadingsGrpcService>();
app.MapGet("/", () => "IoTS Projekat 1 gRPC servis za sensor_readings. Koristite gRPC klijent na ovom hostu.");

app.Run();
