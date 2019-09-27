using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
    public class AmplifierEnvelope
    {
        public int AttackTime;
        public int Decay1Time;
        public int Decay1Level;
        public int Decay2Time;
        public int Decay2Level;
        public int ReleaseTime;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("A={0}, D1={1}/{2}, D2={3}/{4}, R={5}\n", AttackTime, Decay1Time, Decay1Level, Decay2Time, Decay2Level, ReleaseTime));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)AttackTime);
            data.Add((byte)Decay1Time);
            data.Add((byte)Decay1Level);
            data.Add((byte)Decay2Time);
            data.Add((byte)Decay2Level);
            data.Add((byte)ReleaseTime);

            return data.ToArray();
        }
    }


    public class KeyScalingControlEnvelope
    {
        public sbyte Level;  // all (-63)1 ~ (+63)127
        public sbyte AttackTime;
        public sbyte Decay1Time;
        public sbyte ReleaseTime;

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)(Level + 64));
            data.Add((byte)(AttackTime + 64));
            data.Add((byte)(Decay1Time + 64));
            data.Add((byte)(ReleaseTime + 64));

            return data.ToArray();
        }
    }

    public class VelocityControlEnvelope
    {
        public byte Level;  // 0 ~ 63
        public sbyte AttackTime; // (-63)1 ~ (+63)127
        public sbyte Decay1Time; // (-63)1 ~ (+63)127
        public sbyte ReleaseTime; // (-63)1 ~ (+63)127

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add(Level);
            data.Add((byte)(AttackTime + 64));
            data.Add((byte)(Decay1Time + 64));
            data.Add((byte)(ReleaseTime + 64));

            return data.ToArray();
        }
    }
    
    public class DCASettings
    {
        public byte VelocityCurve;  // values are 0 ~ 11, shown as 1 ~ 12
        public AmplifierEnvelope Envelope;
        public KeyScalingControlEnvelope KeyScaling;
        public VelocityControlEnvelope VelocitySensitivity;

        public DCASettings()
        {
            KeyScaling = new KeyScalingControlEnvelope();
            VelocitySensitivity = new VelocityControlEnvelope();

            VelocityCurve = 5;

            Envelope = new AmplifierEnvelope()
            {
                AttackTime = 20,
                Decay1Time = 95,
                Decay1Level = 127,
                Decay2Time = 110,
                Decay2Level = 127,
                ReleaseTime = 11
            };
        }

        public DCASettings(byte[] data, int offset)
        {
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);
            VelocityCurve = (byte)(b + 1);

            Envelope = new AmplifierEnvelope();

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.AttackTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay1Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay1Level = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay2Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay2Level = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.ReleaseTime = b;

            KeyScaling = new KeyScalingControlEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.AttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.Decay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.ReleaseTime = (sbyte)(b - 64);

            VelocitySensitivity = new VelocityControlEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            VelocitySensitivity.Level = b;
            (b, offset) = Util.GetNextByte(data, offset);
            VelocitySensitivity.AttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            VelocitySensitivity.Decay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            VelocitySensitivity.ReleaseTime = (sbyte)(b - 64);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("velocity curve={0}\n", VelocityCurve));
            builder.Append(String.Format("envelope = {0}\n", Envelope.ToString()));
            builder.Append("DCA Modulation:\n");
            builder.Append(String.Format("KS To DCA Env.: Level = {0}  Atak T = {1}, Decy1 T = {2}, Release = {3}\n",
                KeyScaling.Level, KeyScaling.AttackTime, KeyScaling.Decay1Time, KeyScaling.ReleaseTime));
            builder.Append(String.Format("Vel To DCA Env.: Level = {0}  Atak T = {1}, Decy1 T = {2}, Release = {3}\n",
                VelocitySensitivity.Level, VelocitySensitivity.AttackTime, VelocitySensitivity.Decay1Time, VelocitySensitivity.ReleaseTime));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)(VelocityCurve - 1));  // adjust value to 0 ~ 11

            byte[] envData = Envelope.ToData();
            foreach (byte b in envData)
            {
                data.Add(b);
            }

            byte[] ksData = KeyScaling.ToData();
            foreach (byte b in ksData)
            {
                data.Add(b);
            }

            byte[] velData = VelocitySensitivity.ToData();
            foreach (byte b in velData)
            {
                data.Add(b);
            }

            return data.ToArray();
        }
    }
}