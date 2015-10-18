using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;

namespace KoPlayer
{
    public class Settings
    {
        public int formWidth = 1000;
        public int formHeight = 600;

        public string partymix_SourcePlayListName = "Library";
        public int partymix_NumPrevious = 5;
        public int partymix_NumNext = 15;

        public Settings()
        {

        }

        /// <summary>
        /// Loads from file
        /// </summary>
        /// <param name="path"></param>
        public Settings(string path)
        {
            Settings loadedSettings = Load(path);
        }

        /// <summary>
        /// Saves the current settings
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        public void Save(string filename)
        {
            Stream stream = File.Create(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Loads settings from a file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public static Settings Load(string filename)
        {
            Stream stream = null;
            Settings loadedSettings = null;
            try
            {
                stream = File.OpenRead(filename);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                loadedSettings = (Settings)serializer.Deserialize(stream);
                stream.Close();
                
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
