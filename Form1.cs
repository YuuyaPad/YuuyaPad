using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            UpdateStatusBar();
            DebugMenu();
        }

        public FindDialog sf = null;

        private Font currentFont;

        private float zoomFactor = 1.0f; // Current zoom factor

        // RichTextBox printing support class
        public class RichTextBoxPrinter
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left, Top, Right, Bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct CHARRANGE
            {
                public int cpMin;
                public int cpMax;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct FORMATRANGE
            {
                public IntPtr hdc;
                public IntPtr hdcTarget;
                public RECT rc;
                public RECT rcPage;
                public CHARRANGE chrg;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            private const int WM_USER = 0x0400;
            private const int EM_FORMATRANGE = WM_USER + 57;

            public static int Print(RichTextBox rtb, int charFrom, int charTo, PrintPageEventArgs e)
            {
                FORMATRANGE fmtRange;
                RECT rectToPrint;
                RECT rectPage;

                // Convert 1/100 inch to 1/1440 inch (twips)
                rectToPrint.Top = (int)(e.MarginBounds.Top * 14.4);
                rectToPrint.Bottom = (int)(e.MarginBounds.Bottom * 14.4);
                rectToPrint.Left = (int)(e.MarginBounds.Left * 14.4);
                rectToPrint.Right = (int)(e.MarginBounds.Right * 14.4);

                rectPage.Top = (int)(e.PageBounds.Top * 14.4);
                rectPage.Bottom = (int)(e.PageBounds.Bottom * 14.4);
                rectPage.Left = (int)(e.PageBounds.Left * 14.4);
                rectPage.Right = (int)(e.PageBounds.Right * 14.4);

                IntPtr hdc = e.Graphics.GetHdc();

                fmtRange.chrg.cpMin = charFrom;
                fmtRange.chrg.cpMax = charTo;
                fmtRange.hdc = hdc;
                fmtRange.hdcTarget = hdc;
                fmtRange.rc = rectToPrint;
                fmtRange.rcPage = rectPage;

                IntPtr wParam = new IntPtr(1);
                IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
                Marshal.StructureToPtr(fmtRange, lParam, false);

                IntPtr res = SendMessage(rtb.Handle, EM_FORMATRANGE, wParam, lParam);
                Marshal.FreeCoTaskMem(lParam);
                e.Graphics.ReleaseHdc(hdc);

                return res.ToInt32();
            }

            public static void Clear(RichTextBox rtb)
            {
                SendMessage(rtb.Handle, EM_FORMATRANGE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private int checkPrint;

        private void menuItemPrint_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.DocumentName = "YuuyaPad";
            pd.BeginPrint += (s, ev) => { checkPrint = 0; };
            pd.PrintPage += PrintPage;

            PrintDialog dlg = new PrintDialog();
            dlg.Document = pd;

            if (dlg.ShowDialog() == DialogResult.OK)
                pd.Print();
        }

        private void menuItemPrintPreview_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.DocumentName = "YuuyaPad";
            pd.BeginPrint += (s, ev) => { checkPrint = 0; };
            pd.PrintPage += PrintPage;

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = pd;
            preview.Width = 500;
            preview.Height = 400;
            preview.ShowDialog();
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            checkPrint = RichTextBoxPrinter.Print(richTextBox1, checkPrint, richTextBox1.TextLength, e);
            if (checkPrint < richTextBox1.TextLength)
                e.HasMorePages = true;
            else
            {
                e.HasMorePages = false;
                RichTextBoxPrinter.Clear(richTextBox1);
            }
        }

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

        private void UpdateStatusBar()
        {
            // Update the status bar state
            int textLength = richTextBox1.TextLength;
            statusBar1.Text = $"Character count: {textLength}";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Update Edit menu state
            UpdateMenuState();

            // Update Status Bar
            UpdateStatusBar();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            // Show About Dialog
            using (About about = new About())
            {
                // Displayed in the center of the program
                about.StartPosition = FormStartPosition.CenterParent;

                // Modal Displal
                about.ShowDialog(this);
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

            // Displayed in the center of the program
            f.StartPosition = FormStartPosition.CenterScreen;

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
            menuItemPrint_Click(sender, e);
        }

        private void menuItem20_Click(object sender, EventArgs e)
        {
            // Print Preview
            menuItemPrintPreview_Click(sender, e);
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

            // Show status bar by default
            menuItem43.Checked = true;
            statusBar1.Visible = true;

            // Register shortcut keys for zoom
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // Set the initial zoom factor
            richTextBox1.ZoomFactor = zoomFactor;

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

            string SearchURL = "https://www.google.com/search?q="; // Default to Google
            string SearchText = richTextBox1.SelectedText;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // If nothing is selected, nothing happens to prevent unexpected behavior.
                return;
            }

            // Open Google Search
            Process.Start($"{SearchURL}{Uri.EscapeDataString(SearchText)}");
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

        private void menuItem41_Click(object sender, EventArgs e)
        {
            // Find
            FindDialog f = new FindDialog();
            f.Show(this);
        }

        private void FindNext(string searchText)
        {
            // Write Find Next Process here
        }

        private void menuItem42_Click(object sender, EventArgs e)
        {
            // Find Next
            Placeholder();
            // FindNext();
        }

        private void menuItem43_Click(object sender, EventArgs e)
        {
            // Status Bar
            menuItem43.Checked = !menuItem43.Checked;
            statusBar1.Visible = menuItem43.Checked;
        }

        private void menuItem45_Click(object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void menuItem46_Click(object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void menuItem47_Click(object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void ZoomIn()
        {
            // Zoomin
            if (richTextBox1.ZoomFactor < 5.0f) // Max 500%
                richTextBox1.ZoomFactor += 0.1f;
        }

        private void ZoomOut()
        {
            // Zoom out
            if (richTextBox1.ZoomFactor > 0.2f) // Min 20%
                richTextBox1.ZoomFactor -= 0.1f;
        }

        private void ZoomReset()
        {
            // Zoom reset
            richTextBox1.ZoomFactor = 1.0f;
        }

        private void ChangeZoom(float delta)
        {
            zoomFactor += delta;

            // Range limit (50% to 500%)
            if (zoomFactor < 0.5f) zoomFactor = 0.5f;
            if (zoomFactor > 5.0f) zoomFactor = 5.0f;

            richTextBox1.ZoomFactor = zoomFactor;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + + to zoom in
            if (e.Control && e.KeyCode == Keys.Oemplus)
            {
                ChangeZoom(0.1f);
                e.Handled = true;
            }
            // Ctrl + - to zoom out
            else if (e.Control && e.KeyCode == Keys.OemMinus)
            {
                ChangeZoom(-0.1f);
                e.Handled = true;
            }
            // Ctrl + 0 to reset
            else if (e.Control && e.KeyCode == Keys.D0)
            {
                zoomFactor = 1.0f;
                richTextBox1.ZoomFactor = zoomFactor;
                e.Handled = true;
            }
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