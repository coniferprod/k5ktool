using System;
using System.Collections.Generic;
using System.Text;

namespace K5KTool
{
    public class ReverbName
    {
        public string Name;
        public string[] ParameterNames;
    }

    public class ReverbSettings
    {
        public byte Type;  // 0 ~ 10
        public byte DryWet;  // 0 ~ 100
        
        public byte Param1;  // 0 ~ 127
        public byte Param2;  // 0 ~ 127
        public byte Param3;  // 0 ~ 127
        public byte Param4;  // 0 ~ 127

        public static ReverbName[] ReverbNames = 
        {
            /*  0 */ new ReverbName { Name = "Hall 1", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  1 */ new ReverbName { Name = "Hall 2", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  2 */ new ReverbName { Name = "Hall 3", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  3 */ new ReverbName { Name = "Room 1", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  4 */ new ReverbName { Name = "Room 2", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  5 */ new ReverbName { Name = "Room 3", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  6 */ new ReverbName { Name = "Plate 1", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  7 */ new ReverbName { Name = "Plate 2", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  8 */ new ReverbName { Name = "Plate 3", ParameterNames = new string[] { "Dry/Wet 2", "Reverb Time", "Predelay Time", "High Frequency Damping" } },
            /*  9 */ new ReverbName { Name = "Reverse", ParameterNames = new string[] { "Dry/Wet 2", "Feedback", "Predelay Time", "High Frequency Damping" } },
            /* 10 */ new ReverbName { Name = "Long Delay", ParameterNames = new string[]{ "Dry/Wet 2", "Feedback", "Delay Time", "High Frequency Damping" } }
        };

        public ReverbSettings()
        {
            Type = 0;
            DryWet = 50;
            Param1 = 50;
            Param2 = 64;
            Param3 = 64;
            Param4 = 64;
        }

        public override string ToString()
        {
            ReverbName name = ReverbNames[Type];
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("{0}, dry/wet = {1}\n", name.Name, DryWet));
            builder.Append(String.Format("P1 {0} = {1}\n", name.ParameterNames[0], Param1));
            builder.Append(String.Format("P2 {0} = {1}\n", name.ParameterNames[1], Param2));
            builder.Append(String.Format("P3 {0} = {1}\n", name.ParameterNames[2], Param3));
            builder.Append(String.Format("P4 {0} = {1}\n", name.ParameterNames[3], Param4));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add(Type);
            data.Add(DryWet);
            data.Add(Param1);
            data.Add(Param2);
            data.Add(Param3);
            data.Add(Param4);
            return data.ToArray();
        }
    }
}
