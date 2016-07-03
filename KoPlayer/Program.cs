using System;
using System.Windows.Forms;
using KoPlayer.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KoPlayer
{
    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            Run();
#else
            RunRelease();
#endif
        }

        /// <summary>
        /// Does release build checks before running
        /// </summary>
        /// <param name="args"></param>
        static void RunRelease()
        {
            bool createdNew = true;
            using (var mutex = new Mutex(true, "KoPlayer", out createdNew))
            {
                // If new start app, else bring existing to front
                if (createdNew)
                    RunWithErrorLog();
                else
                {
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs the application with errorlogger
        /// </summary>
        static void RunWithErrorLog()
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

        static void Run()
        {
            // Start the main form
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new MainForm();
            Application.Run(mainForm);

            // Save playlists after form exits
            foreach (var pl in mainForm.Playlists)
                pl.Save();
        }
    }
}
