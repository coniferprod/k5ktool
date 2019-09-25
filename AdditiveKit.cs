using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
    public enum EnvelopeLoopType
    {
        Off,
        Loop1,
        Loop2
    }

    public class MORFHarmonicEnvelope
    {
        public byte Time1;
        public byte Time2;
        public byte Time3;
        public byte Time4;
        public EnvelopeLoopType LoopType;
    }

    public class HarmonicCopyParameters
    {
        public byte PatchNumber;
        public byte SourceNumber;
    }

    public class HarmonicParameters
    {
        public bool Morf;  // true if morf on
        public byte TotalGain;

        // Non-MORF parameters
        public byte Group;  // 0 = LO (1~64), 1 = HI (65~128)
        public sbyte KeyScalingToGain;  // (-63)1 ... (+63)127
        public byte BalanceVelocityCurve;
        public byte BalanceVelocityDepth;
        
        // MORF parameters
        // Harmonic Copy
        public HarmonicCopyParameters Copy1;
        public HarmonicCopyParameters Copy2;
        public HarmonicCopyParameters Copy3;
        public HarmonicCopyParameters Copy4;

        public MORFHarmonicEnvelope MORFEnvelope;

        public HarmonicParameters()
        {
            Copy1 = new HarmonicCopyParameters();
            Copy2 = new HarmonicCopyParameters();
            Copy3 = new HarmonicCopyParameters();
            Copy4 = new HarmonicCopyParameters();
            MORFEnvelope = new MORFHarmonicEnvelope();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)(Morf ? 1 : 0));
            data.Add(TotalGain);
            data.Add(Group);
            data.Add((byte)(KeyScalingToGain + 64));
            data.Add(BalanceVelocityCurve);
            data.Add(BalanceVelocityDepth);

            data.Add(Copy1.PatchNumber);
            data.Add(Copy1.SourceNumber);
            data.Add(Copy2.PatchNumber);
            data.Add(Copy2.SourceNumber);
            data.Add(Copy3.PatchNumber);
            data.Add(Copy3.SourceNumber);
            data.Add(Copy4.PatchNumber);
            data.Add(Copy4.SourceNumber);

            data.Add(MORFEnvelope.Time1);
            data.Add(MORFEnvelope.Time2);
            data.Add(MORFEnvelope.Time3);
            data.Add(MORFEnvelope.Time4);
            data.Add((byte)MORFEnvelope.LoopType);

            return data.ToArray();
        }
    }

    public class EnvelopeSegment
    {
        public byte Rate;  // 0 ~ 127
        public sbyte Level; // (-63)1 ... (+63)127
    }
    
    public class LoopingEnvelope {
        public EnvelopeSegment Attack;
        public EnvelopeSegment Decay1;
        public EnvelopeSegment Decay2;
        public EnvelopeSegment Release;
        public EnvelopeLoopType LoopType;

        public LoopingEnvelope()
        {
            Attack = new EnvelopeSegment();
            Decay1 = new EnvelopeSegment();
            Decay2 = new EnvelopeSegment();
            Release = new EnvelopeSegment();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add(Attack.Rate);
            data.Add((byte)(Attack.Level + 64));
            data.Add((byte)Decay1.Rate);
            data.Add((byte)(Decay1.Level + 64));
            data.Add((byte)Decay2.Rate);
            data.Add((byte)(Decay2.Level + 64));
            data.Add((byte)Release.Rate);
            data.Add((byte)(Release.Level - 64));
            return data.ToArray();
        }
    }

    public enum FormantLFOShape
    {
        Triangle,
        Sawtooth,
        Random
    }

    public class FormantLFOSettings
    {
        public byte Speed;
        public FormantLFOShape Shape;
        public byte Depth;
    }

    public class FormantParameters
    {
        public sbyte Bias;  // (-63)1 ... (+63)127
        public int EnvLFOSel;  // 0 = ENV, 1 = LFO
        public sbyte EnvelopeDepth; // (-63)1 ... (+63)127
        public LoopingEnvelope Envelope;
        public sbyte VelocitySensitivityEnvelopeDepth;  // (-63)1 ... (+63)127
        public sbyte KeyScalingEnvelopeDepth;  // (-63)1 ... (+63)127
        public FormantLFOSettings LFO;

        public FormantParameters()
        {
            Envelope = new LoopingEnvelope();
            LFO = new FormantLFOSettings();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)(Bias + 64));
            data.Add((byte)EnvLFOSel);
            data.Add((byte)(EnvelopeDepth + 64));

            byte[] envData = Envelope.ToData();
            foreach (byte b in envData)
            {
                data.Add(b);
            }

            data.Add((byte)(VelocitySensitivityEnvelopeDepth + 64));
            data.Add((byte)(KeyScalingEnvelopeDepth + 64));

            data.Add(LFO.Speed);
            data.Add((byte)LFO.Shape);
            data.Add(LFO.Depth);

            return data.ToArray();
        }
    }

    public class HarmonicEnvelope
    {
        public const int NumRates = 4;
        public EnvelopeSegment Segment0;
        public EnvelopeSegment Segment1;
        public bool Segment1Loop;
        public EnvelopeSegment Segment2;
        public bool Segment2Loop;
        public EnvelopeSegment Segment3;

        public HarmonicEnvelope()
        {
            Segment0 = new EnvelopeSegment();
            Segment1 = new EnvelopeSegment();
            Segment2 = new EnvelopeSegment();
            Segment3 = new EnvelopeSegment();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)Segment0.Rate);
            data.Add((byte)Segment0.Level);

            data.Add((byte)Segment1.Rate);
            byte s1Level = (byte)Segment1.Level;
            if (Segment1Loop)
            {
                s1Level.SetBit(6);
            }
            data.Add(s1Level);

            data.Add((byte)Segment2.Rate);
            byte s2Level = (byte)Segment2.Level;
            if (Segment2Loop)
            {
                s2Level.SetBit(6);
            }
            data.Add(s2Level);

            data.Add((byte)Segment3.Rate);
            data.Add((byte)Segment3.Level);

            return data.ToArray();
        }
    }

    public class AdditiveKit
    {
        public const int WaveNumber = 512;  // if the wave number is 512, the source is ADD
        public const int DataSize = 806;
        public const int NumHarmonics = 64;
        public const int NumFilterBands = 128;

        public byte CheckSum;  // [(HCKIT sum)  + (HCcode1 sum) + (HCcode2 sum) + (FF sum) + (HCenv sum) + (loud sense select) + 0xa5] & 0x7f

        public HarmonicParameters Harmonics;
        public FormantParameters Formant;
        public byte[] SoftHarmonics;
        public byte[] LoudHarmonics;
        public byte[] FormantFilter;
        public HarmonicEnvelope[] HarmonicEnvelopes;

        public AdditiveKit()
        {
            Harmonics = new HarmonicParameters();
            Formant = new FormantParameters();

            SoftHarmonics = new byte[NumHarmonics];
            LoudHarmonics = new byte[NumHarmonics];
            FormantFilter = new byte[NumFilterBands];
            HarmonicEnvelopes = new HarmonicEnvelope[NumHarmonics];

            for (int i = 0; i < NumHarmonics; i++)
            {
                HarmonicEnvelopes[i] = new HarmonicEnvelope();
            }
        }

        public AdditiveKit(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);
            CheckSum = b;

            Harmonics = new HarmonicParameters();

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Morf = (b == 1);

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.TotalGain = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Group = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.KeyScalingToGain = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.BalanceVelocityCurve = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.BalanceVelocityDepth = b;

            Harmonics.Copy1 = new HarmonicCopyParameters();
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy1.PatchNumber = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy1.SourceNumber = b;

            Harmonics.Copy2 = new HarmonicCopyParameters();
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy2.PatchNumber = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy2.SourceNumber = b;

            Harmonics.Copy3 = new HarmonicCopyParameters();
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy3.PatchNumber = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy3.SourceNumber = b;

            Harmonics.Copy4 = new HarmonicCopyParameters();
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy4.PatchNumber = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.Copy4.SourceNumber = b;

            Harmonics.MORFEnvelope = new MORFHarmonicEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.MORFEnvelope.Time1 = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.MORFEnvelope.Time2 = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.MORFEnvelope.Time3 = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.MORFEnvelope.Time4 = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Harmonics.MORFEnvelope.LoopType = (EnvelopeLoopType)b;

            Formant = new FormantParameters();
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Bias = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.EnvLFOSel = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.EnvelopeDepth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Attack.Rate = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Attack.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Decay1.Rate = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Decay1.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Decay2.Rate = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Decay2.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Release.Rate = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.Release.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.Envelope.LoopType = (EnvelopeLoopType)b;

            (b, offset) = Util.GetNextByte(data, offset);
            Formant.VelocitySensitivityEnvelopeDepth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.KeyScalingEnvelopeDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Formant.LFO.Speed = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.LFO.Shape = (FormantLFOShape)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Formant.LFO.Depth = b;

            SoftHarmonics = new byte[NumHarmonics];
            for (int i = 0; i < NumHarmonics; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                SoftHarmonics[i] = b;
            }

            LoudHarmonics = new byte[NumHarmonics];
            for (int i = 0; i < NumHarmonics; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                LoudHarmonics[i] = b;
            }

            // TODO: Maybe the filter bands could be combined with the formant parameters?
            FormantFilter = new byte[NumFilterBands];
            for (int i = 0; i < NumFilterBands; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                FormantFilter[i] = b;
            }

            HarmonicEnvelopes = new HarmonicEnvelope[NumHarmonics];
            for (int i = 0; i < NumHarmonics; i++)
            {
                HarmonicEnvelopes[i] = new HarmonicEnvelope();
                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment0.Rate = b;
                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment0.Level = (sbyte)b;

                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment1.Rate = b;
                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment1Loop = b.IsBitSet(6);
                HarmonicEnvelopes[i].Segment1.Level = (sbyte)(b & 0x3F);  // bottom 6 bits = 0~63

                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment2.Rate = b;
                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment2Loop = b.IsBitSet(6);
                HarmonicEnvelopes[i].Segment2.Level = (sbyte)(b & 0x3F);

                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment3.Rate = b;  // 0~127
                (b, offset) = Util.GetNextByte(data, offset);
                HarmonicEnvelopes[i].Segment3.Level = (sbyte)b;  // 0~63
            }

            (b, offset) = Util.GetNextByte(data, offset);  // 806 dummy
        }

        public override string ToString() 
        {
            StringBuilder b = new StringBuilder();
            b.Append(String.Format("MORF is {0}\n", Harmonics.Morf ? "ON" : "OFF"));
            b.Append(String.Format("Total Gain = {0}\n", Harmonics.TotalGain));
            b.Append(String.Format("Harm group = {0}\n", Harmonics.Group));
            b.Append("Soft harmonics:\n");
            for (int i = 0; i < NumHarmonics; i++)
            {
                b.Append(String.Format("{0}: {1}\n", i, SoftHarmonics[i]));
            }
            b.Append("Loud harmonics:\n");
            for (int i = 0; i < NumHarmonics; i++)
            {
                b.Append(String.Format("{0}: {1}\n", i, LoudHarmonics[i]));
            }

            b.Append("Harmonic envelopes:\n    Atk  Dc1  Dc2  Rls\n");
            for (int i = 0; i < NumHarmonics; i++)
            {
                HarmonicEnvelope env = HarmonicEnvelopes[i];
                b.Append(String.Format("{0}: Level {1}  {2}  {3}  {4}\n", i + 1, env.Segment0.Level, env.Segment1.Level, env.Segment2.Level, env.Segment3.Level));
                b.Append(String.Format("   Rate  {0}  {1}  {2}  {3}\n", env.Segment0.Rate, env.Segment1.Rate, env.Segment2.Rate, env.Segment3.Rate));
            }

            return b.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            byte[] harmonicsData = Harmonics.ToData();
            foreach (byte b in harmonicsData)
            {
                data.Add(b);
            }

            byte[] formantParameterData = Formant.ToData();
            foreach (byte b in formantParameterData)
            {
                data.Add(b);
            }

            foreach (byte b in SoftHarmonics)
            {
                data.Add(b);
            }
            foreach (byte b in LoudHarmonics)
            {
                data.Add(b);
            }
            foreach (byte b in FormantFilter)
            {
                data.Add(b);
            }

            for (int i = 0; i < NumHarmonics; i++)
            {
                byte[] harmonicEnvelopeData = HarmonicEnvelopes[i].ToData();
                foreach (byte b in harmonicEnvelopeData)
                {
                    data.Add(b);
                }
            }

            byte[] allData = data.ToArray();
            byte checkSum = ComputeCheckSum(allData);
            data.Insert(0, checkSum);

            data.Add(0);  // 806 dummy

            return data.ToArray();
        }

        private byte ComputeCheckSum(byte[] data)
        {
            // check sum = [(HCKIT sum) + (HCcode1 sum) + (HCcode2 sum) + (FF sum) + (HCenv sum) + (loud sens select) + 0xA5] & 0x7f
            byte total = 0;

            // TODO: Should these be calculated separately and then added to the total?

            // HCKIT sum = MORF flag, harmonics, formant
            total += (byte)(Harmonics.Morf ? 1 : 0);
            byte[] harmonicsData = Harmonics.ToData();
            foreach (byte b in harmonicsData)
            {
                total += b;
            }

            byte[] formantParameterData = Formant.ToData();
            foreach (byte b in formantParameterData)
            {
                total += b;
            }

            // HC code 1 sum
            foreach (byte b in SoftHarmonics)
            {
                total += b;
            }

            // HC code 2 sum
            foreach (byte b in LoudHarmonics)
            {
                total += b;
            }

            // FF sum
            foreach (byte b in FormantFilter)
            {
                total += b;
            }

            for (int i = 0; i < NumHarmonics; i++)
            {
                byte[] harmonicEnvelopeData = HarmonicEnvelopes[i].ToData();
                foreach (byte b in harmonicEnvelopeData)
                {
                    total += b;
                }
            }

            // TODO: What even *is* the "loud sense select" or "LS select"?

            total += 0xA5;

            return (byte)(total & 0x7f);
        }

    }
}