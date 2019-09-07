using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
        public enum KeyScalingToPitch
    {
        ZeroCent,
        TwentyFiveCent,
        ThirtyThreeCent,
        FiftyCent
    }

    public class PitchEnvelope
    {
        public sbyte StartLevel;  // (-63)1 ~ (+63)127
        public byte AttackTime;  // 0 ~ 127
        public sbyte AttackLevel; // (-63)1 ~ (+63)127
        public byte DecayTime;  // 0 ~ 127
        public sbyte TimeVelocitySensitivity; // (-63)1 ~ (+63)127
        public sbyte LevelVelocitySensitivity; // (-63)1 ~ (+63)127

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Start Level = {0}, Atak T = {1}, Atak L = {2}, Dcay T = {3}", StartLevel, AttackTime, AttackLevel, DecayTime));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)(StartLevel + 64));
            data.Add(AttackTime);
            data.Add((byte)(AttackLevel + 64));
            data.Add(DecayTime);
            data.Add((byte)(TimeVelocitySensitivity + 64));
            data.Add((byte)(LevelVelocitySensitivity + 64));

            return data.ToArray();
        }
    }

    public class DCOSettings
    {
        public int WaveNumber;
        public sbyte Coarse;
        public sbyte Fine;
        public byte FixedKey;  // 0=OFF, 21 ~ 108=ON(A-1 ~ C7)
        public KeyScalingToPitch KSPitch;
        public PitchEnvelope Envelope;

        public DCOSettings()
        {
            Envelope = new PitchEnvelope();
        }

        public DCOSettings(byte[] data, int offset)
        {
            byte b = 0;
            byte waveMSB = 0;
            (b, offset) = Util.GetNextByte(data, offset);
            waveMSB = b;

            byte waveLSB = 0;
            (b, offset) = Util.GetNextByte(data, offset);
            waveLSB = b;

            string waveMSBBitString = Convert.ToString(waveMSB, 2).PadLeft(3, '0');
            string waveLSBBitString = Convert.ToString(waveLSB, 2).PadLeft(7, '0');
            string waveBitString = waveMSBBitString + waveLSBBitString;
            int waveNumber = Convert.ToInt32(waveBitString, 2);
            System.Console.WriteLine(String.Format("wave kit MSB = {0:X2} | {1}, LSB = {2:X2} | {3}, combined = {4}, result = {5}", 
                waveMSB, waveMSBBitString, waveLSB, waveLSBBitString, waveBitString, waveNumber));

            WaveNumber = waveNumber;

            (b, offset) = Util.GetNextByte(data, offset);
            Coarse = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Fine = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            FixedKey = b;
            (b, offset) = Util.GetNextByte(data, offset);
            KSPitch = (KeyScalingToPitch)b;

            Envelope = new PitchEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.StartLevel = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.AttackTime = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.AttackLevel = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.DecayTime = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.TimeVelocitySensitivity = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.LevelVelocitySensitivity = (sbyte)(b - 64);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string waveName = "PCM";
            if (WaveNumber == AdditiveKit.WaveNumber)
            {
                waveName = "ADD";
            } 
            builder.Append(String.Format("Wave Type = {0}  ", waveName));
            if (waveName.Equals("PCM"))
            {
                builder.Append(String.Format("{0} ({1})\n", Wave.Instance[WaveNumber], WaveNumber + 1));
            }
            builder.Append(String.Format("Coarse = {0}  Fine = {1}\n", Coarse, Fine));
            builder.Append(String.Format("KS Pitch = {0}  Fixed Key = {1}\n", KSPitch, FixedKey == 0 ? "OFF" : Convert.ToString(FixedKey)));
            builder.Append(String.Format("Pitch Env: {0}\n", Envelope.ToString()));
            builder.Append(String.Format("Vel To: Level = {0}  Time = {1}\n", Envelope.LevelVelocitySensitivity, Envelope.TimeVelocitySensitivity));

            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            // Convert wave kit number to binary string with 10 digits
            string waveBitString = Convert.ToString(WaveNumber, 2).PadLeft(10, '0');
            string msbBitString = waveBitString.Substring(0, 3);
            data.Add(Convert.ToByte(msbBitString, 2));
            string lsbBitString = waveBitString.Substring(3);
            data.Add(Convert.ToByte(lsbBitString, 2));

            data.Add((byte)Coarse);
            data.Add((byte)Fine);
            data.Add((byte)FixedKey);
            data.Add((byte)KSPitch);

            byte[] envData = Envelope.ToData();
            foreach (byte b in envData)
            {
                data.Add(b);
            }

            return data.ToArray();
        }
    }
}
