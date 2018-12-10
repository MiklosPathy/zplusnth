using System;

namespace Z_nthCommon
{
    static public class Phase
    {
        static public double Correction(double phase)
        {
            if (phase >= 0 && phase < 2 * Math.PI) return phase;
            if (phase == 2 * Math.PI) return 0;
            return phase - (Math.Floor(phase / (2 * Math.PI)) * 2 * Math.PI);
        }

        static public double Square(double phase)
        {
            return phase < Math.PI ? 1 : -1;
        }

        static public double Saw(double phase)
        {
            return phase <= Math.PI ? phase / Math.PI : ((phase - Math.PI) / Math.PI) - 1;
        }

        static private readonly double halfPI = Math.PI / 2;
        static private readonly double oneandhalfPI = Math.PI * 1.5;

        static public double Triangle(double phase)
        {
            if (phase <= halfPI) return phase / halfPI;
            if (phase > halfPI && phase < oneandhalfPI) return 1 - ((phase - halfPI) / halfPI);
            return ((phase - oneandhalfPI) / halfPI) - 1;
        }

        static public void Waveformswitcher(Waveform CurrentOption, double phase, ref double currentsamplevalue)
        {
            double correctedphase = Z_nthCommon.Phase.Correction(phase);
            switch (CurrentOption)
            {
                case Waveform.Sine:
                    currentsamplevalue += Math.Sin(correctedphase);
                    break;
                case Waveform.Saw:
                    currentsamplevalue += Z_nthCommon.Phase.Saw(correctedphase);
                    break;
                case Waveform.Square:
                    currentsamplevalue += Z_nthCommon.Phase.Square(correctedphase);
                    break;
                case Waveform.Triangle:
                    currentsamplevalue += Z_nthCommon.Phase.Triangle(correctedphase);
                    break;
                default:
                    break;
            }
        }
    }
}
