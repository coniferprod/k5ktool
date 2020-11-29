using System;
using System.Collections.Generic;

namespace K5KTool
{
    public class WaveformParameters
    {
        public double A;
        public double B;
        public double C;
        public double XP;
        public double D;
        public double E;
        public double YP;
    }

    public class WaveformEngine
    {
        public static readonly Dictionary<string, WaveformParameters> WaveformParameters = new Dictionary<string, WaveformParameters>
        {
            { "Saw", new WaveformParameters { A = 1.0, B = 0.0, C = 0.0, XP = 0.0, D = 0.0, E = 0.0, YP = 0.0 } },
            { "Square", new WaveformParameters { A = 1.0, B = 1.0, C = 0.0, XP = 0.5, D = 0.0, E = 0.0, YP = 0.0 } },
            { "Triangle", new WaveformParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.5, D = 0.0, E = 0.0, YP = 0.0 } },
            { "Pulse20", new WaveformParameters { A = 1.0, B = 1.0, C = 0.0, XP = 0.2, D = 0.0, E = 0.0, YP = 0.0 } },
            { "PluckedString", new WaveformParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.2, D = 0.0, E = 0.0, YP = 0.0 } },  // 20% uneven triangle
            { "Brassy", new WaveformParameters { A = 2.0, B = 2.0, C = 0.0, XP = 0.1, D = 0.0, E = 0.0, YP = 0.0 } },  // 10% triangular pulse
            { "AnalogSquare", new WaveformParameters { A = 3.0, B = 1.0, C = 0.0, XP = 0.48, D = 2.0, E = 0.0, YP = 0.035 } },
            { "Oboe", new WaveformParameters { A = 0.4, B = 1.0, C = 0.0, XP = 0.12, D = 0.0, E = 1.0, YP = 0.47 } },
            { "Trombone", new WaveformParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.045, D = 1.0, E = 1.0, YP = 0.0625 } },
            { "FrenchHorn", new WaveformParameters { A = 2.0, B = 1.0, C = 0.0, XP = 0.09, D = 1.0, E = 0.0, YP = 0.13 } }
        };

        public static double Compute(int number, WaveformParameters para)
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

        public static int GetHarmonicLevel(int harmonicNumber, WaveformParameters para, int maxLevel = 99)
        {
            double aMax = 1.0;
            double a = WaveformEngine.Compute(harmonicNumber, para);
            double v = Math.Log(Math.Abs(a / aMax));
            double level = ((double) maxLevel) + 8.0 * v;
            //System.Console.WriteLine(String.Format("DEBUG: n = {0}, a = {1}, v = {2}", harmonicNumber, a, v));
            if (level < 0)
            {
                return 0;
            }
            return (int) Math.Floor(level);
        }

        public static byte[] GetHarmonicLevels(string waveformName, int count, int maxLevel)
        {
            WaveformParameters parameters = WaveformParameters[waveformName];
            return GetCustomHarmonicLevels(parameters, count, maxLevel);
        }

        public static byte[] GetCustomHarmonicLevels(WaveformParameters parameters, int count, int maxLevel)
        {
            List<byte> levels = new List<byte>();
            var n = 0;
            while (n < count)
            {
                levels.Add((byte)WaveformEngine.GetHarmonicLevel(n + 1, parameters, maxLevel));
                n++;
            }
            return levels.ToArray();
        }
    }
}