using System;
using System.Drawing;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Settings : Form
    {
        public Font CurrentFont { get; set; }
        public Font SelectedFont { get; private set; }

        public string SelectedSearchEngine { get; private set; } = "Google";

        public string CustomSearchUrl
        {
            get { return textBox1.Text.Trim(); }
            set { textBox1.Text = value; }
        }

        public Settings()
        {
            InitializeComponent();
            InitSettings();
            SettingsDefault();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Save Settings
            SaveSettings();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Discard Settings
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SettingsDefault()
        {
            comboBox1.SelectedIndex = 0; // Theme
            comboBox2.SelectedIndex = 0; // Language
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Change Font
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = SelectedFont ?? CurrentFont ?? SystemFonts.DefaultFont;

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFont = fd.Font;

                    label4.Font = new Font(fd.Font.FontFamily, 9f, fd.Font.Style);
                    label4.Text = $"{fd.Font.Name}, {fd.Font.SizeInPoints}pt";

                    // Save Font
                    AppSettings.SetFont(fd.Font);
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        public void InitSettings(string currentSearchEngine = "Google")
        {
            // Load Settings
            AppSettings.Load();

            // Check Remember window size Settings
            checkBox1.Checked = (AppSettings.KeepWindowSize == 1);

            // Disable dark mode if you are not Windows 10 1809 or later
            DisableDarkModeControlsIfNotSupported();

            // Radio buttons reflect current settings
            string engine = AppSettings.SearchEngine;
            textBox1.Text = AppSettings.CustomSearchUrl;

            // Set Selection
            radioButton1.Checked = engine == "Google";
            radioButton2.Checked = engine == "Yahoo";
            radioButton3.Checked = engine == "Bing";
            radioButton4.Checked = engine == "DuckDuckGo";
            radioButton5.Checked = engine == "Custom";

            // Enable Custom only
            textBox1.Enabled = radioButton5.Checked;

            Font baseFont = SelectedFont ?? CurrentFont ?? SystemFonts.DefaultFont;

            label4.Font = new Font(baseFont.FontFamily, 9f, baseFont.Style);
            label4.Text = $"{baseFont.Name}, {baseFont.SizeInPoints}pt";
            label4.AutoSize = true;

            if (label3 != null)
            {
                label4.Location = new Point(label3.Right + 40, label3.Top);
            }
        }

        private void SaveSettings()
        {
            SearchEngine();
            AppSettings.KeepWindowSize = checkBox1.Checked ? 1 : 0;

            AppSettings.Save();
        }

        private void DisableDarkModeControlsIfNotSupported()
        {
            // Check if your system is Windows 10 1809 or later
            bool supportsDarkMode =
                Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version.Major >= 10 &&
                Environment.OSVersion.Version.Build >= 17655;

            if (!supportsDarkMode)
            {
                // Disable Theme Settings
                label1.Enabled = false;
                comboBox1.Enabled = false;

                // Show Tooltips
                ToolTip tip = new ToolTip();
                tip.SetToolTip(comboBox1, "Changing this setting requires Windows 10 1809 or later.");
            }
        }

        private void SearchEngine()
        {
            // Save Search Engine
            if (radioButton1.Checked)
                AppSettings.SearchEngine = "Google";
            else if (radioButton2.Checked)
                AppSettings.SearchEngine = "Yahoo";
            else if (radioButton3.Checked)
                AppSettings.SearchEngine = "Bing";
            else if (radioButton4.Checked)
                AppSettings.SearchEngine = "DuckDuckGo";
            else if (radioButton5.Checked)
                AppSettings.SearchEngine = "Custom";

            AppSettings.CustomSearchUrl = textBox1.Text.Trim();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = radioButton5.Checked;
        }
    }
}
