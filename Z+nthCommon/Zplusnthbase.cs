using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Z_nthCommon
{
    public abstract class Zplusnthbase : WaveProvider16, INotifyPropertyChanged
    {
        protected static readonly double halfnotemultiplier = Math.Pow(2, ((double)1 / (double)12));
        protected static readonly int maxPolyPhony = 10;
        protected Channel[] Channels;
        private Thread playthread;

        protected enum ChannelState
        {
            Inactive,
            KeyOn,
            ReKeyOn,
            Active,
            KeyOff
        }

        protected class Channel
        {
            public double Freq = 0;
            public ChannelState State = ChannelState.Inactive;
        }

        public Zplusnthbase()
        {
            Channels = new Channel[maxPolyPhony];
            for (int i = 0; i < maxPolyPhony; i++) { Channels[i] = new Channel(); }
            playthread = new Thread(new ThreadStart(SynthThread));
            playthread.Start();
            DesiredLatency = 50;
            CurrentPolyphony = 1;
            Transpose = 0;

            Bending = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            playthread.Abort();
            SavePresets();
        }

        abstract protected void SavePresets();

        private int currentpolyphony;
        public int CurrentPolyphony { get { return currentpolyphony; } set { currentpolyphony = value; NotifyPropertyChanged(); } }

        public void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                case Key.F2:
                case Key.F3:
                case Key.F4:
                case Key.F5:
                case Key.F6:
                case Key.F7:
                case Key.F8:
                case Key.F9:
                case Key.F10:
                case Key.F11:
                case Key.F12:
                case Key.F13:
                case Key.F14:
                case Key.F15:
                case Key.F16:
                case Key.F17:
                case Key.F18:
                case Key.F19:
                case Key.F20:
                case Key.F21:
                case Key.F22:
                case Key.F23:
                case Key.F24:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        GetPreset((int)e.Key - (int)Key.F1 + 1);
                    else
                        SetPreset((int)e.Key - (int)Key.F1 + 1);
                    return;
                default:
                    break;
            }

            if (pressedKeys.Find(x => x.key == e.Key) != null) return;

            int channel = -1;
            while (pressedKeys.Count >= CurrentPolyphony)
            {
                channel = pressedKeys[0].channel;
                pressedKeys.RemoveAt(0);
            }
            if (channel == -1)
            {
                for (int i = 0; i < CurrentPolyphony; i++)
                {
                    if (Channels[i].State == ChannelState.Inactive)
                    {
                        channel = i;
                        break;
                    }
                }
            }
            if (channel == -1)
            {
                for (int i = 0; i < CurrentPolyphony; i++)
                {
                    if (Channels[i].State == ChannelState.KeyOff)
                    {
                        channel = i;
                        break;
                    }
                }
            }
            if (channel == -1)
            {
                for (int i = 0; i < CurrentPolyphony; i++)
                {
                    if (Channels[i].State == ChannelState.Active)
                    {
                        channel = i;
                        break;
                    }
                }
            }
            if (channel == -1)
            {
                channel = CurrentPolyphony - 1;
            }

            var freq = Key2Freq(e.Key);
            if (freq != 0)
            {
                pressedKeys.Add(new PressedKey() { key = e.Key, channel = channel });
                Channels[channel].Freq = Key2Freq(e.Key);
                if (Channels[channel].State == ChannelState.Inactive) Channels[channel].State = ChannelState.KeyOn;
                else Channels[channel].State = ChannelState.ReKeyOn;
            }
            e.Handled = true;
        }

        protected abstract void GetPreset(int presetnum);
        protected abstract void SetPreset(int presetnum);

        public void Window_KeyUp(object sender, KeyEventArgs e)
        {
            PressedKey pk = pressedKeys.Find(x => x.key == e.Key);
            if (pk != null)
            {
                Channels[pk.channel].State = ChannelState.KeyOff;
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

        private int transpose;
        public int Transpose { get { return transpose; } set { transpose = value; NotifyPropertyChanged(); } }

        private double Key2Freq(Key key)
        {
            double basefreq = 440 * Math.Pow(2, transpose);
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
                Thread.Sleep(10);
            }
        }
    }
}
