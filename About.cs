using System;
using System.Reflection;
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
            // Get app version from assembly information
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            label2.Text = $"Version: {version}";

            // Copyright information (AssemblyInfo.cs or your own definition)
            var copyright = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>()
                ?.Copyright ?? "© 2020-2025 Yuuya";
            label3.Text = copyright;

            // Get the OS version
            var os = Environment.OSVersion;
            label4.Text = $"OS: {os.VersionString}";

            // License information
            label5.Text = "This application is licensed under the MIT license.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Close
            this.Close();
        }
    }
}
