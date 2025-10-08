using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShowTextLength();
        }

        private void ShowTextLength()
        {
            // Get the number of characters
            int textLength = richTextBox1.TextLength;
            statusBar1.Text = $"文字数: {textLength}";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Reflects the number of characters in realtime
            ShowTextLength();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            // Show About Dialog
            using (var f = new About())
            {
                f.ShowDialog(this);
            }
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            // View YuuyaPad GitHub page
            Process.Start("https://github.com/YuuyaPad/YuuyaPad");
        }

        private void menuItem13_Click(object sender, EventArgs e)
        {
            // To open a new Form1, use Program.AppContext
            if (Program.AppContext != null)
            {
                Program.AppContext.OpenForm1(new Form1());
            }
            else
            {
                new Form1().Show(); // Just in case
            }
        }


        private void menuItem15_Click(object sender, EventArgs e)
        {
            // Exit Application
            this.Close();
            Application.Exit();
        }

        private void menuItem8_Click(object sender, EventArgs e)
        {
            // New
            Placeholder();
        }

        private void menuItem9_Click(object sender, EventArgs e)
        {
            // Open
            Placeholder();
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            // Save
            Placeholder();
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
            // Save As
            Placeholder();
        }

        private void menuItem17_Click(object sender, EventArgs e)
        {
            // Settings
            Placeholder();
        }

        private void menuItem18_Click(object sender, EventArgs e)
        {
            // Print
            Placeholder();
        }

        private void menuItem20_Click(object sender, EventArgs e)
        {
            // Print Preview
            Placeholder();
        }

        private void Placeholder()
        {
            MessageBox.Show("この機能はまだ未完成です",
                "YuuyaPad",
                MessageBoxButtons.OK,
                MessageBoxIcon.None);
        }

        private void menuItem21_Click(object sender, EventArgs e)
        {
            // Close This Window
            this.Close();
        }

        private void menuItem22_Click(object sender, EventArgs e)
        {
            // Toggle check state
            menuItem22.Checked = !menuItem22.Checked;

            // Toggle RightToLeft of richTextBox1
            if (richTextBox1.RightToLeft == RightToLeft.No)
                richTextBox1.RightToLeft = RightToLeft.Yes; // Right to left like RTL languages
            else
                richTextBox1.RightToLeft = RightToLeft.No; // Defaults
        }
    }
}
