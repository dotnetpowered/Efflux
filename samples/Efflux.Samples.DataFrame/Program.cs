using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Threading.Tasks;
using Efflux.Stream;

namespace Efflux.Samples.DataFrame
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole(options => {
                        options.FormatterName = ConsoleFormatterNames.Systemd;
                    });
                })
                .AddTransient<ITopicFactory, TopicStreamFactory>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            Console.ForegroundColor = ConsoleColor.Yellow;

            var factory = serviceProvider.GetService<ITopicFactory>();
            var topic = factory.OpenTopic("topic2");

            await DataFrameSample.Demo(topic);
        }
    }
}
