using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace PunkOrgan
{
    public class DrawBar
    {
        public int Volume { get; set; }
        public double FreqMul;
        public double[] Phase;
    }

    public class PunkHammond : WaveProvider16
    {
        private static readonly double halfnotemultiplier = Math.Pow(2, ((double)1 / (double)12));
        private static readonly int maxPolyPhony = 10;
        private static readonly int echobuffersize = 20000;

        public DrawBar[] Drawbars { get; set; }

        public int CurrentPolyphony { get; set; }

        public int Leslie_Rate { get; set; }
        public int Leslie_Freq { get; set; }

        public int Echo_Rate { get; set; }
        public int Echo_Freq { get; set; }

        private short[] echobuffer;

        private double[] Notes;

        public double Freq { get { return Notes[0]; } set { Notes[0] = value; Notes[1] = value * Math.Pow(halfnotemultiplier, 4); Notes[1] = value * Math.Pow(halfnotemultiplier, 7); } }

        public int OverDrive { get; set; }

        public int DesiredLatency { get; set; }

        private Thread playthread;

        public PunkHammond()
        {
            Drawbars = new DrawBar[10];
            Notes = new double[maxPolyPhony];

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

            OverDrive = 0;

            DesiredLatency = 100;

            Freq = 0;

            playthread = new Thread(new ThreadStart(SynthThread));
            playthread.Start();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            playthread.Abort();
        }

        private class PressedKey
        {
            public Key key;
            public int channel;
        }

        private List<PressedKey> pressedKeys = new List<PressedKey>();

        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (pressedKeys.Find(x => x.key == e.Key) != null) return;

            int channel = -1;
            while (pressedKeys.Count >= CurrentPolyphony)
            {
                channel = pressedKeys[0].channel;
                Notes[pressedKeys[0].channel] = 0;
                pressedKeys.RemoveAt(0);
            }
            if (channel == -1)
            {
                for (channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Notes[channel] == 0) { break; }
                }
            }

            pressedKeys.Add(new PressedKey() { key = e.Key, channel = channel });
            Notes[channel] = Key2Freq(e.Key);

            e.Handled = true;
        }

        public void Window_KeyUp(object sender, KeyEventArgs e)
        {
            PressedKey pk = pressedKeys.Find(x => x.key == e.Key);
            if (pk != null)
            {
                Notes[pk.channel] = 0;
                pressedKeys.Remove(pk);
            }
            e.Handled = true;
        }

        private double Key2Freq(Key key)
        {
            double basefreq = 440;
            switch (key)
            {
                case Key.Y: return basefreq / Math.Pow(halfnotemultiplier, 9);
                case Key.S: return basefreq / Math.Pow(halfnotemultiplier, 8);
                case Key.X: return basefreq / Math.Pow(halfnotemultiplier, 7);
                case Key.D: return basefreq / Math.Pow(halfnotemultiplier, 6);
                case Key.C: return basefreq / Math.Pow(halfnotemultiplier, 5);
                case Key.V: return basefreq / Math.Pow(halfnotemultiplier, 4);
                case Key.G: return basefreq / Math.Pow(halfnotemultiplier, 3);
                case Key.B: return basefreq / Math.Pow(halfnotemultiplier, 2);
                case Key.H: return basefreq / Math.Pow(halfnotemultiplier, 1);
                case Key.N: return basefreq;
                case Key.J: return basefreq * Math.Pow(halfnotemultiplier, 1);
                case Key.M: return basefreq * Math.Pow(halfnotemultiplier, 2);

                case Key.Q: return basefreq * 2 / Math.Pow(halfnotemultiplier, 9);
                case Key.D2: return basefreq * 2 / Math.Pow(halfnotemultiplier, 8);
                case Key.W: return basefreq * 2 / Math.Pow(halfnotemultiplier, 7);
                case Key.D3: return basefreq * 2 / Math.Pow(halfnotemultiplier, 6);
                case Key.E: return basefreq * 2 / Math.Pow(halfnotemultiplier, 5);
                case Key.R: return basefreq * 2 / Math.Pow(halfnotemultiplier, 4);
                case Key.D5: return basefreq * 2 / Math.Pow(halfnotemultiplier, 3);
                case Key.T: return basefreq * 2 / Math.Pow(halfnotemultiplier, 2);
                case Key.D6: return basefreq * 2 / Math.Pow(halfnotemultiplier, 1);
                case Key.Z: return basefreq * 2;
                case Key.D7: return basefreq * 2 * Math.Pow(halfnotemultiplier, 1);
                case Key.U: return basefreq * 2 * Math.Pow(halfnotemultiplier, 2);
                case Key.I: return basefreq * 2 * Math.Pow(halfnotemultiplier, 3);
                case Key.D9: return basefreq * 2 * Math.Pow(halfnotemultiplier, 4);
                case Key.O: return basefreq * 2 * Math.Pow(halfnotemultiplier, 5);
                case Key.D0: return basefreq * 2 * Math.Pow(halfnotemultiplier, 6);
                case Key.P: return basefreq * 2 * Math.Pow(halfnotemultiplier, 7);

                default: return 0;
            }
        }


        private void SynthThread()
        {
            WaveOutEvent _waveOutEvent = new WaveOutEvent();
            _waveOutEvent.DeviceNumber = -1;
            _waveOutEvent.DesiredLatency = DesiredLatency;
            _waveOutEvent.NumberOfBuffers = 2;
            _waveOutEvent.Init(this);
            _waveOutEvent.Play();

            while (true)
            {
                if (_waveOutEvent.DesiredLatency != DesiredLatency)
                {
                    _waveOutEvent.Stop();
                    while (_waveOutEvent.PlaybackState != PlaybackState.Stopped) Thread.Sleep(10);
                    _waveOutEvent.DesiredLatency = DesiredLatency;
                    _waveOutEvent.Init(this);
                    _waveOutEvent.Play();
                }
                Thread.Sleep(100);
            }
        }


        double Leslie_Phase = 0;
        int echophase = 0;

        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            for (int sample = 0; sample < sampleCount; sample++)
            {
                Leslie_Phase += 2 * Math.PI / WaveFormat.SampleRate * Leslie_Freq;
                if (Leslie_Phase > 2 * Math.PI) Leslie_Phase -= 2 * Math.PI;
                double leslie = Math.Sin(Leslie_Phase) * Leslie_Rate;

                double currentsamplevalue = 0;

                for (int channel = 0; channel < CurrentPolyphony; channel++)
                {
                    if (Notes[channel] > 0)
                    {
                        double commonsinpart = 2 * Math.PI / WaveFormat.SampleRate * (Notes[channel] + leslie);
                        for (int drawbar = 1; drawbar < 10; drawbar++)
                        {
                            Drawbars[drawbar].Phase[channel] += commonsinpart * Drawbars[drawbar].FreqMul;
                            if (Drawbars[drawbar].Phase[channel] > 2 * Math.PI) Drawbars[drawbar].Phase[channel] -= 2 * Math.PI;
                            currentsamplevalue += (Math.Sin(Drawbars[drawbar].Phase[channel]) * Drawbars[drawbar].Volume);
                        }
                    }
                }
                //The extra 10 divider (minus overdrive) is for the drawbars. They have 10 position here.
                currentsamplevalue = currentsamplevalue / 9 * short.MaxValue / (10 - OverDrive) / CurrentPolyphony;

                currentsamplevalue = currentsamplevalue + echobuffer[limitechophase(echophase + Echo_Freq)] * Echo_Rate / 100;

                if (currentsamplevalue > short.MaxValue) currentsamplevalue = currentsamplevalue - short.MaxValue;

                buffer[sample + offset] = (short)currentsamplevalue;
                echobuffer[echophase]= (short)currentsamplevalue;
                echophase++;
                echophase = limitechophase(echophase);
            }
            return sampleCount;
        }

        private int limitechophase(int phase)
        {
            return phase < echobuffersize ? phase : phase - echobuffersize;
        }
    }
}
