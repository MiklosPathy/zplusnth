using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Z_nthCommon
{
    public abstract class Zplusnthbase : WaveProvider16
    {
        protected static readonly double halfnotemultiplier = Math.Pow(2, ((double)1 / (double)12));
        protected static readonly int maxPolyPhony = 10;
        protected double[] Notes;
        private Thread playthread;

        public Zplusnthbase()
        {
            Notes = new double[maxPolyPhony];
            playthread = new Thread(new ThreadStart(SynthThread));
            playthread.Start();
            DesiredLatency = 100;

            Bending = 0;
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            playthread.Abort();
        }

        public int CurrentPolyphony { get; set; }

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
            //for (int drawbar = 1; drawbar < 10; drawbar++) { Drawbars[drawbar].Phase[channel] = Math.PI; }

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

        private class PressedKey
        {
            public Key key;
            public int channel;
        }

        private List<PressedKey> pressedKeys = new List<PressedKey>();

        public double Bending { get; set; }

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

        public int DesiredLatency { get; set; }

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
    }
}
