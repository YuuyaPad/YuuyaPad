using System;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            // Configure the About Dialog
            string ProgramName = "YuuyaPad"; // Program Name
            string Version = "1.0"; // Version
            string Copyright = "Copyright © 2020-2025 Yuuya"; // Copyright
            string License = "This application is licensed under the MIT license."; // License
            var OS = Environment.OSVersion; // OS

            // And display that information
            label1.Text = ProgramName; // Display Program Name
            label2.Text = $"Version {Version}"; // Display Version
            label3.Text = Copyright; // Display Copyright
            label4.Text = $"OS: {OS.VersionString}"; // Display OS
            label5.Text = License; // Display License

            // Check if debug
#if DEBUG
// Debug
label2.Text = $"Version {Version} (Debug)";
#else
            // Release
            label2.Text = $"Version {Version}";
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Close
            this.Close();
        }
    }
}
