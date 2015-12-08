using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib.Filters;

namespace KoPlayer.Forms
{
    public partial class FilterPopup : Form
    {
        public Filter Result { get; set; }
        private Filter old;

        /// <summary>
        /// Dialog for creating a new filter
        /// </summary>
        public FilterPopup()
        {
            InitializeComponent();
            InitFilterCombobox();
            SetParamsCombobox();
        }

        /// <summary>
        /// Dialog for editing existing filter
        /// </summary>
        /// <param name="filter"></param>
        public FilterPopup(Filter filter)
            : this()
        {
            this.Text = "Edit filter";
            ok_button.Text = "Save";
            filterParams_combobox.Items.Clear();

            switch (filter.GetType().ToString())
            {
                case "KoPlayer.Lib.Filters.StringFilter":
                    EditStringFilter(filter as StringFilter);
                    break;
                case "KoPlayer.Lib.Filters.RatingFilter":
                    EditRatingFilter(filter as RatingFilter);
                    break;
                case "KoPlayer.Lib.Filters.DateFilter":
                    EditDateFilter(filter as DateFilter);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public void EditStringFilter(StringFilter filter)
        {
            old = new StringFilter(filter);
            SetStringFilterParams();

            if (filter.Field == "title")
                filterType_combobox.SelectedIndex = 0;
            else if (filter.Field == "artist")
                filterType_combobox.SelectedIndex = 1;
            else if (filter.Field == "album")
                filterType_combobox.SelectedIndex = 2;
            else if (filter.Field == "genre")
                filterType_combobox.SelectedIndex = 3;

            if (filter.Contains)
                filterParams_combobox.SelectedIndex = 0;
            else
                filterParams_combobox.SelectedIndex = 1;

            searchString_textbox.Text = filter.SearchTerm;
        }

        public void EditRatingFilter(RatingFilter filter)
        {
            filterType_combobox.SelectedIndex = 4;
            old = new RatingFilter(filter);
            SetRatingFilterParams();

            if (filter.AndAbove && filter.Inclusive)
                filterParams_combobox.SelectedIndex = 0;
            else if (!filter.AndAbove && filter.Inclusive)
                filterParams_combobox.SelectedIndex = 1;
            else if (filter.AndAbove && !filter.Inclusive)
                filterParams_combobox.SelectedIndex = 2;
            else if (!filter.AndAbove && !filter.Inclusive)
                filterParams_combobox.SelectedIndex = 3;

            ratingBox1.Value = filter.EdgeRating;
        }

        public void EditDateFilter(DateFilter filter)
        {
            filterType_combobox.SelectedIndex = 5;
            old = new DateFilter(filter);
            SetDateFilterParams();
            filterParams_combobox.SelectedIndex = (int)filter.Unit;
            date_box.Value = filter.NumUnits;
        }

        public void InitFilterCombobox()
        {
            filterType_combobox.Items.Add("Title");
            filterType_combobox.Items.Add("Artist");
            filterType_combobox.Items.Add("Album");
            filterType_combobox.Items.Add("Genre");
            filterType_combobox.Items.Add("Rating");
            filterType_combobox.Items.Add("Date Added");
            filterType_combobox.SelectedIndex = 0;
        }

        public void SetParamsCombobox()
        {
            filterParams_combobox.Items.Clear();
            searchString_textbox.Visible = false;
            ratingBox1.Visible = false;
            date_box.Visible = false;

            switch (filterType_combobox.SelectedItem.ToString())
            {
                case "Title":
                case "Artist":
                case "Album":
                case "Genre":
                    SetStringFilterParams();
                    break;
                case "Rating":
                    SetRatingFilterParams();
                    break;
                case "Date Added":
                    SetDateFilterParams();
                    break;
            }

            filterParams_combobox.SelectedIndex = 0;
        }

        public void SetStringFilterParams()
        {
            searchString_textbox.Visible = true;
            searchString_textbox.Focus();
            searchString_textbox.Text = "";
            filterParams_combobox.Items.Add("contains");
            filterParams_combobox.Items.Add("does not contain");
        }

        public void SetRatingFilterParams()
        {
            ratingBox1.Visible = true;
            ratingBox1.Focus();
            ratingBox1.Value = 0;
            filterParams_combobox.Items.Add("is above (inclusive)");
            filterParams_combobox.Items.Add("is below (inclusive)");
            filterParams_combobox.Items.Add("is above (strict)");
            filterParams_combobox.Items.Add("is below (strict)");
        }

        public void SetDateFilterParams()
        {
            date_box.Visible = true;
            date_box.Focus();
            date_box.Value = 1;
            filterParams_combobox.Items.Add("is in the last (days)");
            filterParams_combobox.Items.Add("is in the last (weeks)");
            filterParams_combobox.Items.Add("is in the last (months)");
        }

        public void SetStartPosition()
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (int)(Cursor.Position.X - 50 ),
                (int)(Cursor.Position.Y - 50));
        }

        public enum FilterType
        {
            Title,
            Artist,
            Album,
            Genre,
            Rating,
            DateAdded,
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            Result = CreateFilter();

            DialogResult = DialogResult.OK;
        }

        private Filter CreateFilter()
        {
            switch (filterType_combobox.SelectedItem.ToString())
            {
                case "Title":
                case "Artist":
                case "Album":
                case "Genre":
                    return CreateStringFilter();
                case "Rating":
                    return CreateRatingFilter();
                case "Date Added":
                    return CreateDateFilter();
            }
            return null;
        }

        private StringFilter CreateStringFilter()
        {
            bool contains = filterParams_combobox.SelectedIndex == 0;
            return new StringFilter(filterType_combobox.SelectedItem.ToString().ToLower(),
                searchString_textbox.Text.ToLower(), contains);
        }

        private RatingFilter CreateRatingFilter()
        {
            int i = filterParams_combobox.SelectedIndex;
            bool andAbove = i == 0 || i == 2;
            bool inclusive = i <= 1;

            return new RatingFilter((int)ratingBox1.Value, andAbove, inclusive);
        }

        private DateFilter CreateDateFilter()
        {
            return new DateFilter((TimeUnit)filterParams_combobox.SelectedIndex,
                (int)date_box.Value);
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void filterType_combobox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetParamsCombobox();
        }
    }
}
