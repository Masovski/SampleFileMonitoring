using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

using SampleFileMonitoring.Common;
using SampleFileMonitoring.Common.Interfaces;
using SampleFileMonitoring.Common.Interfaces.Services;
using SampleFileMonitoring.Services;
using SampleFileMonitoring.Services.Monitoring;

namespace SampleFileMonitoring.Core
{
    /// <summary>
    /// Contains application-specific settings related to the monitoring agent functionality.
    /// </summary>
    public class Configuration
    {
        public static SystemPerformanceOptions SystemPerformance { get; private set; }
        public static ProcessingEndpointsOptions ProcessingEndpoints { get; private set; }
        public static MonitoringOptions Monitoring { get; private set; }

        /// <summary>
        /// Sets up the service collection with configurations and registers necessary services.
        /// </summary>
        /// <param name="serviceCollection">The service collection to register services to.</param>
        /// <param name="config">The configuration root that provides application configuration settings.</param>
        /// <remarks>
        /// This method configures application settings from the provided configuration root and then registers services to the given service collection.
        /// </remarks>

        public static void Setup(ServiceCollection serviceCollection, IConfigurationRoot config)
        {
            Configure(config);
            RegisterServices(serviceCollection);
        }

        /// <summary>
        /// Configures the application settings based on the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration source that provides application configuration settings.</param>
        /// <remarks>
        /// This method binds configuration sections to specific option classes for easier access throughout the application.
        /// </remarks>
        public static void Configure(IConfiguration configuration)
        {
            SystemPerformance = Bind<SystemPerformanceOptions>(configuration);
            ProcessingEndpoints = Bind<ProcessingEndpointsOptions>(configuration);
            Monitoring = Bind<MonitoringOptions>(configuration);
        }

        /// <summary>
        /// Registers the required services and their configurations to the provided service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> where services will be registered.</param>
        /// <returns>The <see cref="IServiceCollection"/> with the added services.</returns>
        /// <remarks>
        /// This method sets up the services related to HTTP communication, file monitoring, system performance, and other related functionalities.
        /// </remarks>
        public static IServiceCollection RegisterServices(IServiceCollection services)
        {
            services
                .AddHttpClient<IProcessingService, ProcessingService>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(GetHttpRetryPolicy())
                    .AddPolicyHandler(GetRateLimitPolicy());

            services
                .AddTransient<IMonitoringAgent, MonitoringAgent>()
                .AddSingleton<IFileMonitorFactory, FileMonitorFactory>()
                .AddSingleton<IFileMonitoringService, FileMonitoringService>()
                .AddTransient<SystemPerformanceSettings>((x) =>
                {
                    return new()
                    {
                        ConcurrencyDelayMultiplier = SystemPerformance.ConcurrencyDelayMultiplier,
                        MaxConcurrentIORequests = SystemPerformance.MaxConcurrentIORequests,
                        MaxDegreeOfParallelism = SystemPerformance.MaxDegreeOfParallelism
                    };
                })
                .AddSingleton<ISystemService, SystemService>()
                .AddTransient<ProcessingEndpointSettings>((x) =>
                {
                    return new()
                    {
                        BaseAddress = ProcessingEndpoints.BaseAddress,
                        RegisterFileEndpointPath = ProcessingEndpoints.RelativeRegisterFileEndpoint,
                        RegisterSystemEndpointPath = ProcessingEndpoints.RelativeRegisterSystemEndpointPath
                    };
                })
                .AddSingleton<IProcessingService, ProcessingService>();

            return services;
        }

        /// <summary>
        /// Binds a specific configuration section to an options object of type <typeparamref name="TOptions"/>.
        /// </summary>
        /// <typeparam name="TOptions">The type of options object to bind to.</typeparam>
        /// <param name="configuration">The configuration source to bind from.</param>
        /// <returns>A populated options object of type <typeparamref name="TOptions"/>.</returns>
        /// <remarks>
        /// The method assumes that the configuration section name corresponds to the name of the type <typeparamref name="TOptions"/>.
        /// </remarks>
        private static TOptions Bind<TOptions>(IConfiguration configuration) where TOptions : new()
        {
            TOptions options = new();

            configuration
                .GetSection(typeof(TOptions).Name)
                .Bind(options);

            return options;
        }

        /// <summary>
        /// Gets a rate-limiting policy for HTTP requests.
        /// </summary>
        /// <remarks>
        /// This policy limits the rate of HTTP requests to 5 per second with a burst capability of 3.
        /// </remarks>
        /// <returns>Returns a rate-limiting policy of type <see cref="IAsyncPolicy{HttpResponseMessage}"/>.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRateLimitPolicy()
        {
            var rateLimit = Policy.RateLimitAsync<HttpResponseMessage>(
                numberOfExecutions: 5,
                perTimeSpan: TimeSpan.FromMilliseconds(1000),
                maxBurst: 3);

            return rateLimit;
        }

        /// <summary>
        /// Gets a retry policy for HTTP requests.
        /// </summary>
        /// <remarks>
        /// This policy retries HTTP requests up to 5 times using a decorrelated jitter backoff strategy.
        /// It handles transient HTTP errors and treats HTTP 404 (Not Found) as a condition that should be retried.
        /// </remarks>
        /// <returns>Returns a retry policy of type <see cref="IAsyncPolicy{HttpResponseMessage}"/>.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(1),
                retryCount: 5);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }
    }
}
