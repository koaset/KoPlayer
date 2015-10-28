using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.Streams;

namespace KoPlayer.Forms
{
    public class EqualizerSettings
    {
        public event EventHandler ValueChanged;

        public const int MaxAmplitude = 12;

        private float[] values = new float[10];
        public bool SurpressValueChangedEvent { get; set; }

        public int Length
        {
            get 
            { 
                return values.Length;
            }
        }

        public float this[int index]
        {
            get 
            {
                return values[index];
            }
            set
            {
                float previous = values[index];

                if (value > MaxAmplitude)
                    values[index] = MaxAmplitude;
                else if (value < -MaxAmplitude)
                    values[index] = -MaxAmplitude;
                else
                    values[index] = value;

                if (!SurpressValueChangedEvent)
                    if (ValueChanged != null)
                        if (previous != values[index])
                            ValueChanged(this, new EventArgs());
            }
        }

        public void SetEqualizer(Equalizer eq)
        {
            if (eq != null)
                for (int i = 0; i < this.Length; i++)
                    eq.SampleFilters[i].SetGain(this[i]);
        }
    }
}
