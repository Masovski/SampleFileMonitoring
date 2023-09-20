using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SampleFileMonitoring.Common.Interfaces;

namespace SampleFileMonitoring.ConsoleApp
{
    internal class Program
    {
        static IServiceProvider ServiceProvider { get; set; }
        static ISet<IMonitoringAgent> Agents { get; set; }

        static Program()
        {
            Agents = new HashSet<IMonitoringAgent>();
        }

        static void Main()
        {
            try
            {
                ConfigureServices();

                RunNewAgent(@"X:\Test");

                WaitUntilKeyPressed(ConsoleKey.Escape);
            }
            finally
            {
                foreach (var agent in Agents)
                {
                    agent.Dispose();
                }
            }
        }

        static void RunNewAgent(string path = null)
        {
            var agent = GetAgent();
            Agents.Add(agent);

            if (string.IsNullOrEmpty(path))
                agent.RunAsync();
            else
                agent.RunAsync(path);
        }

        static void WaitUntilKeyPressed(ConsoleKey key)
        {
            while (Console.ReadKey(true).Key != key)
            {
                // Stopping the program from completing...
            }
        }

        static IMonitoringAgent GetAgent()
        {
            return ServiceProvider.GetRequiredService<IMonitoringAgent>();
        }

        static void ConfigureServices()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            var serviceCollection = new ServiceCollection();

            Core.Configuration.Setup(serviceCollection, config);

            serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));
                builder.AddConsole();
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
