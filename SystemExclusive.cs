using System.Collections.Generic;

using KSynthLib.K5000;


namespace K5KTool
{
    public class SystemExclusiveHeader
    {
        public static readonly int DataSize = 6;

        public int Channel;
        public SystemExclusiveFunction Function;
        public byte Group;
        public byte MachineID;
        public byte Substatus1;
        public byte Substatus2;

        public SystemExclusiveHeader()
        {
            Group = 0x00;  // synth group
            MachineID = 0x0A;  // machine number for K5000
        }

        public SystemExclusiveHeader(byte[] data)
        {
            // data[0] = 0xf0
            // data[1] = 0x40
            Channel = (byte)(data[2] + 1);  // adjust to 1...16
            Function = (SystemExclusiveFunction)data[3];
            Group = data[4];
            MachineID = data[5];
            Substatus1 = data[6];
            Substatus2 = data[7];
        }

        public List<byte> ToData()
        {
            var buf = new List<byte>();

            buf.Add((byte)(Channel - 1));  // adjust to 0...15
            buf.Add((byte)Function);
            buf.Add(Group);
            buf.Add(MachineID);
            buf.Add(Substatus1);
            buf.Add(Substatus2);

            return buf;
        }
    }
}
