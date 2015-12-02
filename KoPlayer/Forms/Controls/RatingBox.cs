using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms
{
    class RatingBox : NumericUpDown
    {
        public RatingBox()
        {

        }

        protected override void UpdateEditText()
        {
            this.Text = Song.RatingIntToString((int)this.Value);
        }
    }

}
