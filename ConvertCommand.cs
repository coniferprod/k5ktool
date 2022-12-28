using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K5KTool
{
	public class ConvertCommand
	{
        private List<byte> Data;
        private ConvertOptions Options;

        private string Percentage(int part, int whole)
        {
            var pct = 100 * ((float)part) / ((float)whole);
            return $"{pct}%";
        }


        public ConvertCommand(ConvertOptions opts)
        {
            this.Options = opts;
        }

        public int Convert()
        {
            string fileName = this.Options.FileName;
            string namePart = new DirectoryInfo(fileName).Name;

            byte[] fileData = File.ReadAllBytes(fileName);
            this.Data = new List<byte>(fileData);
            Console.WriteLine($"Source file: {this.Options.FileName}");

            var extensionPart = new DirectoryInfo(fileName).Extension;
            Console.WriteLine($"Extension part = '{extensionPart}'");

            // The following conversions are supported:
            // - KAA to KA1: many native patch files from one native bank
            // - KAA to SYX: one native bank to System Exclusive
            // - KA1 to SYX: one native patch to System Exclusive

            if (this.Options.Input.Equals("kaa"))
            {
                if (!extensionPart.ToLower().Equals(".kaa"))
                {
                    Console.WriteLine("File extension should be .KAA");
                    return -1;
                }

                if (this.Options.Output.Equals("ka1"))
                {
                    Console.WriteLine("Converting KAA to KA1");
                }
                else if (this.Options.Output.Equals("syx"))
                {
                    Console.WriteLine("Converting KAA to SYX");
                }
                else
                {
                    Console.WriteLine($"Conversion from KAA to '{this.Options.Output}' is not supported");
                    return -1;
                }

                var bank = new Bank(new List<byte>(fileData));
                var finalPatches = bank.Patches.OrderBy(p => p.Index).ToList();
                var totalSize = finalPatches.Select(x => x.Size).Sum();

                Console.WriteLine($"'{namePart}' contains {finalPatches.Count} patches using {totalSize} bytes ({Percentage(totalSize, Bank.POOL_SIZE)} of memory).");
                Console.WriteLine($"{Bank.POOL_SIZE - totalSize} bytes ({Percentage(Bank.POOL_SIZE - totalSize, Bank.POOL_SIZE)} of memory) free.");
                Console.WriteLine($"Base offset = {bank.BaseOffset:X8}. Patches:");

                var index = 0;
                foreach (var patch in finalPatches)
                {
                    var addCount = 0;
                    var pcmCount = 0;
                    for (var sourceIndex = 0; sourceIndex < patch.SourceCount; sourceIndex++)
                    {
                        if (patch.SourceOffsets[sourceIndex] != 0)
                        {
                            addCount++;
                        }
                        else
                        {
                            pcmCount++;
                        }
                    }
                    var sourceStr = new StringBuilder("");
                    if (addCount != 0)
                    {
                        sourceStr.Append($"{addCount}ADD");
                    }
                    if (addCount != 0 && pcmCount != 0)
                    {
                        sourceStr.Append(" ");
                    }
                    if (pcmCount != 0)
                    {
                        sourceStr.Append($"{pcmCount}PCM");
                    }
                    Console.WriteLine($"{index + 1} {patch.Name} {sourceStr} {patch.Size} {patch.ToneOffset} {patch.Padding}");
                    index++;
                }

                // Create destination directory if necessary
                try
                {
                    if (!Directory.Exists(this.Options.DirectoryName))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(this.Options.DirectoryName);
                        Console.WriteLine($"Destination directory '{this.Options.DirectoryName}' created");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to create destination director, error: {0}", e.ToString());
                    return -1;
                }
                finally { }

                if (this.Options.Output.Equals("ka1"))
                {
                    index = 0;
                    foreach (var patch in finalPatches)
                    {
                        var outputFileName = Path.Combine(this.Options.DirectoryName, $"{patch.Name}.ka1");
                        var outputFileData = bank.DataPool.GetRange(patch.ToneOffset, patch.Size).ToArray();
                        Console.WriteLine($"Writing {outputFileData.Length} bytes to '{outputFileName}'");
                        File.WriteAllBytes(outputFileName, outputFileData);
                    }
                }
            }
            else if (this.Options.Input.Equals("ka1"))
            {
                if (!extensionPart.ToLower().Equals(".ka1"))
                {
                    Console.WriteLine("File extension should be .KA1");
                    return -1;
                }

                if (this.Options.Output.Equals("syx"))
                {
                    Console.WriteLine("Converting KA1 to SYX");
                }
                else
                {
                    Console.WriteLine("Conversion from KA1 to '{this.Options.Output}' is not supported");
                    return -1;
                }
            }

            return 0;
        }
	}
}
