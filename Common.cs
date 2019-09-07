using System;
using System.Collections.Generic;
using System.Text;

namespace K5KTool
{
    public enum PolyphonyMode {
        Poly,
        Solo1,
        Solo2
    }

    public enum EffectDestination
    {
        Effect1DryWet,
        Effect1Para,
        Effect2DryWet,
        Effect2Para,
        Effect3DryWet,
        Effect3Para,
        Effect4DryWet,
        Effect4Para,
        ReverbDryWet1,
        ReverbDryWet2
    }

    public enum ControlSource
    {
        Bender,
        ChannelPressure,
        Wheel,
        Expression,
        MIDIVolume,
        PanPot,
        GeneralController1,
        GeneralController2,
        GeneralController3,
        GeneralController4,
        GeneralController5,
        GeneralController6,
        GeneralController7,
        GeneralController8
    }

    public enum ControlDestination
    {
        PitchOffset,
        CutoffOffset,
        Level,
        VibratoDepthOffset,
        GrowlDepthOffset,
        TremoloDepthOffset,
        LFOSpeedOffset,
        AttackTimeOffset,
        Decay1TimeOffset,
        ReleaseTimeOffset,
        VelocityOffset,
        ResonanceOffset,
        PanPotOffset,
        FFBiasOffset,
        FFEnvLFODepthOffset,
        FFEnvLFOSpeedOffset,
        HarmonicLoOffset,
        HarmonicHiOffset,
        HarmonicEvenOffset,
        HarmonicOddOffset
    }

    public class EffectControl
    {
        public ControlSource Source;
        public EffectDestination Destination;
        public sbyte Depth;  // (~31)33 ~ (+31)95

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("source = {0} destination = {1} depth = {2}", Source, Destination, Depth));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)Source);
            data.Add((byte)Destination);
            data.Add((byte)(Depth + 64));  // bring to range 33 ~ 95 again
            return data.ToArray();
        }
    }

    public enum MacroControllerType
    {
        PitchOffset,
        CutoffOffset,
        Level,
        VibratoDepthOffset,
        GrowlDepthOffset,
        TremoloDepthOffset,
        LFOSpeedOffset,
        AttackTimeOffset,
        Decay1TimeOffset,
        ReleaseTimeOffset,
        VelocityOffset,
        ResonanceOffset,
        PanPotOffset,
        FFBiasOffset,
        FFEnvLFODepthOffset,
        FFEnvLFOSpeedOffset,
        HarmonicLoOffset,
        HarmonicHiOffset,
        HarmonicEvenOffset,
        HarmonicOddOffset
    }

    public class MacroControllerParameter
    {
        public MacroControllerType Type;
        public sbyte Depth;
    }

    public class MacroController
    {
        public MacroControllerParameter Param1;
        public MacroControllerParameter Param2;

        public MacroController()
        {
            Param1 = new MacroControllerParameter();
            Param2 = new MacroControllerParameter();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("DEST1 = {0}, Depth = {1}. DEST2 = {2}, Depth = {3}", Param1.Type, Param1.Depth, Param2.Type, Param2.Depth));
            return builder.ToString();
        }

    }

    public enum Switch {
        Off,
        HarmMax,
        HarmBright,
        HarmDark,
        HarmSaw,
        SelectLoud,
        AddLoud,
        Add5th,
        AddOdd,
        AddEven,
        HE1,
        HE2,
        HELoop,
        FFMax,
        FFComb,
        FFHiCut,
        FFComb2
    }

    public class CommonSettings
    {
        public static int DataSize = 81;
        public const int MaxSources = 6;

        public string Name;
        public byte Volume;
        public byte EffectAlgorithm;  // 0 ~ 3

        public ReverbSettings Reverb;
        public EffectSettings Effect1;
        public EffectSettings Effect2;
        public EffectSettings Effect3;
        public EffectSettings Effect4;

        public GEQSettings GEQ;

        public PolyphonyMode Poly;

        public int NumSources;

        public bool[] IsSourceMuted;  // 0=muted, 1=not muted - store the inverse here

        public byte AM;  // 0=off, value=1~5(src2~6)

        public EffectControl EffectControl1;
        public EffectControl EffectControl2;

        public bool IsPortamentoEnabled;  // 0=off, 1=on
        public int PortamentoSpeed;  // 0 ~ 127

        // TODO: Add macro controllers
        public MacroController Macro1;
        public MacroController Macro2;
        public MacroController Macro3;
        public MacroController Macro4;

        public Switch Switch1;
        public Switch Switch2;
        public Switch FootSwitch1;
        public Switch FootSwitch2;

        public CommonSettings()
        {
            Name = new String("Init    ");
            Reverb = new ReverbSettings();
            Effect1 = new EffectSettings();
            Effect2 = new EffectSettings();
            Effect3 = new EffectSettings();
            Effect4 = new EffectSettings();
            GEQ = new GEQSettings();
            IsSourceMuted = new bool[MaxSources];
            EffectControl1 = new EffectControl();
            EffectControl2 = new EffectControl();
            Macro1 = new MacroController();
            Macro2 = new MacroController();
            Macro3 = new MacroController();
            Macro4 = new MacroController();
        }

        public CommonSettings(byte[] data)
        {
            Reverb = new ReverbSettings();
            Effect1 = new EffectSettings();
            Effect2 = new EffectSettings();
            Effect3 = new EffectSettings();
            Effect4 = new EffectSettings();
            GEQ = new GEQSettings();
            IsSourceMuted = new bool[MaxSources];
            EffectControl1 = new EffectControl();
            EffectControl2 = new EffectControl();
            Macro1 = new MacroController();
            Macro2 = new MacroController();
            Macro3 = new MacroController();
            Macro4 = new MacroController();

            System.Console.WriteLine(String.Format("Common data:\n{0}", Util.HexDump(data)));

            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);  
            EffectAlgorithm = b; // 0 ~ 3
            System.Console.WriteLine(String.Format("Effect Algorithm = {0}", EffectAlgorithm));

            Reverb = GetReverb(data, offset);
            offset += 6;
            System.Console.WriteLine(String.Format("Reverb = {0}", Reverb.ToString()));

            Effect1 = GetEffect(data, offset);
            offset += 6;
            System.Console.WriteLine(String.Format("E1 = {0}", Effect1.ToString()));

            Effect2 = GetEffect(data, offset);
            offset += 6;
            System.Console.WriteLine(String.Format("E2 = {0}", Effect2.ToString()));

            Effect3 = GetEffect(data, offset);
            offset += 6;
            System.Console.WriteLine(String.Format("E3 = {0}", Effect3.ToString()));

            Effect4 = GetEffect(data, offset);
            offset += 6;
            System.Console.WriteLine(String.Format("E4 = {0}", Effect4.ToString()));

            GEQ = GetGEQ(data, offset);
            offset += 7;
            System.Console.WriteLine(String.Format("GEQ = {0}", GEQ.ToString()));

            // "drum_mark" can be ignored
            (b, offset) = Util.GetNextByte(data, offset);

            Name = GetName(data, offset);
            offset += 8;
            
            (b, offset) = Util.GetNextByte(data, offset);
            Volume = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Poly = (PolyphonyMode) b; // 0=POLY, 1=SOLO1, 2=SOLO2

            // "no use" can be ignored
            (b, offset) = Util.GetNextByte(data, offset);

            (b, offset) = Util.GetNextByte(data, offset);
            NumSources = b;

            (b, offset) = Util.GetNextByte(data, offset);
            for (int i = 0; i < MaxSources; i++)
            {
                bool isNotMuted = b.IsBitSet(i);
                IsSourceMuted[i] = ! isNotMuted;
            }

            (b, offset) = Util.GetNextByte(data, offset);
            AM = b;

            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl1.Source = (ControlSource)b;
            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl1.Destination = (EffectDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl1.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl2.Source = (ControlSource)b;
            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl2.Destination = (EffectDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            EffectControl2.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            IsPortamentoEnabled = (b == 1);

            (b, offset) = Util.GetNextByte(data, offset);
            PortamentoSpeed = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Macro1.Param1.Type = (MacroControllerType)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Macro1.Param2.Type = (MacroControllerType)b;

            (b, offset) = Util.GetNextByte(data, offset);
            Macro2.Param1.Type = (MacroControllerType)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Macro2.Param2.Type = (MacroControllerType)b;

            (b, offset) = Util.GetNextByte(data, offset);
            Macro3.Param1.Type = (MacroControllerType)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Macro3.Param2.Type = (MacroControllerType)b;

            (b, offset) = Util.GetNextByte(data, offset);
            Macro4.Param1.Type = (MacroControllerType)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Macro4.Param2.Type = (MacroControllerType)b;

            (b, offset) = Util.GetNextByte(data, offset);
            Macro1.Param1.Depth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Macro1.Param2.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Macro2.Param1.Depth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Macro2.Param2.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Macro3.Param1.Depth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Macro3.Param2.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Macro4.Param1.Depth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            Macro4.Param2.Depth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            Switch1 = (Switch)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Switch2 = (Switch)b;
            (b, offset) = Util.GetNextByte(data, offset);
            FootSwitch1 = (Switch)b;
            (b, offset) = Util.GetNextByte(data, offset);
            FootSwitch2 = (Switch)b;

            System.Console.WriteLine(String.Format("Common data parsed, offset = {0:X2} ({0})", offset));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Name = '{0}' Volume = {1}\n", Name, Volume));
            builder.Append(String.Format("Poly = {0}\n", Poly));
            builder.Append(String.Format("Portamento = {0}, speed = {1}\n", IsPortamentoEnabled ? "ON" : "OFF", PortamentoSpeed));
            builder.Append(String.Format("Sources = {0}\n", NumSources));
            builder.Append(String.Format("AM = {0}\n", (AM == 0) ? "OFF" : Convert.ToString(AM)));
            builder.Append("Macro controllers:\n");
            builder.Append(String.Format("User 1: {0}\n", Macro1.ToString()));
            builder.Append(String.Format("User 2: {0}\n", Macro2.ToString()));
            builder.Append(String.Format("User 3: {0}\n", Macro3.ToString()));
            builder.Append(String.Format("User 4: {0}\n", Macro4.ToString()));
            builder.Append(String.Format("SW1 = {0}  SW2 = {1}  F.SW1 = {2}  F.SW2 = {3}\n", Switch1, Switch2, FootSwitch1, FootSwitch2));

            builder.Append(String.Format("REVERB:\n{0}\n", Reverb.ToString()));
            builder.Append(String.Format("Effect algorithm = {0}\n", EffectAlgorithm + 1));
            builder.Append(String.Format("EFFECT 1:\n{0}", Effect1.ToString()));
            builder.Append(String.Format("EFFECT 2:\n{0}", Effect2.ToString()));
            builder.Append(String.Format("EFFECT 3:\n{0}", Effect3.ToString()));
            builder.Append(String.Format("EFFECT 4:\n{0}", Effect4.ToString()));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add(EffectAlgorithm);

            byte[] reverbData = Reverb.ToData();
            foreach (byte b in reverbData)
            {
                data.Add(b);
            }

            byte[] effect1Data = Effect1.ToData();
            foreach (byte b in effect1Data)
            {
                data.Add(b);
            }

            byte[] effect2Data = Effect2.ToData();
            foreach (byte b in effect2Data)
            {
                data.Add(b);
            }

            byte[] effect3Data = Effect3.ToData();
            foreach (byte b in effect3Data)
            {
                data.Add(b);
            }

            byte[] effect4Data = Effect4.ToData();
            foreach (byte b in effect4Data)
            {
                data.Add(b);
            }

            byte[] geqData = GEQ.ToData();
            foreach (byte b in geqData)
            {
                data.Add(b);
            }

            data.Add(0); // drum_mark, 0=normal(not drum)

            foreach (char ch in Name)
            {
                data.Add(Convert.ToByte(ch));
            }

            data.Add(Volume);
            data.Add((byte)Poly);
            data.Add(0); // "no use"
            data.Add((byte)NumSources);

            byte sourceMute = 0;
            for (int i = 0; i < MaxSources; i++)
            {
                if (IsSourceMuted[i])
                {
                    sourceMute.SetBit(i);
                }
            }
            data.Add(sourceMute);

            data.Add(AM);

            byte[] effectControl1Data = EffectControl1.ToData();
            foreach (byte b in effectControl1Data)
            {
                data.Add(b);
            }

            byte[] effectControl2Data = EffectControl2.ToData();
            foreach (byte b in effectControl2Data)
            {
                data.Add(b);
            }
            
            data.Add((byte)(IsPortamentoEnabled ? 1 : 0));  // only bit 0 is used for this
            data.Add((byte)PortamentoSpeed);

            data.Add((byte)Macro1.Param1.Type);
            data.Add((byte)Macro1.Param2.Type);
            data.Add((byte)Macro2.Param1.Type);
            data.Add((byte)Macro2.Param2.Type);
            data.Add((byte)Macro3.Param1.Type);
            data.Add((byte)Macro3.Param2.Type);
            data.Add((byte)Macro4.Param1.Type);
            data.Add((byte)Macro4.Param2.Type);

            data.Add((byte)(Macro1.Param1.Depth + 64));
            data.Add((byte)(Macro1.Param2.Depth + 64));
            data.Add((byte)(Macro2.Param1.Depth + 64));
            data.Add((byte)(Macro2.Param2.Depth + 64));
            data.Add((byte)(Macro3.Param1.Depth + 64));
            data.Add((byte)(Macro3.Param2.Depth + 64));
            data.Add((byte)(Macro4.Param1.Depth + 64));
            data.Add((byte)(Macro4.Param2.Depth + 64));

            data.Add((byte)Switch1);
            data.Add((byte)Switch2);
            data.Add((byte)FootSwitch1);
            data.Add((byte)FootSwitch2);

            return data.ToArray();
        }

        private ReverbSettings GetReverb(byte[] data, int offset)
        {
            ReverbSettings reverb = new ReverbSettings();
            reverb.Type = data[offset];
            reverb.DryWet = data[offset + 1];
            reverb.Param1 = data[offset + 2];
            reverb.Param2 = data[offset + 3];
            reverb.Param3 = data[offset + 4];
            reverb.Param4 = data[offset + 5];
            return reverb;
        }

        private EffectSettings GetEffect(byte[] data, int offset)
        {
            EffectSettings effect = new EffectSettings();
            effect.Type = data[offset];
            effect.Depth = data[offset + 1];
            effect.Param1 = data[offset + 2];
            effect.Param2 = data[offset + 3];
            effect.Param3 = data[offset + 4];
            effect.Param4 = data[offset + 5];
            return effect;
        }

        private GEQSettings GetGEQ(byte[] data, int offset)
        {
            GEQSettings g = new GEQSettings();

            // 58(-6) ~ 70(+6), so 64 is zero
            g.Freq1 = (sbyte)(data[offset] - 64);
            g.Freq2 = (sbyte)(data[offset + 1] - 64);
            g.Freq3 = (sbyte)(data[offset + 2] - 64);
            g.Freq4 = (sbyte)(data[offset + 3] - 64);
            g.Freq5 = (sbyte)(data[offset + 4] - 64);
            g.Freq6 = (sbyte)(data[offset + 5] - 64);
            g.Freq7 = (sbyte)(data[offset + 6] - 64);
            return g;
        }

        private string GetName(byte[] data, int offset)
        {
            // Brute-forcing the name in from params 40 ... 47
            byte[] bytes = 
            { 
                data[offset], 
                data[offset + 1], 
                data[offset + 2], 
                data[offset + 3], 
                data[offset + 4], 
                data[offset + 5], 
                data[offset + 6], 
                data[offset + 7]
            };
            string name = Encoding.ASCII.GetString(bytes);
            return name;
        }        
    }
}