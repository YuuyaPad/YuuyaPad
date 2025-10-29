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

            string version = "1.0"; // Version
            string copyright = "Copyright © 2020-2025 Yuuya"; // Copyright
            string license = "This application is licensed under the MIT license."; // License
            var os = Environment.OSVersion; // OS

            label3.Text = copyright; // Display Copyright
            label4.Text = $"OS: {os.VersionString}"; // Display OS
            label5.Text = license; // Display License

            // Check if debug
#if DEBUG
// Debug
label2.Text = $"Version: {version} [Debug]";
#else
            // Release
            label2.Text = $"Version: {version}";
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Close
            this.Close();
        }
    }
}
