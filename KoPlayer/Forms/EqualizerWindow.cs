using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSCore.Streams;
using CSCore.Streams.Effects;

namespace KoPlayer.Forms
{
    public partial class EqualizerWindow : Form
    {
        private const int sliderMax = 500;
        private const int sliderMin = 0;

        private EqualizerSettings equalizerSettings;
        private List<TrackBar> sliderList;

        public EqualizerWindow(EqualizerSettings equalizerSettings)
        {
            this.equalizerSettings = equalizerSettings;
            InitializeComponent();

            #region Create slider list
            sliderList = new List<TrackBar>();
            sliderList.Add(trackBar1);
            sliderList.Add(trackBar2);
            sliderList.Add(trackBar3);
            sliderList.Add(trackBar4);
            sliderList.Add(trackBar5);
            sliderList.Add(trackBar6);
            sliderList.Add(trackBar7);
            sliderList.Add(trackBar8);
            sliderList.Add(trackBar9);
            sliderList.Add(trackBar10);
            #endregion

            foreach (TrackBar tb in sliderList)
            {
                tb.Maximum = sliderMax;
                tb.Minimum = sliderMin;
                int index = Int32.Parse((string)tb.Tag);
                tb.Value = EqualizerToSlider(equalizerSettings[index]);
            }

            label1.Focus();
        }

        private float SliderToEqualizer(int sliderValue)
        {
            return (((float)sliderValue - (float)sliderMin) /
                ((float)sliderMax - (float)sliderMin) * 2 - 1)
                * EqualizerSettings.MaxAmplitude; ;
        }

        private int EqualizerToSlider(float equalizerValue)
        {
            return (int)Math.Round((double)(sliderMin + 0.5 * (sliderMax - sliderMin) *
                (equalizerValue / EqualizerSettings.MaxAmplitude + 1)));
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar tb = sender as TrackBar;
            if (tb != null)
            {
                int index = Int32.Parse((string)tb.Tag);
                equalizerSettings[index] = SliderToEqualizer(tb.Value);
            }
        }
    }
}
