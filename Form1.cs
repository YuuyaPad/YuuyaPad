using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ShowTextLength();
            DebugMenu();
        }

        private Font currentFont;

        public void ApplyFontToRichTextBox(RichTextBox rtb, Font newFont)
        {
            if (rtb == null || newFont == null) return;

            // Change all existing text (fast version without preserving styles)
            rtb.SelectAll();
            rtb.SelectionFont = newFont;

            // Reflect new inputs
            rtb.Font = newFont;

            // Deselection
            rtb.Select(rtb.TextLength, 0);

            richTextBox1.Font = newFont;
        }

        private void ShowTextLength()
        {
            // Get the number of characters
            int textLength = richTextBox1.TextLength;
            statusBar1.Text = $"Character count: {textLength}";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Reflects the number of characters in realtime
            ShowTextLength();

            // 
            UpdateMenuState();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            // Show About Dialog
            using (var f = new About())
            {
                f.ShowDialog(this);
            }
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
            // Open Settings
            Settings f = new Settings();

            f.CurrentFont = richTextBox1.Font;
            f.InitSettings();

            if (f.ShowDialog(this) == DialogResult.OK)
            {
                if (f.SelectedFont != null)
                {
                    // Change the entire font
                    richTextBox1.Font = f.SelectedFont;

                    // Apply the new font to the current content too
                    richTextBox1.SelectAll();
                    richTextBox1.SelectionFont = f.SelectedFont;
                    richTextBox1.DeselectAll();

                    // Default input is also in the same font
                    richTextBox1.SelectionFont = f.SelectedFont;
                }
            }

            f.Dispose();
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

        private void menuItem26_Click(object sender, EventArgs e)
        {
            // View YuuyaPad Home page
            Process.Start("https://yuuyapad.github.io/");
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            // View YuuyaPad GitHub page
            Process.Start("https://github.com/YuuyaPad/YuuyaPad");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initializing the Edit menu
            UpdateMenuState();

            // Get search engine settings
            GetSearchEngine();

            currentFont = richTextBox1.Font;

            richTextBox1.TextChanged += (s, ev) =>
            {
                // Prevent fonts from resetting after a full delete
                if (richTextBox1.TextLength == 0 && richTextBox1.Font != currentFont)
                {
                    richTextBox1.Font = currentFont;
                }
            };
        }

        private void Placeholder()
        {
            // Displayed when trying to access a feature under construction
            MessageBox.Show("This feature is still under construction",
                "YuuyaPad",
                MessageBoxButtons.OK,
                MessageBoxIcon.None);
        }

        private void menuItem33_Click(object sender, EventArgs e)
        {
            // Search
            // In the future we plan to allow customization of the search engine.

            string searchText = richTextBox1.SelectedText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If nothing is selected, nothing happens to prevent unexpected behavior.
                return;
            }

            // Open Google Search
            Process.Start($"https://www.google.com/search?q={Uri.EscapeDataString(searchText)}");

        }

        private void menuItem35_Click(object sender, EventArgs e)
        {
            string dateTimeText;

            try
            {
                // Get Device Format Settings
                dateTimeText = DateTime.Now.ToString(System.Globalization.CultureInfo.CurrentCulture);
            }
            catch
            {
                // Use ISO 8601 format as a fallback
                dateTimeText = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            // Insert at cursor position
            richTextBox1.SelectedText = dateTimeText;
        }

        private void menuItem38_Click(object sender, EventArgs e)
        {
            // Select All
            richTextBox1.SelectAll();
        }

        private void menuItem39_Click(object sender, EventArgs e)
        {
            // Deselect All
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionStart = richTextBox1.TextLength;
        }

        private void menuItem23_Click(object sender, EventArgs e)
        {
            // Cut
            if (richTextBox1.SelectedText.Length > 0)
            {
                richTextBox1.Cut();
            }

        }

        private void menuItem24_Click(object sender, EventArgs e)
        {
            // Copy
            if (richTextBox1.SelectedText.Length > 0)
            {
                richTextBox1.Copy();
            }

        }

        private void menuItem25_Click(object sender, EventArgs e)
        {
            // Paste
            if (Clipboard.ContainsText())
            {
                richTextBox1.Paste();
            }

        }

        private void menuItem30_Click(object sender, EventArgs e)
        {
            // Undo
            richTextBox1.Undo();
        }

        private void menuItem31_Click(object sender, EventArgs e)
        {
            //Redo
            richTextBox1.Redo();
        }

        private void UpdateMenuState()
        {
            // Update the enabled/disabled state of Edit menu items

            // Undo/Redo
            menuItem30.Enabled = richTextBox1.CanUndo;
            menuItem31.Enabled = richTextBox1.CanRedo;

            // Cut/Copy/Paste
            bool hasSelection = !string.IsNullOrEmpty(richTextBox1.SelectedText);
            menuItem23.Enabled = hasSelection;
            menuItem24.Enabled = hasSelection;
            menuItem25.Enabled = Clipboard.ContainsText();
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateMenuState();
        }

        private void GetSearchEngine()
        {
            // Get the name of the search engine
            string SearchEngineName = "Google";
            menuItem33.Text = $"&Search for {SearchEngineName}";
        }

        /// <summary>
        /// DEBUG FEATURES
        /// </summary>
        /// This contains code that is for debugging purposes only.
        /// This code will only run in the "Debug" configuration, not in the "Release" configuration.
        /// It is used for development purposes and is not recommended for any other purposes.

        private void DebugMenu() // DEBUG ONLY 
        {
            // Show menu in Debug, hide in Release
            menuItem27.Visible = false;

#if DEBUG
            menuItem27.Visible = true;
#endif
        }

        private void menuItem28_Click(object sender, EventArgs e) // DEBUG ONLY
        {
            // Used to test exception dialogs
            throw new Exception("This is for testing exception handling.");
        }
    }
}