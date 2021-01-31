using System;

namespace Z_nthCommon
{
    /// <summary>
    /// My "Oscillator" "Class"
    /// </summary>
    static public class Phase
    {
        public const double halfPI = Math.PI / 2;
        public const double oneandhalfPI = Math.PI * 1.5;
        public const double twoPI = Math.PI * 2;

        public const int SuperSawSpreadCount = 3;
        public const int SuperSawSpreadCountDivider = (SuperSawSpreadCount * 2) + 1;

        static public double Correction(double phase)
        {
            if (phase >= 0 && phase < twoPI) return phase;
            if (phase == 2 * Math.PI) return 0;
            return phase - (Math.Floor(phase / twoPI) * twoPI);
        }

        static public double Square(double phase, double p = 1)
        {
            //No reason to have 0 or 2 because that is silence...
            if (p < 0.01) p = 0.01;
            if (p > 1.99) p = 1.99;
            double pulsepos = p * Math.PI;
            return phase < pulsepos ? 1 : -1;
        }

        static public double Saw(double phase, double p = 1)
        {
            if (p == 1)
            {
                return phase <= Math.PI ? phase / Math.PI : ((phase - Math.PI) / Math.PI) - 1;
            }
            //Supersaw.
            //Well, not really. This is just phase shifting, not freq diff.
            double result = Saw(phase);
            p = p - 1;
            p = p / 10;

            for (int i = 1; i <= SuperSawSpreadCount; i++)
            {
                result += Saw(Correction(phase + i * p));
                result += Saw(Correction(phase - i * p));
            }
            result = result / SuperSawSpreadCountDivider;
            return result;
        }

        static public double Triangle(double phase, double p = 1)
        {
            if (p == 1)
            {
                if (phase <= halfPI) return phase / halfPI;
                if (phase > halfPI && phase < oneandhalfPI) return 1 - ((phase - halfPI) / halfPI);
                return ((phase - oneandhalfPI) / halfPI) - 1;
            }
            double p1 = p * halfPI;
            double p2 = twoPI - (p * halfPI);
            if (p == 0) return 0 - ((phase - Math.PI) / (Math.PI * (1 - p / 2)));
            if (phase >= 0 && phase <= p1) return 2 * phase / (p * Math.PI);
            if (phase > p1 && phase < p2) return 0 - ((phase - Math.PI) / (Math.PI * (1 - p / 2)));
            return (2 * phase / (p * Math.PI)) - (4 / p);
        }

        static public double Sine(double phase, double p = 1)
        {
            if (p == 1) return Math.Sin(phase);
            else
            {
                double p1 = p * halfPI;
                double p2 = twoPI - (p * halfPI);
                if (p == 0) return Math.Sin(Math.PI * (0.5 + (phase - p1) / (p2 - p1)));
                if (phase >= 0 && phase <= p1) return Math.Sin(phase / p);
                if (phase > p1 && phase < p2) return Math.Sin(Math.PI * (0.5 + (phase - p1) / (p2 - p1)));
                return -Math.Sin((twoPI - phase) / p);
            }
        }
        /// <summary>
        /// Returns "oscillator" value as function of phase. Result between -1 and 1.
        /// </summary>
        /// <param name="CurrentOption">WaveForm.</param>
        /// <param name="phase">The phase angle of the oscillator. Between 0 and 2PI. If not, it will be.</param>
        /// <param name="currentsamplevalue"></param>
        /// <param name="p">0 to 2. PWM like stuff. Except it works for sine and triangle too. 1 is normal waveform.</param>
        static public void Waveformswitcher(Waveform CurrentOption, double phase, ref double currentsamplevalue, double p = 1)
        {
            double correctedphase = Z_nthCommon.Phase.Correction(phase);
            switch (CurrentOption)
            {
                case Waveform.Sine:
                    currentsamplevalue += Z_nthCommon.Phase.Sine(correctedphase, p);
                    break;
                case Waveform.Saw:
                    currentsamplevalue += Z_nthCommon.Phase.Saw(correctedphase, p);
                    break;
                case Waveform.Square:
                    currentsamplevalue += Z_nthCommon.Phase.Square(correctedphase, p);
                    break;
                case Waveform.Triangle:
                    currentsamplevalue += Z_nthCommon.Phase.Triangle(correctedphase, p);
                    break;
                default:
                    break;
            }
        }
    }
}
