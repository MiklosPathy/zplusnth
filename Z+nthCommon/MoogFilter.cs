using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z_nthCommon
{
    public class MoogFilter
    {
        // Moog 24 dB/oct resonant lowpass VCF
        // References: CSound source code, Stilson/Smith CCRMA paper.
        // Modified by paul.kellett@maxim.abel.co.uk July 2000

        private double f, p, q;             //filter coefficients
        private double b0 = 1, b1 = 1, b2 = 1, b3 = 1, b4 = 1;  //filter buffers (beware denormals!)
        private double t1, t2;              //temporary buffers
        private double frequency = 0.5d, resonance = 0.5d;

        public MoogFilter()
        {
            Coefficients();
        }

        // Set coefficients given frequency & resonance [0.0...1.0]
        private void Coefficients()
        {
            q = 1.0d - frequency;
            p = frequency + 0.8d * frequency * q;
            f = p + p - 1.0d;
            q = resonance * (1.0d + 0.5d * q * (1.0d - q + 5.6d * q * q));
        }

        // Filter (in [-1.0...+1.0])
        public void Process(ref double currentsample)
        {
            currentsample -= q * b4;                          //feedback
            t1 = b1;
            b1 = (currentsample + b0) * p - b1 * f;
            t2 = b2;
            b2 = (b1 + t1) * p - b2 * f;
            t1 = b3;
            b3 = (b2 + t2) * p - b3 * f;
            b4 = (b3 + t1) * p - b4 * f;
            b4 = b4 - b4 * b4 * b4 * 0.166667d;    //clipping
            b0 = currentsample;

            // Lowpass  output:  b4
            // Highpass output:  in - b4;
            // Bandpass output:  3.0f * (b3 - b4);

            currentsample = b4;
        }

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; Coefficients(); }
        }
        public double Resonance
        {
            get { return resonance; }
            set { resonance = value; Coefficients(); }
        }
    }


}
