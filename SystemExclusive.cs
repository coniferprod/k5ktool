using System;
using System.Collections.Generic;

using KSynthLib.K5000;

namespace K5KTool
{
    public class SystemExclusiveHeader
    {
        public static readonly int DataSize = 6;

        public byte Channel;
        public SystemExclusiveFunction Function;
        public byte Group;
        public byte MachineID;
        public byte Substatus1;
        public byte Substatus2;

        public SystemExclusiveHeader()
        {

        }

        public SystemExclusiveHeader(byte[] data)
        {
            // data[0] = 0xf0
            // data[1] = 0x40
            Channel = data[2];
            Function = (SystemExclusiveFunction)data[3];
            Group = data[4];
            MachineID = data[5];
            Substatus1 = data[6];
            Substatus2 = data[7];
        }

        public List<byte> ToData()
        {
            var buf = new List<byte>();

            buf.Add((byte)Function);
            buf.Add(Group);
            buf.Add(MachineID);
            buf.Add(Substatus1);
            buf.Add(Substatus2);

            return buf;
        }
    }
}
