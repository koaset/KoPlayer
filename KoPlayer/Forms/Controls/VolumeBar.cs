using System;
using System.Windows.Forms;

namespace KoPlayer.Forms.Controls
{
    class VolumeBar : TrackBar
    {
        public MusicPlayer Player { get; set; }
        public DataGridViewPlus SongGrid { get; set; }

        public VolumeBar()
        {

        }

        protected override void OnValueChanged(EventArgs e)
        {
            if (Player != null)
                Player.Volume = Value;
            base.OnValueChanged(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            SongGrid?.Focus();
            var ee = (HandledMouseEventArgs)e;
            ee.Handled = true;
            base.OnMouseWheel(e);
        }

        public void SetValue(int value)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate { SetValue(value); }));
            else
            {
                if (value > Maximum)
                    Value = Maximum;
                else if (value < Minimum)
                    Value = Minimum;
                else
                    Value = value;
            }
        }
    }
}
