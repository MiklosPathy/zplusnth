using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using Z_nthCommon;

namespace PunkOrgan
{
    public class DrawBar
    {
        public int Volume { get; set; }
        public double FreqMul;
        public double[] Phase;
    }

    public class PunkHammond : Zplusnthbase
    {
        private static readonly int echobuffersize = 20000;

        public DrawBar[] Drawbars { get; set; }

        public int Leslie_Rate { get; set; }
        public int Leslie_Freq { get; set; }

        public int Echo_Rate { get; set; }
        public int Echo_Freq { get; set; }

        private short[] echobuffer;

        public PunkHammond()
        {
            Drawbars = new DrawBar[10];
            //Thanks to: http://www.jessedeanefreeman.com/hammondstuff.html

            Drawbars[1] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 6, FreqMul = 0.5 };
            Drawbars[2] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 8, FreqMul = Math.Pow(halfnotemultiplier, 7) };
            Drawbars[3] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 8, FreqMul = 1 };
            Drawbars[4] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 8, FreqMul = 2 };
            Drawbars[5] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 7, FreqMul = 2 * Math.Pow(halfnotemultiplier, 7) };
            Drawbars[6] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 6, FreqMul = 4 };
            Drawbars[7] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 5, FreqMul = 4 * Math.Pow(halfnotemultiplier, 4) };
            Drawbars[8] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 4, FreqMul = 4 * Math.Pow(halfnotemultiplier, 7) };
            Drawbars[9] = new DrawBar() { Phase = new double[maxPolyPhony], Volume = 3, FreqMul = 8 };

            Leslie_Freq = 3;
            Leslie_Rate = 1;

            echobuffer = new short[echobuffersize];
            Echo_Freq = 10000;
            Echo_Rate = 50;

            CurrentPolyphony = 3;

            OverDrive = 10;

        }




        double Leslie_Phase = 0;
        int echophase = 0;

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                double bending = Bending;

                Leslie_Phase += 2 * Math.PI / WaveFormat.SampleRate * Leslie_Freq;
                if (Leslie_Phase > 2 * Math.PI) Leslie_Phase -= 2 * Math.PI;
                double leslie = Math.Sin(Leslie_Phase) * Leslie_Rate;

                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Notes[channel] > 0)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Notes[channel] + leslie + bending);
                        for (int drawbar = 1; drawbar < 10; drawbar++)
                        {
                            Drawbars[drawbar].Phase[channel] += commonsinpart * Drawbars[drawbar].FreqMul;
                            if (Drawbars[drawbar].Phase[channel] > 2 * Math.PI) Drawbars[drawbar].Phase[channel] -= 2 * Math.PI;
                            currentsamplevalue += (Math.Sin(Drawbars[drawbar].Phase[channel]) * Drawbars[drawbar].Volume);
                        }
                    }
                }
                //The 90 divider is for the drawbars. There are 9 of them. They have 10 position here.
                currentsamplevalue = currentsamplevalue / 90 * short.MaxValue / CurrentPolyphony;

                if (overDrive != 10) currentsamplevalue = overdrive(currentsamplevalue * overDrive / 10);
                //currentsamplevalue = distortion(currentsamplevalue, overDrive / 10, 1);


                currentsamplevalue = currentsamplevalue + echobuffer[limitechophase(echophase + Echo_Freq)] * Echo_Rate / 100;

                if (currentsamplevalue > short.MaxValue) currentsamplevalue = currentsamplevalue - short.MaxValue;

                buffer[sample + offset] = (short)currentsamplevalue;
                echobuffer[echophase] = (short)currentsamplevalue;
                echophase++;
                echophase = limitechophase(echophase);
            }
            return sampleCount;
        }

        //Buffer looping
        private int limitechophase(int phase)
        {
            return phase < echobuffersize ? phase : phase - echobuffersize;
        }

        private int overDrive;
        public int OverDrive
        {
            get { return overDrive; }
            set
            {
                overDrive = value;
                if (overDrive == 0)
                {
                    th = short.MaxValue;
                    th2 = short.MaxValue;
                }
                else
                {
                    th = short.MaxValue / 3;
                    th2 = th * 2;
                }
            }
        }

        private double th; // threshold for symmetrical soft clipping
        private double th2;
        const double maxvalue2 = short.MaxValue * 2;
        const double maxvalue3 = short.MaxValue * 3;

        private double overdrive(double input)            //by Schetzen Formula
        {
            double absinput = Math.Abs(input);
            if (absinput < th) return 2 * input;
            int signinput = Math.Sign(input);
            if (absinput < th2)
            {
                return (maxvalue3 - Math.Pow((maxvalue2 - absinput * maxvalue3), 2)) / maxvalue3 * signinput;
            }
            return signinput * short.MaxValue;
        }

        private double distortion(double x, double gain, double mix)
        {
            // Distortion based on an exponential function
            // x - input
            // gain - amount of distortion, >0->
            // mix - mix of original and distorted sound, 1=only distorted
            if (x == 0) return 0;
            x = x / short.MaxValue;

            double y = x / Math.Abs(x) * (1 - Math.Pow(Math.E, gain * x * x / Math.Abs(x)));
            return y * short.MaxValue;


            //double q = x * gain / Math.Abs(x);
            //double z = Math.Sign(-q) * (1 - Math.Exp(Math.Sign(-q) * q));
            //double y = mix * z * Math.Abs(x) / Math.Abs(z) + (1 - mix) * x;
            //y = y * Math.Abs(x) / Math.Abs(y);


        }
    }
}
