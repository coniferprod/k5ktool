using System;
using System.Collections.Generic;

namespace K5KTool
{
    public class LeiterParameters
    {
        public double A;
        public double B;
        public double C;
        public double XP;
        public double D;
        public double E;
        public double YP;
    }

    public class LeiterEngine
    {
        public static readonly Dictionary<string, LeiterParameters> WaveformParameters = new Dictionary<string, LeiterParameters>
        {
            { "saw", new LeiterParameters { A = 1.0, B = 0.0, C = 0.0, XP = 0.0, D = 0.0, E = 0.0, YP = 0.0 } },
            { "square", new LeiterParameters { A = 1.0, B = 1.0, C = 0.0, XP = 0.5, D = 0.0, E = 0.0, YP = 0.0 } },
            { "triangle", new LeiterParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.5, D = 0.0, E = 0.0, YP = 0.0 } },
            { "pulse20", new LeiterParameters { A = 1.0, B = 1.0, C = 0.0, XP = 0.2, D = 0.0, E = 0.0, YP = 0.0 } },
            { "pluckedstring", new LeiterParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.2, D = 0.0, E = 0.0, YP = 0.0 } },  // 20% uneven triangle
            { "brassy", new LeiterParameters { A = 2.0, B = 2.0, C = 0.0, XP = 0.1, D = 0.0, E = 0.0, YP = 0.0 } },  // 10% triangular pulse
            { "analogsquare", new LeiterParameters { A = 3.0, B = 1.0, C = 0.0, XP = 0.48, D = 2.0, E = 0.0, YP = 0.035 } },
            { "oboe", new LeiterParameters { A = 0.4, B = 1.0, C = 0.0, XP = 0.12, D = 0.0, E = 1.0, YP = 0.47 } },
            { "trombone", new LeiterParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.045, D = 1.0, E = 1.0, YP = 0.0625 } },
            { "frenchhorn", new LeiterParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.09, D = 1.0, E = 0.0, YP = 0.13 } }
        };

        public static double Compute(int number, LeiterParameters para)
        {
            double n = (double)number;
            double a = para.A;
            double b = para.B;
            double c = para.C;
            double x = n * Math.PI * para.XP;
            double y = n * Math.PI * para.YP;
            double d = para.D;
            double e = para.E;
        
            double module1 = 1.0 / Math.Pow(n, a);
            double module2 = Math.Pow(Math.Sin(x), b) * Math.Pow(Math.Cos(x), c);
            double module3 = Math.Pow(Math.Sin(y), d) * Math.Pow(Math.Cos(y), e);
        
            return module1 * module2 * module3;
        }

        public static int GetHarmonicLevel(int harmonicNumber, LeiterParameters para, int maxLevel = 99) 
        {
            double aMax = 1.0;
            double a = LeiterEngine.Compute(harmonicNumber, para);
            double v = Math.Log(Math.Abs(a / aMax));
            double level = ((double) maxLevel) + 8.0 * v;
            System.Console.WriteLine(String.Format("DEBUG: n = {0}, a = {1}, v = {2}", harmonicNumber, a, v));
            if (level < 0)
            {
                return 0;
            }
            return (int) Math.Floor(level);
        }

        public static byte[] GetHarmonicLevels(string waveformName, int count, int maxLevel)
        {
            LeiterParameters para = WaveformParameters[waveformName];
            List<byte> levels = new List<byte>();
            var n = 0;
            while (n < count)
            {
                levels.Add((byte)LeiterEngine.GetHarmonicLevel(n + 1, para, maxLevel));
                n++;
            }
            return levels.ToArray();
        }
    }    
}