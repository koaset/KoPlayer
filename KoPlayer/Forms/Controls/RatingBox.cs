using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms.Controls
{
    class RatingBox : NumericUpDown
    {
        public RatingBox()
        {
            Value = 0;
            Minimum = 0;
            Maximum = 5;
        }

        protected override void UpdateEditText()
        {
            Text = Song.RatingIntToString((int)Value);
        }
    }

}
