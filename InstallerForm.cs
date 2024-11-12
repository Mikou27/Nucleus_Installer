using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Updater
{
    public partial class Installer : Form
    {
        private string version;
        private WebClient webClient;
        public static string destinationPath;
        private bool finished = false;
        private ZipFile zip = null;

        /// <summary>
        /// Installer. This class use try/catch instead of checking files/directory existence because antivirus detect File.Exists() and Directory.Exists() as potencial threats(lol). 
        /// </summary>
        public Installer()
        {
            InitializeComponent();
            version = VersionCheck.CheckAppUpdate();

            if (version == null || version == string.Empty)
            {
                btn_yes.Visible = false;
                btn_no.Visible = false;
                label.ForeColor = Color.Red;
                label.Text = "Unable to retrieve latest Nucleus release version from GitHub";
                return;
            }

            label.Text = $@"Start Nucleus Co-op {version} Download?";
            label.Location = new Point(Width / 2 - label.Width / 2, label.Location.Y);
            TopMost = true;
        }

        private void btn_yes_MouseClick(object sender, MouseEventArgs e)
        {
            if (finished)
            {
                Process.Start(destinationPath);
                Invoke(new Action(delegate
                {
                    Close();
                }));

                return;
            }
            
            destinationPath = DestinationPathDialog();

            if (destinationPath == string.Empty && destinationPath != null && destinationPath != "")
            {
                List<string> destinationPathContent = new List<string>(Directory.GetFileSystemEntries(destinationPath, "*", SearchOption.AllDirectories));

                if (destinationPathContent.Any(file => file.Contains("NucleusCoop.exe")))
                {
                    DialogResult warning = MessageBox.Show("The selected installation path already contains a Nucleus Co-op installation, all customized files and settings will \nbe overwritten.\n\nDo you wish to continue anyway?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (warning == DialogResult.No)
                    {
                        destinationPath = DestinationPathDialog();
                        return;
                    }
                }
            }

            if (destinationPath == string.Empty)
            {
                string message = "Nucleus Co-op should not be installed here.\n\n" +
                                    "Do NOT install in any of these folders:\n" +
                                    "- A folder containing any game files\n" +
                                    "- C:\\Program Files or C:\\Program Files (x86)\n" +
                                    "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                                    "- Any folder with security settings like C:\\Windows\n" +
                                    "\n" +
                                    "A good place is C:\\NucleusCo-op\\NucleusCoop.exe";

                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                destinationPath = DestinationPathDialog();
                return;
            }

            DownloadReleaseZip();
            btn_yes.Visible = false;
            btn_no.Visible = false;
            prog_DownloadBar.Visible = true;
        }

        private static string DestinationPathDialog()
        {
            try
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select the installation directory, remember to add it to your antivirus exceptions list before processing the installation.";

                    DialogResult result = fbd.ShowDialog();

                    Console.WriteLine(fbd.SelectedPath);
                    bool problematic = fbd.SelectedPath.Contains(@"C:\Program Files\") ||
                                        fbd.SelectedPath.Contains(@"C:\Program Files (x86)\") ||
                                        fbd.SelectedPath.Contains(@"C:\Users\") ||
                                        fbd.SelectedPath.Contains(@"C:\Windows\");

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath) && !problematic)
                    {
                        return fbd.SelectedPath;
                    }

                    return string.Empty;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Path not supprted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                destinationPath = DestinationPathDialog();
                return string.Empty;
            }
        }

        private void DownloadReleaseZip()
        {
            DeleteTemp();

            try
            {
                Directory.CreateDirectory((Path.Combine(destinationPath, @"Temp")));

                using (webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += wc_DownloadProgressChanged;
                    webClient.DownloadFileAsync(
                     new System.Uri($@"https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases/download/{version}/NucleusApp.zip"),
                     //new System.Uri($@"https://github.com/Mikou27/splitscreenme-nucleus/releases/download/{version}/NucleusApp.zip"),
                    Path.Combine(destinationPath, @"Temp\\NucleusApp.zip"));
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                }
            }
            catch
            { }
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                Invoke(new Action(delegate
                {
                    prog_DownloadBar.Dispose();
                    label.Text = "Extract and install Nucleus";
                    label.Location = new Point(Width / 2 - label.Width / 2, label.Location.Y);

                }));


                bool isValidZip = ZipFile.CheckZip(Path.Combine(destinationPath, @"Temp\NucleusApp.zip"));

                if (isValidZip)
                {
                    zip = new ZipFile(Path.Combine(destinationPath, @"Temp\NucleusApp.zip"));
                    zip.Password = "nucleus";
                    zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zip.ExtractAll(destinationPath);
                    zip.Dispose();
                }
                else
                {
                    MessageBox.Show("Zip file is missing or corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Invoke(new Action(delegate
                {
                    DeleteTemp();
                    finished = true;

                    btn_yes.Visible = true;
                    btn_yes.Text = "Exit";
                    btn_yes.Location = new Point(Width / 2 - btn_yes.Width / 2, Height / 2 - btn_yes.Height / 2);

                    label.Text = "Installation completed!";
                    label.Location = new Point(Width / 2 - label.Width / 2, label.Location.Y);
                }));

            });
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prog_DownloadBar.Value = e.ProgressPercentage;
            label.Text = e.ProgressPercentage + "%";
            label.Location = new Point((prog_DownloadBar.Location.X + (prog_DownloadBar.Width / 2)) - label.Width / 2, label.Location.Y);
        }

        private void btn_no_MouseClick(object sender, MouseEventArgs e)
        {
            DeleteTemp();
            Close();
        }

        private void DeleteTemp()
        {
            if (destinationPath == null || destinationPath == string.Empty)
            {
                return;
            }

            try
            {
                if (webClient != null)
                {
                    webClient.CancelAsync();
                }

                Directory.Delete(Path.Combine(destinationPath, @"Temp"), true);
            }
            catch /*(Exception ex)*/
            {
                //MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Updater_FormClosing(object sender, FormClosingEventArgs e)
        {
            DeleteTemp();
        }

        private void label_changelog_Click(object sender, EventArgs e)
        {
            Process.Start($@"https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases/tag/{version}");
        }
    }
}
