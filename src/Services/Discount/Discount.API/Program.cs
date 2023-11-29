using Common.Shared;
using Logging.Shared;
using Serilog;
using OpenTelemetry.Shared;
using Discount.API.Repositories.Interfaces;
using Discount.API.Repositories;
using Discount.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Logging.Shared.Logging.ConfigureLogging);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();



builder.Services.AddOpenTelemetryExt(builder.Configuration);
var app = builder.Build();
app.MigrateDatabase<Program>();

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
app.MapControllers();

app.Run();
