﻿using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Xml.Serialization;
using Timer = System.Timers.Timer;

namespace Z_nthCommon
{
    public abstract class Zplusnthbase : WaveProvider16, INotifyPropertyChanged
    {
        protected static readonly double halfnotemultiplier = Math.Pow(2, ((double)1 / (double)12));
        protected static readonly int maxPolyPhony = 10;
        public Channel[] Channels { get; set; }
        private Thread playthread;

        public enum ChannelState
        {
            Inactive,
            KeyOn,
            ReKeyOn,
            Active,
            KeyOff
        }

        public class Channel : INotifyPropertyChanged
        {
            private double _Freq = 0;
            public double Freq { get { return _Freq; } set { _Freq = value; NotifyPropertyChanged("Freq4Disp"); } }
            private ChannelState _State = ChannelState.Inactive;
            public ChannelState State { get { return _State; } set { _State = value; NotifyPropertyChanged("Freq4Disp"); } }

            public string Freq4Disp { get { return State == ChannelState.Inactive ? "" : Math.Round(Freq).ToString(); } }

            protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class BaseState
        {
            public int DesiredLatency { get; set; } = 50;
        }

        public Zplusnthbase()
        {
            Channels = new Channel[maxPolyPhony];
            for (int i = 0; i < maxPolyPhony; i++) { Channels[i] = new Channel(); }
            playthread = new Thread(new ThreadStart(SynthThread));
            playthread.IsBackground = true;
            playthread.Start();
            midithread = new Thread(new ThreadStart(MidiThread));
            midithread.IsBackground = true;
            DesiredLatency = 50;
            CurrentPolyphony = 1;
            Transpose = 0;

            Bending = 0;

            MidiNumberToFreq = new Dictionary<int, double>();
            for (int i = 1; i < 120; i++)
            {
                MidiNumberToFreq[i] = Math.Pow(2, ((double)i - 69) / 12) * 440;
            }
        }

        protected short OutLimiter(ref double currentsamplevalue)
        {
            currentsamplevalue = currentsamplevalue * short.MaxValue;
            if (currentsamplevalue > short.MaxValue) currentsamplevalue = short.MaxValue;
            if (currentsamplevalue < short.MinValue) currentsamplevalue = short.MinValue;
            return (short)currentsamplevalue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            playthread.Abort();
            if (midithread.ThreadState == ThreadState.Suspended) midithread.Resume();
            if (midithread.ThreadState == ThreadState.Running) midithread.Abort();
            SavePresets();
            SaveState();
        }

        private void SaveState()
        {
            BaseState state = new BaseState() { DesiredLatency = DesiredLatency };

            XmlSerializer serializer = new XmlSerializer(typeof(BaseState));
            TextWriter writer = new StreamWriter(typeof(BaseState).Name + ".xml");
            serializer.Serialize(writer, state);
            writer.Close();
        }

        public void LoadState()
        {
            if (File.Exists(typeof(BaseState).Name + ".xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BaseState));
                FileStream fs = new FileStream(typeof(BaseState).Name + ".xml", FileMode.Open);
                BaseState state;
                state = (BaseState)serializer.Deserialize(fs);
                DesiredLatency = state.DesiredLatency;
            }
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
                case Key.Insert:
                    StartDemo();
                    break;
                case Key.Delete:
                    StopDemo();
                    break;
                default:
                    break;
            }

            if (pressedKeys.Find(x => x.key == e.Key) != null) return;

            int channel = GetChannel();

            var freq = Key2Freq(e.Key);
            if (freq != 0)
            {
                pressedKeys.Add(new PressedKey() { key = e.Key, channel = channel, midiNoteNumber = 0 });
                Channels[channel].Freq = freq;
                if (Channels[channel].State == ChannelState.Inactive) Channels[channel].State = ChannelState.KeyOn;
                else Channels[channel].State = ChannelState.ReKeyOn;
            }
            e.Handled = true;
        }

        private int GetChannel()
        {
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
            return channel;
        }

        MidiFile mifi;

        private void StopDemo()
        {
            if (midithread.ThreadState == ThreadState.Stopped) return;
            midithread.Suspend();
            foreach (var item in Channels)
            {
                item.State = ChannelState.Inactive;
            }
        }

        private void StartDemo()
        {
            if (midithread.ThreadState == ThreadState.Running) return;
            if (midithread.ThreadState == ThreadState.Stopped) midithread = new Thread(new ThreadStart(MidiThread));

            if (!File.Exists("demo.mid")) return;
            mifi = new MidiFile("demo.mid", false);

            MidiEventsByTick = mifi.Events.SelectMany(x => x).OrderBy(x => x.AbsoluteTime).
                 GroupBy(x => x.AbsoluteTime, x => x).ToDictionary(x => x.Key, x => x.ToArray());

            MaxMidiTicks = MidiEventsByTick.Keys.Max();

            var timeSignature = mifi.Events[0].OfType<TimeSignatureEvent>().FirstOrDefault();

            int beatsPerBar = timeSignature == null ? 4 : timeSignature.Numerator;
            int ticksPerBar = timeSignature == null ? mifi.DeltaTicksPerQuarterNote * 4 : (timeSignature.Numerator * mifi.DeltaTicksPerQuarterNote * 4) / (1 << timeSignature.Denominator);
            ticksPerBeat = ticksPerBar / beatsPerBar;

            midisleeptime = 1;
            MidiTicks = 0;
            if (midithread.ThreadState == ThreadState.Suspended) midithread.Resume();
            else midithread.Start();
        }

        private Thread midithread;
        private Dictionary<long, MidiEvent[]> MidiEventsByTick;
        private int MidiTicks;
        private long MaxMidiTicks;
        private int ticksPerBeat;
        int midisleeptime;

        private void MidiThread()
        {
            while (MidiTicks <= MaxMidiTicks)
            {
                Thread.Sleep(midisleeptime);

                MidiEvent[] me = null;
                MidiEventsByTick.TryGetValue(MidiTicks, out me);
                MidiTicks++;
                if (me == null) continue;

                foreach (MidiEvent item in me)
                {
                    if (item.Channel == 10) continue;
                    if (item is TempoEvent)
                    {
                        TempoEvent te = (TempoEvent)item;
                        int interv = te.MicrosecondsPerQuarterNote / ticksPerBeat / 1000;
                        midisleeptime = interv == 0 ? 1 : interv;
                    }
                    if (item is NoteOnEvent)
                    {
                        NoteOnEvent ne = (NoteOnEvent)item;
                        PressedKey pk = pressedKeys.Find(x => x.midiNoteNumber == ne.NoteNumber);

                        if (pk != null)
                        {
                            if (ne.Velocity == 0 || ne.CommandCode == MidiCommandCode.NoteOff) //Keyoff
                            {
                                Channels[pk.channel].State = ChannelState.KeyOff;
                                pressedKeys.Remove(pk);
                            }
                            continue;
                        }

                        int channel = GetChannel();

                        var freq = MidiNumberToFreq[ne.NoteNumber];
                        if (freq != 0)
                        {
                            pressedKeys.Add(new PressedKey() { key = Key.None, channel = channel, midiNoteNumber = ne.NoteNumber });
                            Channels[channel].Freq = freq;
                            if (Channels[channel].State == ChannelState.Inactive) Channels[channel].State = ChannelState.KeyOn;
                            else Channels[channel].State = ChannelState.ReKeyOn;
                        }
                    }
                }
            }
            return;
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
            public int midiNoteNumber;
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
                case Key.Oem102: return basefreq / Math.Pow(halfnotemultiplier, 16);
                case Key.A: return basefreq / Math.Pow(halfnotemultiplier, 15);
                case Key.Y: return basefreq / Math.Pow(halfnotemultiplier, 14);
                case Key.S: return basefreq / Math.Pow(halfnotemultiplier, 13);
                case Key.X: return basefreq / Math.Pow(halfnotemultiplier, 12);
                case Key.D: return basefreq / Math.Pow(halfnotemultiplier, 11);
                case Key.C: return basefreq / Math.Pow(halfnotemultiplier, 10);
                case Key.V: return basefreq / Math.Pow(halfnotemultiplier, 9);
                case Key.G: return basefreq / Math.Pow(halfnotemultiplier, 8);
                case Key.B: return basefreq / Math.Pow(halfnotemultiplier, 7);
                case Key.H: return basefreq / Math.Pow(halfnotemultiplier, 6);
                case Key.N: return basefreq / Math.Pow(halfnotemultiplier, 5);
                case Key.M: return basefreq / Math.Pow(halfnotemultiplier, 4);
                case Key.K: return basefreq / Math.Pow(halfnotemultiplier, 3);
                case Key.OemComma: return basefreq / Math.Pow(halfnotemultiplier, 2);
                case Key.L: return basefreq / Math.Pow(halfnotemultiplier, 1);
                case Key.OemPeriod: return basefreq;
                case Key.Oem1: return basefreq * Math.Pow(halfnotemultiplier, 1);
                case Key.OemMinus: return basefreq * Math.Pow(halfnotemultiplier, 2);

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
                case Key.Oem3: return basefreq * 2 * Math.Pow(halfnotemultiplier, 6);
                case Key.P: return basefreq * 2 * Math.Pow(halfnotemultiplier, 7);
                case Key.Oem4: return basefreq * 2 * Math.Pow(halfnotemultiplier, 8);
                case Key.OemPlus: return basefreq * 2 * Math.Pow(halfnotemultiplier, 9);
                case Key.Oem6: return basefreq * 2 * Math.Pow(halfnotemultiplier, 10);

                default: return 0;
            }
        }

        Dictionary<int, double> MidiNumberToFreq;

        private int _DesiredLatency;
        public int DesiredLatency { get { return _DesiredLatency; } set { _DesiredLatency = value; NotifyPropertyChanged(); } }


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
                Thread.Sleep(1);
            }
        }
    }
}
