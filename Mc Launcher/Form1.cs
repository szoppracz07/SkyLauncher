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
using DiscordRPC;
using DiscordRPC.Logging;
using System.Runtime.InteropServices;

namespace Mc_Launcher
{
    public partial class SkyLauncher : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
 (
      int nLeftRect,
      int nTopRect,
      int nRightRect,
      int nBottomRect,
      int nWidthEllipse,
         int nHeightEllipse

  );

        public SkyLauncher()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }

        public DiscordRpcClient client;
        bool initalized = false;

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

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            textBox1.Text = Properties.Settings.Default.nick;

            check_nick_load();

            comboBox1.Items.Add("1.16.4");
            comboBox1.Items.Add("1.16.2");
            comboBox1.Items.Add("1.16.1");
            comboBox1.Items.Add("1.14.4");
            comboBox1.Items.Add("1.12.2");
            comboBox1.Items.Add("1.8.8");

            if (Properties.Settings.Default.wersja.Length != 0)
            {
                comboBox1.SelectedIndex = comboBox1.FindStringExact(Properties.Settings.Default.wersja);
            }

            initalized = true;
            client = new DiscordRpcClient("548882106567360513");
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.Initialize();
            client.SetPresence(new DiscordRPC.RichPresence()
            {
                Details = "Playing Minecraft!",
                State = "sky-launcher.github.io",
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "s",
                    LargeImageText = "Sky Launcher",
                }
            });

            string cheatpath = Path.Combine(appDataPath, @"SkyLauncher\versions\Sigma");

            if (Directory.Exists(cheatpath))
            {
                comboBox1.Items.Add("Sigma");
            }

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

        private void check_nick_load()
        {
            if (textBox1.Text.Length <= 2)
            {
                return;
            }
            if (textBox1.Text.Length >= 17)
            {
                return;
            }

            button1.Visible = true;
            label2.Visible = true;
            fakegraj.Visible = false;
            var request = WebRequest.Create("https://minotar.net/armor/bust/" + textBox1.Text + "/65.png");
            label3.Text = "Hello, " + textBox1.Text;

            using (var respone = request.GetResponse())
            using (var stream = respone.GetResponseStream())
            {
                pictureBox1.Image = Bitmap.FromStream(stream);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.nick = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length <= 2)
            {
                MessageBox.Show("Nick cannot be shorter than 3 letters / numbers!", "SkyLauncher");
                return;
            }
            if (textBox1.Text.Length >= 17)
            {
                MessageBox.Show("Nick cannot be longer than 16 letters / numbers!", "SkyLauncher");
                return;
            }

            Properties.Settings.Default.nick = textBox1.Text;
            Properties.Settings.Default.Save();

            button1.Visible = true;
            label2.Visible = true;
            fakegraj.Visible = false;
            var request = WebRequest.Create("https://minotar.net/armor/bust/" + textBox1.Text + "/65.png");
            label3.Text = "Hello, " + textBox1.Text;

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
            MProfile profile = MProfile.FindProfile(infos, comboBox1.SelectedItem.ToString());
            DownloadGame(profile);

            var option = new MLaunchOption()
            {
                // must require
                StartProfile = profile,
                JavaPath = "java.exe", //JAVA PAT
                MaximumRamMb = 4096, // MB
                Session = MSession.GetOfflineSession(textBox1.Text),

                // not require
                LauncherName = "SkyLauncher", 
                CustomJavaParameter = "" // java args
            };

            MLaunch launch = new MLaunch(option);
            launch.GetProcess().Start();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.wersja = comboBox1.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        int mouseX = 0, mouseY = 0;
        bool mouseDown;

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseX = MousePosition.X - 200;
                mouseY = MousePosition.Y - 40;

                this.SetDesktopLocation(mouseX, mouseY);
            }
        }

        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://sky-launcher.github.io");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/fvnWS8V");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://szoppracz07.ovh");
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }
    }
}
