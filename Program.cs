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

        const int POOL_SIZE = 0x20000;

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
                        /* 
                        var bank = engine.ReadBank(fileName);
                        var bytesUsed = bank.SortedTonePointer[bank.PatchCount].TonePointer;
                        var percentageUsed = bytesUsed * 100.0 / (float)POOL_SIZE;
                        Console.WriteLine($"{fileName} contains {bank.PatchCount} patches using {bytesUsed} bytes ({percentageUsed}% of memory)");
                        */
                        uint[] pointers = engine.ReadPointers(fileName);
                        int index = 0;
                        int i = 0;
                        for (i = 0; i < 128; i++)
                        {
                            Console.Write(string.Format("{0} {1:X6} -- ", i, pointers[index]));
                            index++;
                            for (int s = 0; s < 6; s++)
                            {
                                Console.Write(string.Format("S{0}={1:X6} ", s, pointers[index]));
                                index++;
                            }
                            Console.WriteLine();
                        }
                        var basePointer = pointers[index];
                        Console.WriteLine("{0:X6}", basePointer);

                        // Adjust the pointers
                        index = 0;
                        for (index = 0; index < pointers.Length - 1; index++)
                        {
                            pointers[index] -= basePointer;
                        }

                        Console.WriteLine("After adjusting pointers:");
                        index = 0;
                        for (i = 0; i < 128; i++)
                        {
                            Console.Write(string.Format("{0} {1:X6} -- ", i, pointers[index]));
                            index++;
                            for (int s = 0; s < 6; s++)
                            {
                                Console.Write(string.Format("S{0}={1:X6} ", s, pointers[index]));
                                index++;
                            }
                            Console.WriteLine();
                        }

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
