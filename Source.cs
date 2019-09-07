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
        public byte VelocityCurve;
        public AmplifierEnvelope Envelope;
        public KeyScalingControlEnvelope KeyScaling;
        public VelocityControlEnvelope VelocitySensitivity;

        public DCASettings()
        {
            Envelope = new AmplifierEnvelope();
            KeyScaling = new KeyScalingControlEnvelope();
            VelocitySensitivity = new VelocityControlEnvelope();
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

            data.Add((byte)(VelocityCurve - 1));

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

    public enum LFOWaveform
    {
        Triangle,
        Square,
        Sawtooth,
        Sine,
        Random
    }

    public class LFOControl
    {
        public byte Depth; // 0 ~ 63
        public sbyte KeyScaling; // (-63)1 ~ (+63)127
    }

    public class LFOSettings
    {
        public LFOWaveform Waveform;
        public byte Speed;
        public byte DelayOnset;
        public byte FadeInTime;
        public byte FadeInToSpeed;
        public LFOControl Vibrato;
        public LFOControl Growl;
        public LFOControl Tremolo;

        public LFOSettings()
        {
            Vibrato = new LFOControl();
            Growl = new LFOControl();
            Tremolo = new LFOControl();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Waveform={0}  Speed={1}  Delay Onset={2}\n", Waveform, Speed, DelayOnset));
            builder.Append(String.Format("Fade In Time={0}  Fade In To Speed={1}\n", FadeInTime, FadeInToSpeed));
            builder.Append("LFO Modulation:\n");
            builder.Append(String.Format("Vibrato(DCO) = {0}   KS To Vibrato={1}\n", Vibrato.Depth, Vibrato.KeyScaling));
            builder.Append(String.Format("Growl(DCF) = {0}   KS To Growl={1}\n", Growl.Depth, Growl.KeyScaling));
            builder.Append(String.Format("Tremolo(DCA) = {0}   KS To Tremolo={1}\n", Tremolo.Depth, Tremolo.KeyScaling));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)Waveform);
            data.Add(Speed);
            data.Add(DelayOnset);
            data.Add(FadeInTime);
            data.Add(FadeInToSpeed);

            data.Add(Vibrato.Depth);
            data.Add((byte)(Vibrato.KeyScaling + 64));

            data.Add(Growl.Depth);
            data.Add((byte)(Growl.KeyScaling + 64));

            data.Add(Tremolo.Depth);
            data.Add((byte)(Tremolo.KeyScaling + 64));

            return data.ToArray();
        }
    }

    public enum VelocitySwitchType
    {
        Off,
        Loud,
        Soft
    }

    public class VelocitySwitchSettings
    {
        public VelocitySwitchType Type;
        public int Velocity;  // 31 ~ 127
    }
    
    public class ModulationSettings
    {
        public ControlDestination Destination;
        public int Depth;
    }

    public class ControllerSettings
    {
        public ModulationSettings Destination1;
        public ModulationSettings Destination2;

        public ControllerSettings()
        {
            Destination1 = new ModulationSettings();
            Destination2 = new ModulationSettings();
        }
    }

    public class AssignableController
    {
        public ControlSource Source;
        public ModulationSettings Target;

        public AssignableController()
        {
            Target = new ModulationSettings();
        }
    }

    public enum PanType
    {
        Normal,
        KeyScaling,
        NegativeKeyScaling,
        Random
    }

    public class Source
    {
        public static int DataSize = 86;

        public byte ZoneLow;
        public byte ZoneHigh;
        public VelocitySwitchSettings VelocitySwitch;
        public byte EffectPath;
        public byte Volume;
        public byte BenderPitch;
        public byte BenderCutoff;

        public ControllerSettings Press;
        public ControllerSettings Wheel;
        public ControllerSettings Express;
        public AssignableController Assign1;        
        public AssignableController Assign2;

        public int KeyOnDelay;
        public PanType Pan;
        public sbyte NormalPanValue;  // (63L)1 ~ (63R)127

        public DCOSettings DCO;
        public DCFSettings DCF;
        public DCASettings DCA;
        public LFOSettings LFO;

        public AdditiveKit ADD;

        public Source()
        {
            ZoneLow = 0;
            ZoneHigh = 127;
            VelocitySwitch = new VelocitySwitchSettings();
            Volume = 120;
            Press = new ControllerSettings();
            Wheel = new ControllerSettings();
            Express = new ControllerSettings();
            Assign1 = new AssignableController();
            Assign2 = new AssignableController();
            DCO = new DCOSettings();
            DCF = new DCFSettings();
            DCA = new DCASettings();
            LFO = new LFOSettings();
            ADD = new AdditiveKit();
        }

        public Source(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);
            ZoneLow = b;

            (b, offset) = Util.GetNextByte(data, offset);
            ZoneHigh = b;

            (b, offset) = Util.GetNextByte(data, offset);
            VelocitySwitch = new VelocitySwitchSettings();
            VelocitySwitch.Type = (VelocitySwitchType)(b >> 5);
            VelocitySwitch.Velocity = (b & 0x1F);
            System.Console.WriteLine(String.Format("velo sw original value = {0:X2}", b));
            
            (b, offset) = Util.GetNextByte(data, offset);
            EffectPath = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Volume = b;
            
            (b, offset) = Util.GetNextByte(data, offset);
            BenderPitch = b;

            (b, offset) = Util.GetNextByte(data, offset);
            BenderCutoff = b;

            Press = new ControllerSettings();
            (b, offset) = Util.GetNextByte(data, offset);
            Press.Destination1.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Press.Destination1.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Press.Destination2.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Press.Destination2.Depth = b;

            Wheel = new ControllerSettings();
            (b, offset) = Util.GetNextByte(data, offset);
            Wheel.Destination1.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Wheel.Destination1.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Wheel.Destination2.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Wheel.Destination2.Depth = b;

            Express = new ControllerSettings();
            (b, offset) = Util.GetNextByte(data, offset);
            Express.Destination1.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Express.Destination1.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Express.Destination2.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Express.Destination2.Depth = b;

            Assign1 = new AssignableController();
            (b, offset) = Util.GetNextByte(data, offset);
            Assign1.Source = (ControlSource)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Assign1.Target.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Assign1.Target.Depth = b;

            Assign2 = new AssignableController();
            (b, offset) = Util.GetNextByte(data, offset);
            Assign2.Source = (ControlSource)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Assign2.Target.Destination = (ControlDestination)b;
            (b, offset) = Util.GetNextByte(data, offset);
            Assign2.Target.Depth = b;

            (b, offset) = Util.GetNextByte(data, offset);
            KeyOnDelay = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Pan = (PanType)b;
            (b, offset) = Util.GetNextByte(data, offset);
            NormalPanValue = (sbyte)(b - 64);

            // DCO
            System.Console.WriteLine(String.Format("Parsing DCO wave kit number, at offset {0:X8} (from source data start)", offset));

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

            DCO = new DCOSettings();

            DCO.WaveNumber = waveNumber;

            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Coarse = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Fine = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.FixedKey = b;
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.KSPitch = (KeyScalingToPitch)b;
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.StartLevel = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.AttackTime = b;
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.AttackLevel = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.DecayTime = b;
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.TimeVelocitySensitivity = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCO.Envelope.LevelVelocitySensitivity = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF = new DCFSettings();
            DCF.IsActive = (b == 0);  // 0=Active, 1=Bypass

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Mode = (FilterMode) b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.VelocityCurve = (byte)(b + 1);  // 0~11 (1~12)

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Resonance = b;  // 0~7

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Level = b;  // 0~7 (7~0)

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Cutoff = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.CutoffKeyScalingDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.CutoffVelocityDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.EnvelopeDepth = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.AttackTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.Decay1Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.Decay1Level = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.Decay2Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.Decay2Level = (sbyte)(b - 64);

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.Envelope.ReleaseTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCF.KSToEnvAttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCF.KSToEnvDecay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCF.VelocityToEnvDepth = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCF.VelocityToEnvAttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCF.VelocityToEnvDecay1Time = (sbyte)(b - 64);

            DCA = new DCASettings();

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.VelocityCurve = (byte)(b + 1);

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.AttackTime = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.Decay1Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.Decay1Level = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.Decay2Time = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.Decay2Level = b;

            (b, offset) = Util.GetNextByte(data, offset);
            DCA.Envelope.ReleaseTime = b;

            DCA.KeyScaling = new KeyScalingControlEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.KeyScaling.Level = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.KeyScaling.AttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.KeyScaling.Decay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.KeyScaling.ReleaseTime = (sbyte)(b - 64);

            DCA.VelocitySensitivity = new VelocityControlEnvelope();
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.VelocitySensitivity.Level = b;
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.VelocitySensitivity.AttackTime = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.VelocitySensitivity.Decay1Time = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            DCA.VelocitySensitivity.ReleaseTime = (sbyte)(b - 64);

            LFO = new LFOSettings();
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Waveform = (LFOWaveform) b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Speed = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.DelayOnset = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.FadeInTime = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.FadeInToSpeed = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Vibrato.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Vibrato.KeyScaling = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Growl.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Growl.KeyScaling = (sbyte)(b - 64);
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Tremolo.Depth = b;
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Tremolo.KeyScaling = (sbyte)(b - 64);

            ADD = new AdditiveKit();
            /*
            if (DCO.WaveNumber == AdditiveKit.WaveNumber)
            {
                byte[] additiveData = new byte[AdditiveKit.DataSize];
                Buffer.BlockCopy(data, offset, additiveData, 0, AdditiveKit.DataSize);
                ADD = new AdditiveKit(additiveData);
            }
             */
            // Don't automatically add the wave kit data, since we may not have it in our buffer
        }

        public override string ToString() 
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Zone: low = {0}, high = {1}\n", ZoneLow, ZoneHigh));
            builder.Append(String.Format("Vel. sw type = {0}, velocity = {1}\n", VelocitySwitch.Type, VelocitySwitch.Velocity));
            builder.Append(String.Format("Effect path = {0}\n", EffectPath));
            builder.Append(String.Format("Volume = {0}\n", Volume));
            builder.Append(String.Format("Bender Pitch = {0}  Bender Cutoff = {1}\n", BenderPitch, BenderCutoff));
            builder.Append(String.Format("Key ON Delay = {0}\n", KeyOnDelay));
            builder.Append(String.Format("Pan type = {0}, value = {1}\n", Pan, NormalPanValue));
            builder.Append(String.Format("DCO:\n{0}\n", DCO.ToString()));
            builder.Append(String.Format("DCF:\n{0}\n", DCF.ToString()));
            builder.Append(String.Format("DCA:\n{0}\n", DCA.ToString()));
            builder.Append(String.Format("LFO:\n{0}\n", LFO.ToString()));

            if (DCO.WaveNumber == AdditiveKit.WaveNumber)
            {
                builder.Append(String.Format("ADD data:\n{0}", ADD.ToString()));
            }
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add(ZoneLow);
            data.Add(ZoneHigh);

            uint typeValue = (uint) VelocitySwitch.Type;
            uint velocityValue = (uint) VelocitySwitch.Velocity;
            uint outValue = (typeValue << 5) | velocityValue;
            data.Add((byte)velocityValue);

            data.Add(EffectPath);
            data.Add(Volume);
            data.Add(BenderPitch);
            data.Add(BenderCutoff);

            data.Add((byte)Press.Destination1.Destination);
            data.Add((byte)Press.Destination1.Depth);
            data.Add((byte)Press.Destination2.Destination);
            data.Add((byte)Press.Destination2.Depth);

            data.Add((byte)Wheel.Destination1.Destination);
            data.Add((byte)Wheel.Destination1.Depth);
            data.Add((byte)Wheel.Destination2.Destination);
            data.Add((byte)Wheel.Destination2.Depth);
            
            data.Add((byte)Express.Destination1.Destination);
            data.Add((byte)Express.Destination1.Depth);
            data.Add((byte)Express.Destination2.Destination);
            data.Add((byte)Express.Destination2.Depth);

            data.Add((byte)Assign1.Source);
            data.Add((byte)Assign1.Target.Destination);
            data.Add((byte)Assign1.Target.Depth);

            data.Add((byte)Assign2.Source);
            data.Add((byte)Assign2.Target.Destination);
            data.Add((byte)Assign2.Target.Depth);

            data.Add((byte)KeyOnDelay);
            data.Add((byte)Pan);
            data.Add((byte)(NormalPanValue + 64));

            byte[] dcoData = DCO.ToData();
            foreach (byte b in dcoData)
            {
                data.Add(b);
            }

            byte[] dcfData = DCF.ToData();
            foreach (byte b in dcfData)
            {
                data.Add(b);
            }

            byte[] dcaData = DCA.ToData();
            foreach (byte b in dcaData)
            {
                data.Add(b);
            }

            byte[] lfoData = LFO.ToData();
            foreach (byte b in lfoData)
            {
                data.Add(b);
            }

            if (DCO.WaveNumber == AdditiveKit.WaveNumber)
            {
                byte[] additiveData = ADD.ToData();
                foreach (byte b in additiveData)
                {
                    data.Add(b);
                }
            }

            return data.ToArray();
        }
    }
}