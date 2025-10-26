using System;
using System.Drawing;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Settings : Form
    {
        public Font CurrentFont { get; set; }
        public Font SelectedFont { get; private set; }

        public Settings()
        {
            InitializeComponent();
            InitSettings();
            SettingsDefault();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Save Settings
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
                // Based on the current or selected font
                fd.Font = SelectedFont ?? CurrentFont ?? SystemFonts.DefaultFont;

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    // To apply to richTextBox1
                    SelectedFont = fd.Font;

                    // Label4's appearance remains the same size, only the font is changed
                    label4.Font = new Font(fd.Font.FontFamily, 9f, fd.Font.Style);
                    label4.Text = $"{fd.Font.Name}, {fd.Font.SizeInPoints}pt";
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // Do not show even if you select No
            this.Hide();

            // Settings implementation is in early stages
            DialogResult result = MessageBox.Show("Settings is still under construction and may not function properly.\r\nDo you still want to continue?",
                "Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            if (result == DialogResult.Yes)
            {
                // Continue Settings
                this.Show();
                ContinueSettings();
            }
            else if (result == DialogResult.No)
            {
                // Close Settings
                this.Close();
            }
        }

        public void InitSettings()
        {
            Font baseFont = SelectedFont ?? CurrentFont ?? SystemFonts.DefaultFont;

            label4.Font = new Font(baseFont.FontFamily, 9f, baseFont.Style);
            label4.Text = $"{baseFont.Name}, {baseFont.SizeInPoints}pt";
            label4.AutoSize = true;

            if (label3 != null)
            {
                label4.Location = new Point(label3.Right + 40, label3.Top);
            }
        }

        private void ContinueSettings()
        {
            // Yes
            InitSettings();
        }

    }
}
