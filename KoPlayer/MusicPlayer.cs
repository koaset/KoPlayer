using System;
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
        private IWaveSource finalSource;
        private Equalizer equalizer;

        private VolumeSource volumeSource;
        private float deviceVolume = 1f;
        
        public event EventHandler OpenCompleted;

        public static MMDevice PlaybackDevice { get; private set; }
        public static MMDeviceCollection PlaybackDevices { get; private set; }

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
                if (finalSource != null)
                    return finalSource.GetPosition();
                return TimeSpan.Zero;
            }
            set
            {
                if (finalSource != null)
                    finalSource.SetPosition(value);
            }
        }

        public TimeSpan Length
        {
            get
            {
                if (finalSource != null)
                    return finalSource.GetLength();
                return TimeSpan.Zero;
            }
        }

        public int Volume
        {
            get
            {
                if (soundOut != null)
                    return Math.Min(100, Math.Max((int)(volumeSource.Volume * 1000), 0));
                return 1;
            }
            set
            {
                if (soundOut != null)
                {
                    volumeSource.Volume = Math.Min(1.0f, Math.Max(value / 1000f, 0f));
                }
            }
        }

        public float DeviceVolume
        {
            get
            {
                return deviceVolume;
            }
            set
            {
                if (deviceVolume > 0)
                    deviceVolume = Math.Min(1.0f, value);
            }
        }

        public MusicPlayer()
        {
            SetDevice();

            using (var client = AudioClient.FromMMDevice(PlaybackDevice))
            {
                client.Initialize(AudioClientShareMode.Shared,
                    AudioClientStreamFlags.None, 1000, 0, client.GetMixFormat(), Guid.Empty);
                
            }
        }

        public void SetDevice(string name = null)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                PlaybackDevices = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

                PlaybackDevice = PlaybackDevice ?? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                if (!string.IsNullOrEmpty(name))
                {
                    foreach (var device in PlaybackDevices)
                    {
                        if (device.FriendlyName == name)
                            PlaybackDevice = device;
                    }
                }
            }
        }

        public void Open(string filename)
        {
            CleanupPlayback();

            var source = CodecFactory.Instance.GetCodec(filename);

            volumeSource = new VolumeSource(source);

            equalizer = Equalizer.Create10BandEqualizer(volumeSource);

            finalSource = equalizer
                    .ToStereo()
                    .ChangeSampleRate(44100)
                    .AppendSource(Equalizer.Create10BandEqualizer, out equalizer)
                    .ToWaveSource(16);
            
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                soundOut = new WasapiOut() { Latency = 100, Device = PlaybackDevice }; 
            else
                soundOut = new DirectSoundOut();

            soundOut.Initialize(finalSource);

            soundOut.Volume = deviceVolume;

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
                deviceVolume = soundOut.Volume;
                soundOut.Dispose();
                soundOut = null;
            }
            if (finalSource != null)
            {
                finalSource.Dispose();
                finalSource = null;
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