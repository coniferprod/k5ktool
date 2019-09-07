using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
    public enum FilterMode
    {
        LowPass,
        HighPass
    }

    // Same as amp envelope, but decay levels 1...127 are interpreted as -63 ... 63
    public class FilterEnvelope
    {
        public byte AttackTime;
        public byte Decay1Time;
        public sbyte Decay1Level;
        public byte Decay2Time;
        public sbyte Decay2Level;
        public byte ReleaseTime;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("A={0}, D1={1}/{2}, D2={3}/{4}, R={5}", AttackTime, Decay1Time, Decay1Level, Decay2Time, Decay2Level, ReleaseTime));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)AttackTime);
            data.Add((byte)Decay1Time);
            data.Add((byte)(Decay1Level + 64));
            data.Add((byte)Decay2Time);
            data.Add((byte)(Decay2Level + 64));
            data.Add((byte)ReleaseTime);

            return data.ToArray();
        }
    }

    public class DCFSettings
    {
        public bool IsActive;
        public FilterMode Mode;
        public byte VelocityCurve;  // 0 ~ 11 (1 ~ 12)
        public byte Resonance; // 0 ~ 7
        public int Level;
        public byte Cutoff;
        public sbyte CutoffKeyScalingDepth; // (-63)1 ~ (+63)127
        public sbyte CutoffVelocityDepth; // (-63)1 ~ (+63)127
        public sbyte EnvelopeDepth; // (-63)1 ~ (+63)127
        public FilterEnvelope Envelope;
        public sbyte KSToEnvAttackTime;
        public sbyte KSToEnvDecay1Time;
        public sbyte VelocityToEnvDepth;
        public sbyte VelocityToEnvAttackTime;
        public sbyte VelocityToEnvDecay1Time;

        public DCFSettings()
        {
            Envelope = new FilterEnvelope();
        }

        public DCFSettings(byte[] data, int offset)
        {
            byte b = 0;  // will be reused when getting the next byte
            (b, offset) = Util.GetNextByte(data, offset);
            IsActive = (b == 0);  // 0=Active, 1=Bypass

            (b, offset) = Util.GetNextByte(data, offset);
            Mode = (FilterMode) b;

            (b, offset) = Util.GetNextByte(data, offset);
            VelocityCurve = (byte)(b + 1);  // 0~11 (1~12)

            (b, offset) = Util.GetNextByte(data, offset);
            Resonance = b;  // 0~7

            (b, offset) = Util.GetNextByte(data, offset);
            Level = b;  // 0~7 (7~0)

            (b, offset) = Util.GetNextByte(data, offset);
            Cutoff = b;

            (b, offset) = Util.GetNextByte(data, offset);
            CutoffKeyScalingDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            CutoffVelocityDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeDepth = (sbyte)(b - 64);

            Envelope = new FilterEnvelope();

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.AttackTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay1Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay1Level = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay2Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.Decay2Level = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Envelope.ReleaseTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            KSToEnvAttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            KSToEnvDecay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            VelocityToEnvDepth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            VelocityToEnvAttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            VelocityToEnvDecay1Time = (sbyte)(b - 64);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("active = {0}, mode = {1}\n", IsActive ? "YES" : "NO", Mode));
            builder.Append(String.Format("velocity curve = {0}\n", VelocityCurve));
            builder.Append(String.Format("resonance = {0}, level = {1}, cutoff = {2}\n", Resonance, Level, Cutoff));
            builder.Append(String.Format("cutoff KS depth = {0}, cutoff vel depth = {1}\n", CutoffKeyScalingDepth, CutoffVelocityDepth));
            builder.Append(String.Format("envelope = {0}\n", Envelope.ToString()));
            builder.Append(String.Format("DCF Mod.: KS to Attack = {0}  KS to Dcy1 = {1}  Vel to Env = {2}  Vel to Atk = {3}  Vel to Dcy1 = {4}\n", 
                KSToEnvAttackTime, KSToEnvDecay1Time, VelocityToEnvDepth, VelocityToEnvAttackTime, VelocityToEnvDecay1Time));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)(IsActive ? 1 : 0));
            data.Add((byte)Mode);
            data.Add((byte)(VelocityCurve - 1));
            data.Add(Resonance);
            data.Add((byte)Level);
            data.Add(Cutoff);
            data.Add((byte)(CutoffKeyScalingDepth + 64));
            data.Add((byte)(CutoffVelocityDepth + 64));
            data.Add((byte)(EnvelopeDepth + 64));

            byte[] envData = Envelope.ToData();
            foreach (byte b in envData)
            {
                data.Add(b);
            }

            data.Add((byte)KSToEnvAttackTime);
            data.Add((byte)KSToEnvDecay1Time);
            data.Add((byte)VelocityToEnvDepth);
            data.Add((byte)VelocityToEnvAttackTime);
            data.Add((byte)VelocityToEnvDecay1Time);

            return data.ToArray();
        }
    }
}
