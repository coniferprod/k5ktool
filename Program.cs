using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace k5ktool
{
    public class Program
    {
        // Single patch files have a ".ka1" or ".KA1" extension
        const string SingleExtension = "ka1";

        // Patch bank files have a ".kaa" or ".KAA" extension
        const string BankExtension = "kaa";

        // System exclusive files have a ".syx" extension
        const string SystemExtension = "syx";

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var commandArg = app.Argument("command", "The command to issue").IsRequired();

            var filenameOption = app.Option("-f|--filename <FILE>", "Name of the file to process", CommandOptionType.SingleValue).IsRequired();

            app.OnExecute(() =>
            {
                var command = commandArg.Value;
                Console.WriteLine($"Command = {command}");

                if (command.Equals("list"))
                {
                    var filename = filenameOption.Value();
                    Console.WriteLine("Filename = " + filename);

                    if (File.Exists(filename))
                    {
                        byte[] data = File.ReadAllBytes(filename);
                        Console.WriteLine("Read {0} bytes from file {1}", data.Length, filename);
                    }

                }

                return 0;
            });

            return app.Execute(args);
        }
    }
}
