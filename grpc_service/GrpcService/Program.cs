using GrpcService.Repositories;
using GrpcService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();
builder.Services.AddSingleton<SensorReadingRepository>();

var app = builder.Build();

app.MapGrpcService<SensorReadingsGrpcService>();
app.MapGet("/", () => "IoTS Projekat 1 gRPC servis za sensor_readings. Koristite gRPC klijent na ovom hostu.");

app.Run();
