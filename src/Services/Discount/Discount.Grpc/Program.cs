using Discount.Grpc.Repositories.Interfaces;
using Serilog;
using OpenTelemetry.Shared;
using Discount.Grpc.Repositories;
using Discount.Grpc.Extensions;
using Common.Shared;
using Logging.Shared;
using Discount.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Logging.Shared.Logging.ConfigureLogging);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682



builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddAutoMapper(typeof(Program));
    
builder.Services.AddOpenTelemetryExt(builder.Configuration);


// Add services to the container.
builder.Services.AddGrpc();



var app = builder.Build();
app.MigrateDatabase<Program>();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
// For observability
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
//app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.Run();
