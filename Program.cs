using System;
using McMaster.Extensions.CommandLineUtils;

namespace k5ktool
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var commandArg = app.Argument("command", "The command to issue").IsRequired();

            var filenameOption = app.Option("-f|--filename <FILE>", "Name of the file to process", CommandOptionType.SingleValue).IsRequired();

            app.OnExecute(() =>
            {
                Console.WriteLine($"Command = {commandArg.Value}");
                Console.WriteLine("Filename = " + filenameOption.Value());
            });

            return app.Execute(args);
        }
    }
}
