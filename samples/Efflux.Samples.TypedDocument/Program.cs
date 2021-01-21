using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Efflux.Stream;
using System.Threading.Tasks;

namespace Efflux.Samples.TypedDocument
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
                        options.Format = ConsoleLoggerFormat.Systemd;
                    });
                })
                .AddTransient<ITopicFactory, TopicStreamFactory>()
                .AddTransient<TypedDocumentSample>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            Console.ForegroundColor = ConsoleColor.Yellow;

            var sample = serviceProvider.GetService<TypedDocumentSample>();
            await sample.TypedDocumentDemo();
        }
    }
}
