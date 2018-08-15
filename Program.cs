using System;
using System.IO;
using System.Collections.Generic;
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

            var fileNameOption = app.Option("-f|--filename <FILE>", "Name of the file to process", CommandOptionType.SingleValue).IsRequired();

            app.OnExecute(() =>
            {
                var command = commandArg.Value;
                Console.WriteLine($"Command = {command}");

                if (command.Equals("list"))
                {
                    var fileName = fileNameOption.Value();
                    Console.WriteLine("filename = " + fileName);

                    Engine engine = new Engine();

                    if (File.Exists(fileName))
                    {
                        var bank = engine.GetBank(fileName);
                    }
                    else
                    {
                        Console.WriteLine($"File {fileName} not found");
                        return -1;
                    }
                }

                return 0;
            });

            return app.Execute(args);
        }
    }
}
