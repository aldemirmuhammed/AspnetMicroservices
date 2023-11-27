using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Shared
{
    public static class Logging
    {
        public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (builderContext, loggerConfiguration) =>
        {

            var environment = builderContext.HostingEnvironment;
            loggerConfiguration.Enrich.FromLogContext()
            .ReadFrom.Configuration(builderContext.Configuration)
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Env", environment.EnvironmentName)
            .Enrich.WithProperty("AppName", environment.ApplicationName);

            var elasticSearcBaseUrl = builderContext.Configuration.GetSection("ElasticSearch")["BaseUrl"];
            var userName = builderContext.Configuration.GetSection("ElasticSearch")["Username"];
            var password = builderContext.Configuration.GetSection("ElasticSearch")["Password"];
            var indexName = builderContext.Configuration.GetSection("ElasticSearch")["IndexName"];


            loggerConfiguration.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticSearcBaseUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"{indexName}-{environment.EnvironmentName}-logs-"+ "{0:yyy.MM.dd}",
                ModifyConnectionSettings = x => x.BasicAuthentication(userName, password),
                CustomFormatter = new ElasticsearchJsonFormatter()
            }) ;

        };

    }
}
