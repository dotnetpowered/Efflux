using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Efflux.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Console;
using Efflux.Pump.Commands;

namespace Efflux.Pump
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<bool>(
                    "--verbose",
                    "Verbose logging")
            };
            rootCommand.Description = "Efflux Pump";

            var publishCommand = new Command("publish") {
                new Option<string>(
                    "--topic",
                    description: "Topic to publish message"),
                new Argument("arg1")
            };
            rootCommand.AddCommand(publishCommand);
            publishCommand.Handler = CommandHandler.Create<string, bool, string>(async (topic, verbose, arg1) =>
            {
                ServiceProvider serviceProvider = AddServices(verbose);
                var cmd = serviceProvider.GetService<PublishCommand>();
                await cmd.PublishMessage(topic, arg1, verbose);
            });

            var pullCommand = new Command("consume") {
                new Option<string>(
                    "--topic",
                    description: "Topic to consume message from"),
                new Option<string>(
                    "--consumer",
                    description: "Name of consumer"),
            };
            rootCommand.AddCommand(pullCommand);
            pullCommand.Handler = CommandHandler.Create<string, string, bool>(async (topic, consumer, verbose) =>
            {
                ServiceProvider serviceProvider = AddServices(verbose);
                var cmd = serviceProvider.GetService<ConsumeCommand>();
                await cmd.ConsumeMessage(topic, consumer, verbose);
            });

      
            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        private static ServiceProvider AddServices(bool verbose)
        {
            //setup our DI
            return new ServiceCollection()
                .AddLogging(loggingBuilder =>
                {
                    if (verbose)
                        loggingBuilder.AddConsole();
                })
                .AddTransient<ITopicFactory, TopicStreamFactory>()
                .AddSingleton<PublishCommand>()
                .AddSingleton<ConsumeCommand>()
                .BuildServiceProvider();
        }
    }
}
