using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Version = SemVer.Version;

namespace Blish_HUD
{
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        private static readonly Logger Logger;

        private static readonly Mutex SingleInstanceMutex = new Mutex(true, "{5802208e-71ca-4745-ab1b-d851bc17a460}");

        static Program()
        {
            // Make sure logging and logging services are available as soon as possible
            DebugService.InitDebug();
            Logger = Logger.GetLogger(typeof(Program));

            if (!string.IsNullOrEmpty(OverlayVersion.PreRelease))
            {
                Logger.Info("Running PreRelease {preReleaseVersion}", OverlayVersion.PreRelease);
            }

            if (!string.IsNullOrEmpty(OverlayVersion.Build))
            {
                Logger.Info("Running Build {build}", OverlayVersion.Build);
            }
        }

        public static Version OverlayVersion { get; } = new Version(
            typeof(BlishHud).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
            true);

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (IsMoreThanOneInstance())
            {
                Logger.Warn("Blish HUD is already running!");
                return;
            }

            // Needed by textboxes to enable CTRL + A selection
            Application.EnableVisualStyles();

            using (var game = new BlishHud())
            {
                game.Run();
            }

            SingleInstanceMutex.ReleaseMutex();
        }

        private static bool IsMoreThanOneInstance()
        {
            return !SingleInstanceMutex.WaitOne(TimeSpan.Zero, true);
        }
    }
}