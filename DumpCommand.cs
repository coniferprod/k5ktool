using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using KSynthLib.K5000;
using KSynthLib.Common;

namespace K5KTool
{
	public class DumpCommand
	{
		public byte Channel;
		public DumpHeader Header;
		public uint PatchNumber;

		private byte[] data;

        public DumpCommand(byte[] fileData)
		{
			this.data = new byte[fileData.Length];
			Array.Copy(fileData, this.data, fileData.Length);
			//Console.WriteLine($"this.data length = {this.data.Length} bytes");

			// header[0] must be 0xF0
			// header[1] must be 0x40 (Kawai ID)
			this.Channel = data[2];
			this.Header = new DumpHeader(data);
			this.PatchNumber = data[8];  // only meaningful for one single, one drum inst, and one combi
        }

		public int DumpPatches(string outputFormat)
		{
			var offset = 8;   // skip to the tone map

			byte[] buffer;
			(buffer, offset) = Util.GetNextBytes(this.data, offset, ToneMap.Size);
			var toneMap = new ToneMap(buffer);

			List<int> patchNumbers = new List<int>();

			var patchCount = 0;
			for (var i = 0; i < ToneMap.ToneCount; i++)
			{
				if (toneMap[i])
				{
					patchCount += 1;
					//Console.Write(i + 1);
					//Console.Write(" ");

					patchNumbers.Add(i);
				}
			}

			var minimumPatchSize = SingleCommonSettings.DataSize + 2 * KSynthLib.K5000.Source.DataSize;
			var totalPatchSize = 0;  // the total size of all the single patches
			var allPatchInfos = new List<PatchInfo>();
			var singlePatches = new List<SinglePatch>();
			foreach (var patchNumber in patchNumbers)
			{
				var startOffset = offset;  // save the current offset because we need to copy more bytes later

				var sizeToRead = Math.Max(minimumPatchSize, this.data.Length - offset);
				// We don't know yet how many bytes the patch is, but it is at least the minimum size
				(buffer, offset) = Util.GetNextBytes(this.data, offset, sizeToRead);
				// the offset has now been updated past the read size, so need to adjust it back later

				//Console.Error.WriteLine(Util.HexDump(buffer));
				//Console.Error.WriteLine($"checksum = {buffer[0]:X2}H");

				var patch = new SinglePatch(buffer);

				// Find out how many PCM and ADD sources
				var pcmCount = 0;
				var addCount = 0;
				foreach (var source in patch.Sources)
				{
					if (source.IsAdditive)
					{
						addCount += 1;
					}
					else
					{
						pcmCount += 1;
					}
				}

				// Figure out the total size of the single patch based on the counts
				var patchSize = 1 + SingleCommonSettings.DataSize  // includes the checksum
					+ patch.Sources.Length * KSynthLib.K5000.Source.DataSize  // all sources have this part
					+ addCount * AdditiveKit.DataSize;
				//Console.WriteLine($"{pcmCount}PCM {addCount}ADD size={patchSize} bytes");

				offset = startOffset;  // back up to the start of the patch data
				// Read the whole patch now that we know its size
				//Console.WriteLine($"About to read {patchSize} bytes starting from offset {offset:X4}h");
				(buffer, offset) = Util.GetNextBytes(this.data, offset, patchSize);

				totalPatchSize += patchSize;

				singlePatches.Add(patch);
			}

			foreach (var patch in singlePatches)
			{
				if (outputFormat.Equals("text"))
            	{
					Console.WriteLine(patch);
				}
				else if (outputFormat.Equals("json"))
				{
	                string jsonString = JsonConvert.SerializeObject(
                    	patch,
                    	Newtonsoft.Json.Formatting.Indented,
                    	new Newtonsoft.Json.Converters.StringEnumConverter()
                	);
	                Console.WriteLine(jsonString);
				}
			}

			return 0;
		}
	}
}
