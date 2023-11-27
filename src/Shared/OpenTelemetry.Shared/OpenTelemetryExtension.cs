using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTelemetry.Shared
{
    public static class OpenTelemetryExtension
    {
        public static void AddOpenTelemetryExt(this IServiceCollection serviceCollection, IConfiguration configuration)
        {

            serviceCollection.Configure<OpenTelemetryConstant>(configuration.GetSection("OpenTelemetry"));

            var openTelemetry = (configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstant>())!;
            ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetry.ActivitySourceName);

            serviceCollection.AddOpenTelemetry().WithTracing(options =>
            {
                options.AddSource(openTelemetry.ActivitySourceName)
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .ConfigureResource(resources =>
                {
                    resources.AddService(openTelemetry.ServiceName, serviceVersion: openTelemetry.ServiceVersion);
                });
                options.AddAspNetCoreInstrumentation(aspnetcoreoptions =>
                {
                    // add route filter as example api/controller
                    aspnetcoreoptions.Filter = (context) =>
                    {
                        if (!string.IsNullOrEmpty(context.Request.Path.Value))
                            return context.Request.Path.Value!.Contains("api", StringComparison.InvariantCulture);
                        return false;
                    };

                    // Detail exception 
                    aspnetcoreoptions.RecordException = true;
                    aspnetcoreoptions.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exception.message", exception.Message);
                        activity.SetTag("exception.innerException", exception.InnerException);
                        activity.SetTag("exception.stackTrace", exception.StackTrace);
                    };
                });
                options.AddEntityFrameworkCoreInstrumentation(aspnetcoreoptions =>
                {
                    aspnetcoreoptions.SetDbStatementForText = true;
                    aspnetcoreoptions.SetDbStatementForStoredProcedure = true;
                    aspnetcoreoptions.EnrichWithIDbCommand = (activity, dbCommand) =>
                    {
                        activity.SetTag("db.commandText", dbCommand.CommandText);
                        activity.SetTag("db.connection", dbCommand.Connection?.ConnectionString);
                        activity.SetTag("db.commandType", dbCommand.CommandType);
                        activity.SetTag("db.commandTimeout", dbCommand.CommandTimeout);
                    };
                });
                options.AddHttpClientInstrumentation(httpOptions =>
                {
                    httpOptions.FilterHttpRequestMessage = (request) =>
                    {
                            return !request.RequestUri!.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);
                    };

                    httpOptions.EnrichWithHttpRequestMessage = async (activity, request) =>
                    {
                        var requestContent = "empty";
                        if (request.Content != null)
                        {
                            requestContent = await request.Content.ReadAsStringAsync();
                            activity.SetTag("http.request.body", requestContent);
                        }
                    };
                    httpOptions.EnrichWithHttpResponseMessage =async (activity, response) =>
                    {
                        if (response.Content != null)
                        {
                            activity.SetTag("http.response.body",await response.Content.ReadAsStringAsync());
                        }
                    };
                });
                options.AddRedisInstrumentation(redisOptions =>
                {
                    redisOptions.SetVerboseDatabaseStatements = true;
                });

                options.AddConsoleExporter();
                options.AddOtlpExporter();


            });

        }
    }
}
