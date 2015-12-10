using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoPlayer.Forms.Controls
{
    class KoPlayerButton : Button
    {
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            ImageIndex = 1;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            ImageIndex = 0;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                ImageIndex = 2;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            ImageIndex = 1;
        }
    }
}
