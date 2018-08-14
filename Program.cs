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

        const int MaxPatchCount = 128;  // the max number of patches in a bank

        const int MaxSourceCount = 6;  // the max number of sources in a patch

        public static uint ReadPointer(BinaryReader br)
        {
            var data = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToUInt32(data, 0);
        }

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
                        using (FileStream fs = File.OpenRead(filename))
                        {
                            using (BinaryReader binaryReader = new BinaryReader(fs))
                            {
                                var patchCount = 0;
                                for (int patchIndex = 0; patchIndex < MaxPatchCount; patchIndex++)
                                {
                                    Patch patch;
                                    patch.Index = patchIndex;
                                    patch.TonePointer = ReadPointer(binaryReader);
                                    patch.IsUsed = patch.TonePointer != 0;
                                    if (patch.IsUsed)
                                    {
                                        patchCount++;
                                    }

                                    patch.AdditiveKitCount = 0;
                                    for (int sourceIndex = 0; sourceIndex < MaxSourceCount; sourceIndex++)
                                    {
                                        Source source;
                                        source.AdditiveKitPointer = ReadPointer(binaryReader);
                                        source.IsAdditive = source.AdditiveKitPointer != 0;
                                        patch.AdditiveKitCount++;
                                    }

                                    Console.WriteLine($"{patch.Index} {patch.TonePointer}");
                                }
                                
                                Console.WriteLine($"{patchCount}");
                            }
                        }
                    }
                }

                return 0;
            });

            return app.Execute(args);
        }
    }
}
