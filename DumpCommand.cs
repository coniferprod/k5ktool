using System;

namespace K5KTool
{
	public class DumpCommand
	{
		public enum Cardinality
        {
			One,
			Block
        }

		public enum BankIdentifier
        {
			A,
			B,
			D,
			E,
			F
        }

		public byte Channel;
		public Cardinality Card;
		public BankIdentifier Bank;

		public DumpCommand(byte[] header)
		{
			// header[0] must be 0xF0
			// header[1] must be 0x40 (Kawai ID)
			this.Channel = header[2];

			this.Card = Cardinality.One;
			if (header[3] == 0x20)
            {
				this.Card = Cardinality.One;
            }
			else if (header[3] == 0x21)
            {
				this.Card = Cardinality.Block;
            }

			this.Bank = BankIdentifier.A;
			switch (header[6])
            {
				case 0x00:
					this.Bank = BankIdentifier.A;
					break;

				case 0x01:
					this.Bank = BankIdentifier.B;
					break;

				case 0x02:
					this.Bank = BankIdentifier.D;
					break;

				case 0x03:
					this.Bank = BankIdentifier.E;
					break;

				case 0x04:
					this.Bank = BankIdentifier.F;
					break;

				default:
					break;
            }


		}
	}

}
