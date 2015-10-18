using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoPlayer.Forms
{

    public partial class DataGridViewPlus : DataGridView
    {
        #region Attributes & Properties

        private bool _enableDragDrop = false;
        private bool _suppressDragSelection = false;
        private int _clickedRow = -1;
        private Rectangle _dragBounds = Rectangle.Empty;
        private MouseEventArgs _mouseDownArgs = null;

        [Category("Behavior"), DefaultValue(false),
         Description("Enable or disable drag/drop functionality.")]
        public bool EnableDragDrop
        {
            get { return _enableDragDrop; }
            set { _enableDragDrop = value; }
        }

        [Category("Behavior"), DefaultValue(false),
         Description("Enable or disable drag selection of rows.  Disabling this will make dragging of unselected rows smoother.")]
        public bool SuppressDragSelection
        {
            get { return _suppressDragSelection; }
            set { _suppressDragSelection = value; }
        }

        [Category("Action"), Description("Occurs when the user begins dragging a row.")]
        public event ItemDragEventHandler RowDrag;

        #endregion

        #region Constructors

        public DataGridViewPlus() :base()
        {
            VerticalScrollBar.VisibleChanged += VerticalScrollBar_VisibleChanged;
            InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_enableDragDrop)
            {
                int row = this.HitTest(e.X, e.Y).RowIndex;

                // We only drag selected rows so the user can drag multiple rows.  If we call
                // base.OnMouseDown(e), the click will deselect all rows apart from the current row.
                if (
                    row >= 0 &&
                    row < this.Rows.Count &&
                    (_suppressDragSelection || this.SelectedRows.Contains(this.Rows[row]))
                  )
                {
                    _clickedRow = row;
                    Size dragSize = SystemInformation.DragSize;

                    _dragBounds = new Rectangle(new Point(
                      e.X - (dragSize.Width / 2),
                      e.Y - (dragSize.Height / 2)), dragSize);

                    _mouseDownArgs = e; // Record for future use if they abort drag/drop and are just clicking.

                    // If we are not suppressing it means this row has been selected, so we exit before base.OnMouseDown.
                    if (!_suppressDragSelection) return;
                }
                else
                {
                    _dragBounds = Rectangle.Empty;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_enableDragDrop)
            {
                int row = this.HitTest(e.X, e.Y).RowIndex;

                if (row < 0) // Ignore column headers.
                {
                    _dragBounds = Rectangle.Empty;
                    return;
                }

                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    // If the mouse has been click-dragged outside our grid.
                    if (_dragBounds != Rectangle.Empty && !_dragBounds.Contains(e.X, e.Y))
                    {
                        ItemDragEventArgs dragArgs = new ItemDragEventArgs(MouseButtons.Left, this.SelectedRows);
                        OnRowDrag(dragArgs);
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // The drag drop operation was not concluded.
            if (_dragBounds.Contains(e.X, e.Y))
            {
                if (_clickedRow >= 0 && _clickedRow < this.Rows.Count)
                {
                    // Let the grid continue processing the click we hijacked earlier...
                    // Hopefully it will be on the same row!  Tolerance is good enough.
                    base.OnMouseDown(_mouseDownArgs);
                    _mouseDownArgs = null;
                }
            }

            base.OnMouseUp(e);
        }

        protected virtual void OnRowDrag(ItemDragEventArgs e)
        {
            if (RowDrag != null)
                RowDrag(this, e);
        }

        void VerticalScrollBar_VisibleChanged(object sender, EventArgs e)
        {
            VerticalScrollBar.Visible = true;
        }

        #endregion
    }
}