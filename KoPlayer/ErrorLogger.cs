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
                using (sr = new StreamReader(logPath))
                    oldFile = sr.ReadToEnd();

            using (sw = new StreamWriter(logPath, false))
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
            
            ex = null;
        }

        /// <summary>
        /// Writes an exception to the log file
        /// </summary>
        /// <param name="ex"></param>
        public static void Log(Exception ex)
        {
            using (var logger = new ErrorLogger(ex))
                logger.Run();
        }
    }
}
