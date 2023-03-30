using System;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;

            Mutex mutex = new Mutex(true, "$Installer_Mutex$", out createdNew);

            if (!createdNew)
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
