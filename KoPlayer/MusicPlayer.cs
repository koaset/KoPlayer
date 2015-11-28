using System;
using System.ComponentModel;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;

namespace KoPlayer
{
    public class MusicPlayer : IDisposable
    {
        private ISoundOut soundOut;
        private IWaveSource waveSource;
        private Equalizer equalizer;

        public event EventHandler OpenCompleted;

        public event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped
        {
            add
            {
                if (soundOut != null)
                    soundOut.Stopped += value;
            }
            remove
            {
                if (soundOut != null)
                    soundOut.Stopped -= value;
            }
        }

        public Equalizer Equalizer
        {
            get
            {
                return equalizer;
            }
        }

        public PlaybackState PlaybackState
        {
            get
            {
                if (soundOut != null)
                    return soundOut.PlaybackState;
                return PlaybackState.Stopped;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (waveSource != null)
                    return waveSource.GetPosition();
                return TimeSpan.Zero;
            }
            set
            {
                if (waveSource != null)
                    waveSource.SetPosition(value);
            }
        }

        public TimeSpan Length
        {
            get
            {
                if (waveSource != null)
                    return waveSource.GetLength();
                return TimeSpan.Zero;
            }
        }

        public int Volume
        {
            get
            {
                if (soundOut != null)
                    return Math.Min(100, Math.Max((int)(soundOut.Volume * 100), 0));
                return 1;
            }
            set
            {
                if (soundOut != null)
                {
                    soundOut.Volume = Math.Min(1.0f, Math.Max(value / 100f, 0f));
                }
            }
        }

        public void Open(string filename, MMDevice device)
        {
            CleanupPlayback();

            var source = CodecFactory.Instance.GetCodec(filename);

            waveSource =
                CodecFactory.Instance.GetCodec(filename)
                    .ToStereo()
                    .ChangeSampleRate(44100)
                    .ToSampleSource()
                    .AppendSource(Equalizer.Create10BandEqualizer, out equalizer)
                    .ToWaveSource(16);

            soundOut = new WasapiOut() {Latency = 100, Device = device};
            soundOut.Initialize(waveSource);
            if (this.OpenCompleted != null)
                this.OpenCompleted(this, new EventArgs());
        }

        public void Play()
        {
            if (soundOut != null)
                soundOut.Play();
        }

        public void Pause()
        {
            if (soundOut != null)
                soundOut.Pause();
        }

        public void Stop()
        {
            if (soundOut != null)
                soundOut.Stop();
        }

        private void CleanupPlayback()
        {
            if (soundOut != null)
            {
                soundOut.Dispose();
                soundOut = null;
            }
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }
        }

        public void Dispose()
        {
            CleanupPlayback();
            if (equalizer != null)
                equalizer.Dispose();
        }
    }
}