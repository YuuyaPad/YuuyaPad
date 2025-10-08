using System;
using System.Reflection.Emit;
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
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "不明";
            label2.Text = $"バージョン: {version}";

            // Copyright information (AssemblyInfo.cs or your own definition)
            var copyright = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>()
                ?.Copyright ?? "© 2020-2025 Yuuya";
            label3.Text = copyright;

            // Get the OS version
            var os = Environment.OSVersion;
            label4.Text = $"OS: {os.VersionString}";

            label5.Text = "このアプリケーションは MIT ライセンスで提供されています。";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
