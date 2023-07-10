using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (Process.GetProcessesByName("Installer").Length > 1)
            {
                return;
            }

            if (VersionCheck.CheckAppUpdate() == string.Empty)
            {
                return;
            }
         
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Installer());
        }
    }
}
