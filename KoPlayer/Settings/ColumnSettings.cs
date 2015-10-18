using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace KoPlayer
{
    public class ColumnSetting
    {
        public int DisplayIndex;
        public string Name;
        public int Width;
        public bool Visible;

        public ColumnSetting()
        {
            this.DisplayIndex = 0;
            this.Name = "";
            this.Width = 0;
            this.Visible = false;
        }
    }

    public class ColumnSettings
    {
        public List<ColumnSetting> SettingList;

        public ColumnSettings()
        {
            SettingList = new List<ColumnSetting>(); ;
        }

        public ColumnSettings(DataGridViewColumnCollection columns)
        {
            SettingList = new List<ColumnSetting>(columns.Count);

            foreach (DataGridViewColumn c in columns)
            {
                ColumnSetting cs = new ColumnSetting();
                cs.DisplayIndex = c.DisplayIndex;
                cs.Name = c.Name;
                cs.Width = c.Width;
                cs.Visible = c.Visible;
                SettingList.Add(cs);
            }
        }

        public void Save(string path)
        {
            Stream stream = File.Create(path);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ColumnSettings));
                serializer.Serialize(stream, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Column settings save error: " + ex.ToString());
            }
            finally
            {
                stream.Close();
            }
        }

        public static ColumnSettings Load(string path)
        {
            Stream stream = null;
            XmlSerializer serializer;
            ColumnSettings loadedSettings;
            try
            {
                stream = File.OpenRead(path);
                serializer = new XmlSerializer(typeof(ColumnSettings));
                loadedSettings = (ColumnSettings)serializer.Deserialize(stream);
            }
            catch 
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            return loadedSettings;
        }
    }
}
