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
                    // Reflect this appropriately in richTextBox1
                    SelectedFont = fd.Font;

                    // Label4: Reflects only the font name and style (size is fixed at 9pt)
                    float fixedLabelSize = 9f;
                    label4.Font = new Font(fd.Font.FontFamily, fixedLabelSize, fd.Font.Style);

                    // Display content
                    label4.Text = $"{fd.Font.Name}, {fd.Font.SizeInPoints}pt";

                    // Auto size (prevent cut-off)
                    label4.AutoSize = true;

                    // Placed to the right of label3
                    label4.Location = new Point(
                        label3.Right + 40,  // 40px margin to the left
                        label3.Top
                    );
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

        private void InitSettings()
        {
            // The code to initialize the font and size display will be inserted later
        }

        private void ContinueSettings()
        {
            InitSettings();
        }

    }
}
