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

        static public double Square(double phase, double PWM = 50)
        {
            double pulsepos = PWM / 100 * twoPI;
            return phase < pulsepos ? 1 : -1;
        }

        static public double Saw(double phase)
        {
            return phase <= Math.PI ? phase / Math.PI : ((phase - Math.PI) / Math.PI) - 1;
        }

        static private readonly double halfPI = Math.PI / 2;
        static private readonly double oneandhalfPI = Math.PI * 1.5;
        static private readonly double twoPI = Math.PI * 2;

        static public double Triangle(double phase, double PWM = 50)
        {
            if (PWM == 50)
            {
                if (phase <= halfPI) return phase / halfPI;
                if (phase > halfPI && phase < oneandhalfPI) return 1 - ((phase - halfPI) / halfPI);
                return ((phase - oneandhalfPI) / halfPI) - 1;
            }
            double p = PWM * 2 / 100;
            double p1 = p * halfPI;
            double p2 = twoPI - (p * halfPI);
            if (p == 0) return 0 - ((phase - Math.PI) / Math.PI - (p * halfPI));
            if (phase > 0 && phase <= p1) return phase / (p * halfPI);
            if (phase > p1 && phase < p2) return 0 - ((phase - Math.PI) / Math.PI - (p * halfPI));
            return (phase / (p * halfPI)) - (4 / p);
        }

        static public void Waveformswitcher(Waveform CurrentOption, double phase, ref double currentsamplevalue, double PWM = 50)
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
                    currentsamplevalue += Z_nthCommon.Phase.Square(correctedphase, PWM);
                    break;
                case Waveform.Triangle:
                    currentsamplevalue += Z_nthCommon.Phase.Triangle(correctedphase, PWM);
                    break;
                default:
                    break;
            }
        }
    }
}
