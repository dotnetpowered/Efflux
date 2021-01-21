using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Efflux.Stream;

namespace Efflux.Samples.FileSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole(options => {
                        options.Format = ConsoleLoggerFormat.Systemd;
                    });
                })
                .AddTransient<ITopicFactory, TopicStreamFactory>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Efflux FileSystem Sample");

            var factory = serviceProvider.GetService<ITopicFactory>();
            var topic = factory.OpenTopic("fs-topic");

            FileSystemSample.Demo(topic);
        }
    }
}
