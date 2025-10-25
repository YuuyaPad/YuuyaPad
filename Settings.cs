using System;
using System.Drawing;
using System.Drawing.Text;
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
                fd.Font = SelectedFont ?? CurrentFont ?? this.Font;

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFont = fd.Font;
                    label4.Text = $"{fd.Font.Name}, {fd.Font.SizeInPoints}pt";
                    label4.Font = fd.Font;
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // Settings implementation is in early stages
            DialogResult result = MessageBox.Show("Settings is still under construction and may not function properly.\r\nDo you still want to continue?",
                "Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            if (result == DialogResult.Yes)
            {
                // Continue Settings
                ContinueSettings();
            }
            else if (result == DialogResult.No)
            {
                // Close Settings
                this.Close();
            }
        }

        private void InitSettings()
        {
            // Settings Initialization
            label4.AutoSize = true;
            label4.TextAlign = ContentAlignment.MiddleRight;
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }

        private void ContinueSettings()
        {
            InitSettings();

            if (CurrentFont != null)
            {
                label4.Text = $"{CurrentFont.Name}, {CurrentFont.SizeInPoints}pt";
                label4.Font = CurrentFont;
                SelectedFont = CurrentFont;
            }
        }

    }
}
