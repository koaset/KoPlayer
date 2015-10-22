namespace KoPlayer.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setLibraryPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPlaylistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playPauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.albumArtBox = new System.Windows.Forms.PictureBox();
            this.volumeTrackBar = new System.Windows.Forms.TrackBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.playListGridView = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.songsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.previousButton = new System.Windows.Forms.Button();
            this.playpauseButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.searchBar = new System.Windows.Forms.TrackBar();
            this.songInfoLabel = new System.Windows.Forms.Label();
            this.currentTime_Label = new System.Windows.Forms.Label();
            this.songLength_Label = new System.Windows.Forms.Label();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.searchBox = new System.Windows.Forms.TextBox();
            this.songGridView = new KoPlayer.Forms.DataGridViewPlus();
            this.playListBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.albumArtBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.playListGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.songsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.songGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.playListBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.playbackToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1136, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.setLibraryPathToolStripMenuItem,
            this.newPlaylistToolStripMenuItem,
            this.preferencesToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // setLibraryPathToolStripMenuItem
            // 
            this.setLibraryPathToolStripMenuItem.Name = "setLibraryPathToolStripMenuItem";
            this.setLibraryPathToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.setLibraryPathToolStripMenuItem.Text = "Add folder to Library";
            this.setLibraryPathToolStripMenuItem.Click += new System.EventHandler(this.addFolderToLibraryToolStripMenuItem_Click);
            // 
            // newPlaylistToolStripMenuItem
            // 
            this.newPlaylistToolStripMenuItem.Name = "newPlaylistToolStripMenuItem";
            this.newPlaylistToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.newPlaylistToolStripMenuItem.Text = "New Playlist";
            this.newPlaylistToolStripMenuItem.Click += new System.EventHandler(this.newPlaylistToolStripMenuItem_Click);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.preferencesToolStripMenuItem.Text = "Settings";
            this.preferencesToolStripMenuItem.Click += new System.EventHandler(this.preferencesToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // playbackToolStripMenuItem
            // 
            this.playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playPauseToolStripMenuItem,
            this.playNextToolStripMenuItem,
            this.playPreviousToolStripMenuItem});
            this.playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
            this.playbackToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.playbackToolStripMenuItem.Text = "Playback";
            // 
            // playPauseToolStripMenuItem
            // 
            this.playPauseToolStripMenuItem.Name = "playPauseToolStripMenuItem";
            this.playPauseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.playPauseToolStripMenuItem.Text = "Play / Pause";
            this.playPauseToolStripMenuItem.Click += new System.EventHandler(this.playPauseToolStripMenuItem_Click);
            // 
            // playNextToolStripMenuItem
            // 
            this.playNextToolStripMenuItem.Name = "playNextToolStripMenuItem";
            this.playNextToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.playNextToolStripMenuItem.Text = "Play Next";
            this.playNextToolStripMenuItem.Click += new System.EventHandler(this.playNextToolStripMenuItem_Click);
            // 
            // playPreviousToolStripMenuItem
            // 
            this.playPreviousToolStripMenuItem.Name = "playPreviousToolStripMenuItem";
            this.playPreviousToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.playPreviousToolStripMenuItem.Text = "Play Previous";
            this.playPreviousToolStripMenuItem.Click += new System.EventHandler(this.playPreviousToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // albumArtBox
            // 
            this.albumArtBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.albumArtBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.albumArtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.albumArtBox.Location = new System.Drawing.Point(12, 446);
            this.albumArtBox.Name = "albumArtBox";
            this.albumArtBox.Size = new System.Drawing.Size(175, 175);
            this.albumArtBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.albumArtBox.TabIndex = 3;
            this.albumArtBox.TabStop = false;
            // 
            // volumeTrackBar
            // 
            this.volumeTrackBar.AutoSize = false;
            this.volumeTrackBar.Location = new System.Drawing.Point(193, 62);
            this.volumeTrackBar.Maximum = 50;
            this.volumeTrackBar.Name = "volumeTrackBar";
            this.volumeTrackBar.Size = new System.Drawing.Size(118, 27);
            this.volumeTrackBar.TabIndex = 5;
            this.volumeTrackBar.TickFrequency = 25;
            this.volumeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.volumeTrackBar.Value = 10;
            this.volumeTrackBar.ValueChanged += new System.EventHandler(this.volumeTrackBar_ValueChanged);
            this.volumeTrackBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.volumeTrackBar_MouseWheel);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 624);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1136, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // playListGridView
            // 
            this.playListGridView.AllowDrop = true;
            this.playListGridView.AllowUserToAddRows = false;
            this.playListGridView.AllowUserToDeleteRows = false;
            this.playListGridView.AllowUserToResizeColumns = false;
            this.playListGridView.AllowUserToResizeRows = false;
            this.playListGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.playListGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.playListGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.playListGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.playListGridView.ColumnHeadersVisible = false;
            this.playListGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            this.playListGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.playListGridView.GridColor = System.Drawing.Color.Black;
            this.playListGridView.Location = new System.Drawing.Point(12, 95);
            this.playListGridView.MultiSelect = false;
            this.playListGridView.Name = "playListGridView";
            this.playListGridView.RowHeadersVisible = false;
            this.playListGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.playListGridView.Size = new System.Drawing.Size(175, 345);
            this.playListGridView.TabIndex = 7;
            this.playListGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.playListGridView_CellClick);
            this.playListGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.playListGridView_CellEndEdit);
            this.playListGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.playListGridView_DragDrop);
            this.playListGridView.DragEnter += new System.Windows.Forms.DragEventHandler(this.playListGridView_DragEnter);
            this.playListGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.playListGridView_KeyDown);
            this.playListGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.playListGridView_MouseDown);
            // 
            // Column1
            // 
            this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(12, 54);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(48, 23);
            this.previousButton.TabIndex = 8;
            this.previousButton.Text = "prev";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // playpauseButton
            // 
            this.playpauseButton.Location = new System.Drawing.Point(66, 54);
            this.playpauseButton.Name = "playpauseButton";
            this.playpauseButton.Size = new System.Drawing.Size(48, 23);
            this.playpauseButton.TabIndex = 9;
            this.playpauseButton.Text = "play/pause";
            this.playpauseButton.UseVisualStyleBackColor = true;
            this.playpauseButton.Click += new System.EventHandler(this.playpauseButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(120, 54);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(48, 23);
            this.nextButton.TabIndex = 10;
            this.nextButton.Text = "next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // searchBar
            // 
            this.searchBar.AutoSize = false;
            this.searchBar.BackColor = System.Drawing.SystemColors.Control;
            this.searchBar.LargeChange = 0;
            this.searchBar.Location = new System.Drawing.Point(336, 74);
            this.searchBar.Maximum = 1000;
            this.searchBar.Name = "searchBar";
            this.searchBar.Size = new System.Drawing.Size(437, 15);
            this.searchBar.SmallChange = 0;
            this.searchBar.TabIndex = 12;
            this.searchBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.searchBar.ValueChanged += new System.EventHandler(this.searchBar_ValueChanged);
            this.searchBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.searchBar_MouseDown);
            this.searchBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.searchBar_MouseUp);
            this.searchBar.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.searchBar_MouseWheel);
            // 
            // songInfoLabel
            // 
            this.songInfoLabel.Location = new System.Drawing.Point(336, 24);
            this.songInfoLabel.Name = "songInfoLabel";
            this.songInfoLabel.Size = new System.Drawing.Size(437, 50);
            this.songInfoLabel.TabIndex = 13;
            this.songInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // currentTime_Label
            // 
            this.currentTime_Label.Location = new System.Drawing.Point(338, 58);
            this.currentTime_Label.Name = "currentTime_Label";
            this.currentTime_Label.Size = new System.Drawing.Size(46, 13);
            this.currentTime_Label.TabIndex = 15;
            this.currentTime_Label.Text = "0:00";
            this.currentTime_Label.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // songLength_Label
            // 
            this.songLength_Label.Location = new System.Drawing.Point(724, 58);
            this.songLength_Label.Name = "songLength_Label";
            this.songLength_Label.Size = new System.Drawing.Size(46, 13);
            this.songLength_Label.TabIndex = 16;
            this.songLength_Label.Text = "0:00";
            this.songLength_Label.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // trayIcon
            // 
            this.trayIcon.Text = "KoPlayer";
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.searchBox.Location = new System.Drawing.Point(962, 57);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(162, 20);
            this.searchBox.TabIndex = 17;
            this.searchBox.Text = "Search Library";
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.Enter += new System.EventHandler(this.searchBox_Enter);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            this.searchBox.Leave += new System.EventHandler(this.searchBox_Leave);
            // 
            // songGridView
            // 
            this.songGridView.AllowDrop = true;
            this.songGridView.AllowUserToAddRows = false;
            this.songGridView.AllowUserToDeleteRows = false;
            this.songGridView.AllowUserToOrderColumns = true;
            this.songGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.songGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.songGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.songGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.songGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.songGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.songGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.songGridView.EnableDragDrop = true;
            this.songGridView.Location = new System.Drawing.Point(193, 95);
            this.songGridView.Name = "songGridView";
            this.songGridView.ReadOnly = true;
            this.songGridView.RowHeadersVisible = false;
            this.songGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.songGridView.Size = new System.Drawing.Size(943, 526);
            this.songGridView.TabIndex = 14;
            this.songGridView.RowDrag += new System.Windows.Forms.ItemDragEventHandler(this.songGridView_RowDrag);
            this.songGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.songGridView_CellDoubleClick);
            this.songGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.songGridView_ColumnHeaderMouseClick);
            this.songGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.songGridView_DragDrop);
            this.songGridView.DragEnter += new System.Windows.Forms.DragEventHandler(this.songGridView_DragEnter);
            this.songGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.songGridView_KeyDown);
            this.songGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.songGridView_MouseDown);
            // 
            // playListBindingSource
            // 
            this.playListBindingSource.DataSource = typeof(KoPlayer.PlayLists.PlayList);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1136, 646);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.songLength_Label);
            this.Controls.Add(this.currentTime_Label);
            this.Controls.Add(this.songGridView);
            this.Controls.Add(this.songInfoLabel);
            this.Controls.Add(this.searchBar);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.playpauseButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.playListGridView);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.volumeTrackBar);
            this.Controls.Add(this.albumArtBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximumSize = new System.Drawing.Size(1920, 1280);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.Text = "KoPlayer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.albumArtBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.playListGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.songsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.songGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.playListBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setLibraryPathToolStripMenuItem;
        private System.Windows.Forms.PictureBox albumArtBox;
        private System.Windows.Forms.TrackBar volumeTrackBar;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.DataGridView playListGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.BindingSource playListBindingSource;
        private System.Windows.Forms.BindingSource songsBindingSource;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button playpauseButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.TrackBar searchBar;
        private System.Windows.Forms.Label songInfoLabel;
        private System.Windows.Forms.ToolStripMenuItem newPlaylistToolStripMenuItem;
        private Forms.DataGridViewPlus songGridView;
        private System.Windows.Forms.Label currentTime_Label;
        private System.Windows.Forms.Label songLength_Label;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.ToolStripMenuItem playPauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playPreviousToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}