using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CmlLib.Launcher;
using CmlLib.Utils;
using CmlLib;
using MCServerStatus;
using System.IO;

namespace Mc_Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Downloader_ChangeProgress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // when download file was changed
            // 20%, 30%, 80%, ...
            Console.WriteLine("{0}%", e.ProgressPercentage);
        }
        private void DownloadGame(MProfile profile) // download game files
        {
            MDownloader downloader = new MDownloader(profile);
            downloader.ChangeFile += Downloader_ChangeFile;
            downloader.ChangeProgress += Downloader_ChangeProgress;
            downloader.DownloadAll();
        }

        private void Downloader_ChangeFile(DownloadFileChangedEventArgs e)
        {
            // when the progress of current downloading file was changed
            // [Library] hi.jar - 3/51
            Console.WriteLine("[{0}] {1} - {2}/{3}", e.FileKind.ToString(), e.FileName, e.ProgressedFileCount, e.TotalFileCount);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string path = Path.Combine(appDataPath, @"SkyLauncher\");

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    return;
                }

                // Try to create the directory.
                Directory.CreateDirectory(path);
            }
            finally { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Visible = true;
            var request = WebRequest.Create("https://minotar.net/armor/bust/" + textBox1.Text + "/190.png");

            using (var respone = request.GetResponse())
            using (var stream = respone.GetResponseStream())
            {
                pictureBox1.Image = Bitmap.FromStream(stream);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            MLogin login = new MLogin();
            MSession session = MSession.GetOfflineSession(textBox1.Text);

            session = login.TryAutoLogin();
            Minecraft.Initialize(Path.Combine(appDataPath, @"SkyLauncher\"));
            MProfileInfo[] infos = MProfileInfo.GetProfiles();
            MProfile profile = MProfile.FindProfile(infos, "1.12.2");
            DownloadGame(profile);

            var option = new MLaunchOption()
            {
                // must require
                StartProfile = profile,
                JavaPath = "java.exe", //SET YOUR JAVA PATH (if you want autoset, goto wiki)
                MaximumRamMb = 4096, // MB
                Session = MSession.GetOfflineSession(textBox1.Text),

                // not require
                LauncherName = "McLauncher", // display launcher name at main window
                CustomJavaParameter = "" // set your own java args
            };

            MLaunch launch = new MLaunch(option);
            launch.GetProcess().Start();
        }
    }
}
