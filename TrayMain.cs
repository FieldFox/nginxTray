using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NginxTray
{
    public partial class TrayMain : Form
    {
        private string nginxExe = @"nginx.exe";
        

        public TrayMain()
        {
            InitializeComponent();

            nginxTrayItem.Visible = true;

            initGui(isNginxRunning());
        }

        private void initGui(bool isRunning)
        {
            
            versionToolStripMenuItem.Visible = false;

            toggleNginxToolStripMenuItem.Text = isRunning ? "Stop" : "Start";
            reloadToolStripMenuItem.Enabled = isRunning;

            toggleNginxToolStripMenuItem.Enabled = true;
        }


        // NOT WORKING, could not read STDOUT from nginx
        private string loadNginxVersion()
        {
            var process = getNginxProcess();
            process.StartInfo.Arguments = "-v";

            process.Start();

            var version = process.StandardError.ReadToEnd();

            return string.Empty; // version;
        }

        #region Process Helpers

        private Process getNginxProcess()
        {
            string nginxWorkingDir = getNginxWorkingDir();
            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = Path.Combine(nginxWorkingDir, nginxExe);
            process.StartInfo.WorkingDirectory = nginxWorkingDir;

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            return process;
        }

        private bool isNginxRunning()
        {
            var nginxProcesses = Process.GetProcessesByName("nginx");

            return nginxProcesses.Count() > 0;
        }

        private string getNginxWorkingDir()
        {
            return NginxTray.Properties.Settings.Default.workingDir;
        }

        #endregion

        #region ToolstripItems
        private void toggleNginxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleNginxToolStripMenuItem.Enabled = false;

            var process = getNginxProcess();

            if (isNginxRunning())
            {
                process.StartInfo.Arguments = "-s stop";
                initGui(false);
            }
            else 
            {
                initGui(true);
            }

            process.Start();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var process = getNginxProcess();
            process.StartInfo.Arguments = "-s reload ";
            process.Start();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(getNginxWorkingDir(), "conf", "nginx.conf"));
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(getNginxWorkingDir(), "log"));
        }

        private void htmlWwwToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nginxWorkingDir = Properties.Settings.Default.workingDir;
            Process.Start(Path.Combine(getNginxWorkingDir(), "html"));
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;

            tbExeDir.Text = getNginxWorkingDir();
            if (Directory.Exists(tbExeDir.Text))
            {
                folderBrowserDialog1.SelectedPath = tbExeDir.Text;
            }

            btnOk.Enabled = File.Exists(Path.Combine(tbExeDir.Text, nginxExe));

            this.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Buttons
        private void btnSelectExeDir_Click(object sender, EventArgs e)
        {
            var dialogResult = folderBrowserDialog1.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                tbExeDir.Text = folderBrowserDialog1.SelectedPath;
            }

            btnOk.Enabled = File.Exists(Path.Combine(tbExeDir.Text, nginxExe));
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.workingDir = tbExeDir.Text;
            Properties.Settings.Default.Save();
            WindowState = FormWindowState.Minimized;
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog1.Reset();
            WindowState = FormWindowState.Minimized;
        }
        #endregion

        #region Hidden Form Settings
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.Hide();
            }
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        #endregion
    }
}
