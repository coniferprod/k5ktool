﻿using System;

using KSynthLib.K5000;

namespace K5KTool
{
	public class DumpCommand
	{
		public byte Channel;
		public DumpHeader Header;
		public uint PatchNumber;

		public DumpCommand()
		{
			this.Channel = 0;
			this.Header = new DumpHeader(Cardinality.One, BankIdentifier.A, PatchKind.Single);
			this.PatchNumber = 0;
		}

		public DumpCommand(byte[] header)
		{
			this.Channel = header[2];
			this.Header = new DumpHeader(header);
			this.PatchNumber = header[8];  // only meaningful for one single, one drum inst, and one combi
		}
	}
}
