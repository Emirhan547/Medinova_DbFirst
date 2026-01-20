using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace Medinova
{
    public static class LoggingConfig
    {
        private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

        public static void ConfigureLogging()
        {
            var minimumLevel = GetSetting("Serilog:MinimumLevel", "Information");
            var elasticUri = GetSetting("Serilog:ElasticsearchUri", string.Empty);
            var indexFormat = GetSetting("Serilog:ElasticsearchIndex", "medinova-logs-{0:yyyy.MM.dd}");
            var environment = GetSetting("Serilog:Environment", Environment.GetEnvironmentVariable("ASPNET_ENV") ?? "Production");
            var filePathSetting = GetSetting("Serilog:FilePath", "~/App_Data/logs/medinova-.log");

            var level = ParseLogLevel(minimumLevel);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Data", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Data.Entity", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Medinova")
                .Enrich.WithProperty("Environment", environment);

            var filePath = ResolveFilePath(filePathSetting);
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                loggerConfiguration = loggerConfiguration.WriteTo.File(
                    filePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: OutputTemplate);
            }

            if (Uri.TryCreate(elasticUri, UriKind.Absolute, out var elasticUriValue))
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUriValue)
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = indexFormat,
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
                });
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static string ResolveFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                return HostingEnvironment.MapPath(path);
            }

            return path;
        }

        private static LogEventLevel ParseLogLevel(string level)
        {
            if (Enum.TryParse(level, true, out LogEventLevel parsed))
            {
                return parsed;
            }

            return LogEventLevel.Information;
        }

        private static string GetSetting(string key, string defaultValue)
        {
            var envKey = key.Replace(":", "__");
            var value = Environment.GetEnvironmentVariable(envKey)
                ?? Environment.GetEnvironmentVariable(key)
                ?? ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}