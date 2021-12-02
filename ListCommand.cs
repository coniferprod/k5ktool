using System;
using System.Linq;
using System.Collections.Generic;

using KSynthLib.K5000;
using KSynthLib.Common;

namespace K5KTool
{
	public class PatchInfo
	{
		public BankIdentifier Bank;
		public int PatchNumber;
		public string PatchName;
		public int PCMSourceCount;
		public int AdditiveSourceCount;
	}

	public class ListCommand
	{
		public byte Channel;
		public DumpHeader Header;
        public uint PatchNumber;

		private byte[] data;

        public ListCommand(byte[] fileData)
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

		public int ListPatches()
		{
			// SysEx header at this point should be "F0 40 00 21 00 0A 00 00" followed by the tone map
			var offset = 8;   // skip to the tone map
			//Console.Error.WriteLine($"offset = {offset}");

			// For a block data dump, need to parse the tone map
			byte[] buffer;
			(buffer, offset) = Util.GetNextBytes(this.data, offset, PatchMap.Size);
			// now the offset has been updated to past the tone map
			//Console.Error.WriteLine($"offset = {offset}");
			var patchMap = new PatchMap(buffer);

			List<int> patchNumbers = new List<int>();

			//Console.WriteLine("Patches included:");
			var patchCount = 0;
			for (var i = 0; i < PatchMap.PatchCount; i++)
			{
				if (patchMap[i])
				{
					patchCount += 1;
					//Console.Write(i + 1);
					//Console.Write(" ");

					patchNumbers.Add(i);
				}
			}
			//Console.WriteLine($"\nTotal = {patchCount} patches");

			// Whatever the first patch is, it must be at least this many bytes (always has at least two sources)
			var minimumPatchSize = SingleCommonSettings.DataSize + 2 * KSynthLib.K5000.Source.DataSize;
			//Console.Error.WriteLine($"minimum patch size = {minimumPatchSize}");

			var totalPatchSize = 0;  // the total size of all the single patches

			var allPatchInfos = new List<PatchInfo>();

			var singlePatches = new List<SinglePatch>();
			foreach (var patchNumber in patchNumbers)
			{
				var startOffset = offset;  // save the current offset because we need to copy more bytes later

				var sizeToRead = Math.Max(minimumPatchSize, this.data.Length - offset);
				//Console.WriteLine($"About to read {sizeToRead} bytes starting from offset {offset:X4}h");
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

				var patchInfo = new PatchInfo
				{
					Bank = this.Header.Bank,
					PatchNumber = patchNumber + 1,
					PatchName = patch.SingleCommon.Name,
					PCMSourceCount = pcmCount,
					AdditiveSourceCount = addCount,
				};
				Console.WriteLine($"{patchInfo.Bank}{patchInfo.PatchNumber:D3} | {patchInfo.PatchName,8} | {patchInfo.PCMSourceCount}PCM {patchInfo.AdditiveSourceCount}ADD");

				allPatchInfos.Add(patchInfo);
			}

			foreach (var patchInfo in allPatchInfos)
			{
				Console.WriteLine($"{patchInfo.Bank}{patchInfo.PatchNumber:D3} | {patchInfo.PatchName,8} | {patchInfo.PCMSourceCount}PCM {patchInfo.AdditiveSourceCount}ADD");
			}

			return 0;
		}
    }
}
