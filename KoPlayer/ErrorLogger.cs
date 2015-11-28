using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoPlayer
{
    /// <summary>
    /// Writes an exception to a log file
    /// </summary>
    class ErrorLogger : IDisposable
    {
        private StreamReader sr;
        private StreamWriter sw;
        private Exception ex;
        private const string logPath = "error.log";

        public ErrorLogger(Exception ex)
        {
            this.ex = ex;
        }

        /// <summary>
        /// Saves exception info to log file
        /// </summary>
        public void Run()
        {
            string oldFile = "";

            if (File.Exists(logPath))
                using (var sr = new StreamReader(logPath))
                    oldFile = sr.ReadToEnd();

            using (var sw = new StreamWriter(logPath, false))
            {
                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine("Error: " + ex.ToString());
                sw.WriteLine();
                sw.WriteLine(oldFile);
            }
        }

        public void Dispose()
        {
            if (sr != null)
                sr.Dispose();

            if (sw != null)
                sw.Dispose();

            if (ex != null)
                ex = null;
        }
    }
}
