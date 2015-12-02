﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer
{
    class DragDropSongs : DataObject
    {
        public bool ReturnPaths { get; set; }

        public DragDropSongs(string format, object data)
            : base(format, data)
        {
            ReturnPaths = true;
        }

        public override object GetData(string test)
        {
            List<DataGridViewRow> rows =
                (List<DataGridViewRow>)base.GetData(test);

            if (ReturnPaths)
            {
                List<string> paths = new List<string>();
                foreach (DataGridViewRow row in rows)
                {
                    Song s = (Song)row.DataBoundItem;
                    if (!paths.Contains(s.Path))
                        paths.Add(s.Path);
                }
                return paths.ToArray();
            }
            return rows;
        }
    }
}
