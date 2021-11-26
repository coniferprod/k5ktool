using KSynthLib.K5000;

namespace K5KTool
{
	public class ListCommand
	{
		public byte Channel;
		public DumpHeader Header;
        public uint PatchNumber;

		public ListCommand()
		{
			this.Channel = 0;
			this.Header = new DumpHeader(Cardinality.One, BankIdentifier.A, PatchKind.Single);
			this.PatchNumber = 0;
		}

        public ListCommand(byte[] header)
		{
			// header[0] must be 0xF0
			// header[1] must be 0x40 (Kawai ID)
			this.Channel = header[2];
			this.Header = new DumpHeader(header);
			this.PatchNumber = header[8];  // only meaningful for one single, one drum inst, and one combi
        }
    }
}
