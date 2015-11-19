using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Forms;

namespace KoPlayer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-nolog")
            {
                Run();
            }
            else
            {
                try
                {
                    Run();
                }
                catch (Exception e)
                {
                    using (var errorLogger = new ErrorLogger(e))
                    {
                        errorLogger.Run();
                    }
                }
            }
        }

        static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
