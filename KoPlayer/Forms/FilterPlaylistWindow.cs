using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;
using KoPlayer.Lib.Filters;

namespace KoPlayer.Forms
{
    public partial class FilterPlaylistWindow : Form
    {
        private List<Filter> filters;

        /// <summary>
        /// This is a window meant for creating a new filtered playlist
        /// </summary>
        public FilterPlaylistWindow()
        {
            InitializeComponent();
            filters = new List<Filter>();
        }

        /// <summary>
        /// This is a window meant for editing an existing filtered playlist
        /// </summary>
        /// <param name="pl"></param>
        public FilterPlaylistWindow(FilterPlaylist pl)
        {
            InitializeComponent();

            this.Text = "Edit rating filter playlist";

            filter_listbox.Items.AddRange(pl.Filters.ToArray());

            this.create_button.Text = "Save";
        }

        private void create_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            filters = filter_listbox.Items.Cast<Filter>().ToList();

            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        public List<Filter> GetFilterList()
        {
            return this.filters;
        }

        public void SetStartPosition()
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (int)(Cursor.Position.X - 0.5 * this.Size.Width),
                (int)(Cursor.Position.Y - 0.5 * this.Size.Width));
        }

        private void addFilter_button_Click(object sender, EventArgs e)
        {
            var popup = new FilterPopup();
            popup.SetStartPosition();

            if (popup.ShowDialog() == DialogResult.OK)
            {
                Filter result = popup.Result;
                filter_listbox.Items.Add(result);
            }
        }

        private void editFilter_button_Click(object sender, EventArgs e)
        {
            if (filter_listbox.SelectedIndex == -1)
                return;

            int i = filter_listbox.SelectedIndex;
            var filterToEdit = (Filter)filter_listbox.SelectedItem;

            var popup = new FilterPopup(filterToEdit);
            popup.SetStartPosition();

            if (popup.ShowDialog() == DialogResult.OK)
            {
                Filter result = popup.Result;
                filter_listbox.Items.RemoveAt(i);
                filter_listbox.Items.Insert(i, result);
            }
        }

        private void removeFilter_button_Click(object sender, EventArgs e)
        {
            if (filter_listbox.SelectedIndex != -1)
                filter_listbox.Items.RemoveAt(filter_listbox.SelectedIndex);
        }
    }
}
