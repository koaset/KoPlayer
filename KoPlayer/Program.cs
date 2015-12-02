using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            bool createdNew = true;
            using (var mutex = new Mutex(true, "KoPlayer", out createdNew))
            {
                // If new start app, else bring existing to front
                if (createdNew)
                    HandleStartup(args);
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
        /// Runs the application according to the args
        /// </summary>
        /// <param name="args"></param>
        static void HandleStartup(string[] args)
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

        /// <summary>
        /// Starts the main form
        /// </summary>
        static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
