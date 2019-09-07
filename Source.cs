using System;
using System.Text;
using System.Collections.Generic;

namespace K5KTool
{
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

            DCO = new DCOSettings(data, offset);
            offset += DCO.ToData().Length;

            DCF = new DCFSettings(data, offset);
            offset += DCF.ToData().Length;

            DCA = new DCASettings(data, offset);
            offset += DCA.ToData().Length;

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