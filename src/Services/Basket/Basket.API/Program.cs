using Common.Shared;
using Logging.Shared;
using Serilog;
using OpenTelemetry.Shared;
using Microsoft.Extensions.Configuration;
using Basket.API.Repositories.Interfaces;
using Basket.API.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Logging.Shared.Logging.ConfigureLogging);

// Add services to the container.

// Redis Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
});

builder.Services.AddScoped<IBasketRepository,BasketRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryExt(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// For observability
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseExceptionMiddleware();
app.MapControllers();

app.Run();
