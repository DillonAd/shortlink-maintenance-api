﻿using System;
using HexMaster.ShortLink.Maintenance;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using HexMaster.ShortLink.Core.Configuration;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]
namespace HexMaster.ShortLink.Maintenance
{
    public class Startup : FunctionsStartup
    {

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            
//            var currentConfig = builder.GetContext().Configuration;
            

            builder.ConfigurationBuilder.AddAzureAppConfiguration((options) =>
            {
                options.Connect(Environment.GetEnvironmentVariable("appConfigConnectionString"));
            });

        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();



            builder.Services.ConfigureCore(configuration);

            var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            builder.Services.AddApplicationInsightsTelemetry(x => { x.EnableAdaptiveSampling = false; });
            builder.Services.AddLogging(loggingBuilder =>
            {
                
                var logger = new LoggerConfiguration()
                    .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
                    .CreateLogger();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }
}


