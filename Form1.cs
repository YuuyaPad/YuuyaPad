using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace YuuyaPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            UpdateStatusBar();
            UpdateTitle();
            EnableDragAndDrop();

            DebugMenu();
        }

        private const int WM_PASTE = 0x0302;

        public RichTextBox Editor => richTextBox1;

        public FindDialog sf = null;

        private int lastSearchIndex = -1;
        private string lastSearchText = "";
        public bool lastSearchWrap = false;
        private RichTextBoxFinds lastSearchFlag = RichTextBoxFinds.None;

        private Font currentFont;

        private float zoomFactor = 1.0f; // Current zoom factor

        private string currentSearchEngine = "Google";

        private FindDialog findDialog = null;

        private string currentFilePath = null;
        private bool isModified = false;
        private bool isLoading = false;

        private bool isInternalUpdate = false;

        private string lastTextSnapshot = "";

        private ContextMenu rtbContextMenu;

        private Encoding currentEncoding = new UTF8Encoding(false);

        private bool suppressTextChanged = false;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PASTE)
            {
                // Rejects when an image is pasted
                if (Clipboard.ContainsImage())
                {
                    return; // Do not paste
                }

                // Allow pasting only if it is text
                if (Clipboard.ContainsText())
                {
                    richTextBox1.SelectedText = Clipboard.GetText();
                    return;
                }

                // Pasting other than the above is prohibited
                return;
            }

            base.WndProc(ref m);
        }

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

        private void EnableDragAndDrop()
        {
            // Enable Drag & Drop
            richTextBox1.AllowDrop = true;

            // Event Handler Registration
            richTextBox1.DragEnter += richTextBox1_DragEnter;
            richTextBox1.DragDrop += richTextBox1_DragDrop;
        }

        private void richTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            richTextBox1.ClearUndo();
            isModified = false;

            // Show copy effect only when file is dropped
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    if (!CheckUnsavedChanges())
                        return;

                    LoadFileToEditor(files[0]);
                }
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
            int index = richTextBox1.SelectionStart;
            int line = richTextBox1.GetLineFromCharIndex(index);
            int col = index - richTextBox1.GetFirstCharIndexOfCurrentLine();

            statusBar1.Text = $"{line + 1} row, {col + 1} column　{textLength} characters";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!suppressTextChanged)
            {
                isModified = true;
                UpdateTitle();

                // Update Edit menu state
                UpdateMenuState();

                // Update Status Bar
                UpdateStatusBar();
            }
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            // About

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
            // New Window

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
            SaveWindowsSize();
            this.Close();
            Application.Exit();
        }

        private void menuItem8_Click(object sender, EventArgs e)
        {
            // New
            if (!CheckUnsavedChanges())
                return;

            richTextBox1.Clear();
            currentFilePath = null;
            isModified = false;
            UpdateTitle();

            NewFile();
        }

        private void menuItem9_Click(object sender, EventArgs e)
        {
            // Open
            if (!CheckUnsavedChanges())
                return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Open";
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadFileToEditor(ofd.FileName);
                }
            }
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            // Save
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                // Overwrite if file already exists
                SaveFile(currentFilePath);
            }
            else
            {
                // If there is no file, it will behave the same as "Save As"
                menuItem11_Click(sender, e);
            }
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
            // Save As
            SaveFileAs();
        }

        private void menuItem17_Click(object sender, EventArgs e)
        {
            // Open Settings
            Settings f = new Settings();

            // Displayed in the center of the program
            f.StartPosition = FormStartPosition.CenterScreen;

            f.CurrentFont = richTextBox1.Font;
            f.InitSettings(currentSearchEngine); // Pass the current search engine

            if (f.ShowDialog(this) == DialogResult.OK)
            {
                // Update Search Menu Text
                UpdateSearchMenuText();

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

                // Search engine updates
                if (!string.IsNullOrEmpty(f.SelectedSearchEngine))
                    currentSearchEngine = f.SelectedSearchEngine;
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
            // Load saved settings
            AppSettings.Load();

            // Check if you are running as administrator
            GetRunAdmin();

            // Use system UI font
            richTextBox1.LanguageOption = RichTextBoxLanguageOptions.UIFonts;

            // Enable Context Menu
            InitializeRichTextBoxContextMenu();

            // Load font settings as the top priority
            Font loadedFont = AppSettings.GetFont();
            richTextBox1.Font = loadedFont;
            currentFont = loadedFont;

            // Initialize Encoding menu
            InitializeEncodingMenu();

            // Initializing the Edit menu
            UpdateMenuState();

            // Get search engine settings
            GetSearchEngine();
            UpdateSearchMenuText();

            // Restore Window Size
            LoadWindowSize();

            // Show status bar by default
            statusBar1.Visible = AppSettings.ShowStatusBar;
            menuItem43.Checked = AppSettings.ShowStatusBar;

            // Register shortcut keys for zoom
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // Register the FormClosed event
            this.FormClosed += Form1_FormClosed;

            // Register the TextChanged event
            richTextBox1.TextChanged += richTextBox1_TextChanged;

            // Set default encoding to UTF-8 (Without BOM)
            menuItem50.Checked = true;
            currentEncoding = new UTF8Encoding(false);

            // Set the initial zoom factor
            richTextBox1.ZoomFactor = zoomFactor;

            richTextBox1.KeyPress += (s, ev) =>
            {
                isModified = true;
            };

            richTextBox1.TextChanged += (s, ev) =>
            {
                if (richTextBox1.Focused)
                    isModified = true;
            };
        }

        private void menuItem33_Click(object sender, EventArgs e)
        {
            string searchText = richTextBox1.SelectedText.Trim();
            if (string.IsNullOrEmpty(searchText)) return;

            // Load Latest Settings
            AppSettings.Load();

            string baseUrl = "";
            string url = "";

            // URL determination based on search engine
            switch (AppSettings.SearchEngine)
            {
                case "Yahoo":
                    baseUrl = "https://search.yahoo.co.jp/search?p=";
                    break;
                case "Bing":
                    baseUrl = "https://www.bing.com/search?q=";
                    break;
                case "DuckDuckGo":
                    baseUrl = "https://duckduckgo.com/?q=";
                    break;
                case "Custom":
                    baseUrl = AppSettings.CustomSearchUrl;
                    break;
                default:
                    baseUrl = "https://www.google.com/search?q=";
                    break;
            }

            // For Custom URLs
            if (AppSettings.SearchEngine == "Custom")
            {
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    // If it contains (searchText), it will be replaced. If it does not contain (searchText), it will be concatenated to the end.
                    if (baseUrl.Contains("(searchText)"))
                    {
                        url = baseUrl.Replace("(searchText)", Uri.EscapeDataString(searchText));
                    }
                    else
                    {
                        if (!baseUrl.EndsWith("=") && !baseUrl.EndsWith("?") && !baseUrl.EndsWith("/"))
                            baseUrl += "=";
                        url = baseUrl + Uri.EscapeDataString(searchText);
                    }
                }
                else
                {
                    url = "https://www.google.com/search?q=" + Uri.EscapeDataString(searchText);
                }
            }
            else
            {
                url = baseUrl + Uri.EscapeDataString(searchText);
            }

            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open browser.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Search
            menuItem33.Enabled = !string.IsNullOrWhiteSpace(richTextBox1.SelectedText);
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            // Update Edit menu state on selection change
            UpdateMenuState();
            UpdateStatusBar();

            // Update Search menu item state
            menuItem33.Enabled = !string.IsNullOrWhiteSpace(richTextBox1.SelectedText);
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
            if (findDialog == null || findDialog.IsDisposed)
            {
                findDialog = new FindDialog(this);

                findDialog.StartPosition = FormStartPosition.Manual;
                findDialog.Location = new Point(
                    this.Location.X + (this.Width - findDialog.Width) / 2,
                    this.Location.Y + (this.Height - findDialog.Height) / 2
                );

                findDialog.FormClosed += (s, args) => findDialog = null;
                findDialog.Show(this);
            }
            else
            {
                findDialog.Activate();
            }
        }

        public bool FindNext()
        {
            if (string.IsNullOrEmpty(lastSearchText))
                return false;

            RichTextBoxFinds flags = lastSearchFlag;
            int start = richTextBox1.SelectionStart + richTextBox1.SelectionLength;

            // First search from current location
            int index = richTextBox1.Find(lastSearchText, start, flags);

            if (index >= 0)
            {
                richTextBox1.Select(index, lastSearchText.Length);
                richTextBox1.ScrollToCaret();
                this.Activate();
                return true;
            }

            // If not found, search from the beginning if wrap search is ON
            if (lastSearchWrap)
            {
                index = richTextBox1.Find(lastSearchText, 0, flags);

                if (index >= 0)
                {
                    richTextBox1.Select(index, lastSearchText.Length);
                    richTextBox1.ScrollToCaret();
                    this.Activate();
                    return true;
                }
            }

            // If you still can't find it
            MessageBox.Show($"Cannot find '{lastSearchText}'","YuuyaPad - Find",MessageBoxButtons.OK,MessageBoxIcon.Information);
            return false;
        }


        public void SetSearchParams(string text, RichTextBoxFinds flags)
        {
            if (string.IsNullOrEmpty(text))
            {
                lastSearchText = "";
                lastSearchFlag = RichTextBoxFinds.None;
                lastSearchIndex = -1;
                return;
            }

            lastSearchText = text;
            lastSearchFlag = flags;

            // Start searching just before the current selection
            lastSearchIndex = Math.Max(0, richTextBox1.SelectionStart - 1);
        }

        private void menuItem42_Click(object sender, EventArgs e)
        {
            // Find Next
            FindNext();
        }

        private void menuItem43_Click(object sender, EventArgs e)
        {
            // Show Status Bar
            menuItem43.Checked = !menuItem43.Checked;
            statusBar1.Visible = menuItem43.Checked;

            //Reflect the display of the status bar in settings
            AppSettings.ShowStatusBar = menuItem43.Checked;
            AppSettings.Save();
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

        private void menuItem49_Click(object sender, EventArgs e)
        {
            // Changing the character code
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isModified) return;

            // If not saved to a file, it will be "Untitled"
            var fileName = string.IsNullOrEmpty(currentFilePath) ? "Untitled" : currentFilePath;

            // Save confirmation message
            var result = MessageBox.Show(
                $"{fileName} has been modified and not yet saved.\nDo you want to save your changes?",
                "YuuyaPad",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                // Remember window size and settings in settings
                SaveWindowsSize();

                if (!SaveFileAs())
                {
                    e.Cancel = true;
                }
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true; // Do not close if cancel is pressed
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.LinkText,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the link:\n{ex.Message}", "YuuyaPad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSearchMenuText()
        {
            AppSettings.Load();

            string name = AppSettings.SearchEngine;

            if (string.IsNullOrEmpty(name))
                name = "Google";

            if (name == "Custom")
                name = "User defined"; // Custom is displayed as User defined

            menuItem33.Text = "&Search " + name;
        }

        private void NewFile()
        {
            suppressTextChanged = true;

            richTextBox1.Clear();
            currentFilePath = null;
            isModified = false;

            UpdateTitle();

            suppressTextChanged = false;
        }

        private bool SaveFile(string path)
        {
            try
            {
                File.WriteAllText(path, richTextBox1.Text);

                suppressTextChanged = true;

                currentFilePath = path;
                isModified = false;

                UpdateTitle();

                suppressTextChanged = false;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file.\n{ex.Message}", "Error");
                return false;
            }
        }

        private bool SaveFileAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save As";
                sfd.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";

                // Prohibited characters
                char[] invalidChars = Path.GetInvalidFileNameChars();

                // Default File Name
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    string rawName = "";

                    // The first 10 characters of the file name are automatically determined
                    if (richTextBox1.TextLength > 0)
                        rawName = richTextBox1.Text.Substring(0, Math.Min(10, richTextBox1.Text.Length));
                    else
                        rawName = "Untitled";

                    // Exclude prohibited characters
                    string safeName = string.Concat(rawName.Where(c => !invalidChars.Contains(c)));

                    // Fallback in case it becomes empty
                    if (string.IsNullOrWhiteSpace(safeName))
                        safeName = "Untitled";

                    sfd.FileName = safeName + ".txt";
                }
                else
                {
                    sfd.FileName = Path.GetFileName(currentFilePath);
                }

                // Save
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = sfd.FileName;
                    return SaveFile(currentFilePath);
                }

                return false;
            }
        }

        // Show the file you are working on in the title bar
        private void UpdateTitle()
        {
            isInternalUpdate = true;

            string name = currentFilePath == null ? "Untitled" : Path.GetFileName(currentFilePath);

#if DEBUG
            this.Text = name + (isModified ? "*" : "") + " - YuuyaPad [Debug]" ;
#else
            this.Text = name + (isModified ? "*" : "") + " - YuuyaPad";
#endif

            isInternalUpdate = false;
        }

        private void LoadFileToEditor(string path)
        {
            suppressTextChanged = true;

            string content = File.ReadAllText(path);
            richTextBox1.Text = content;

            currentFilePath = path;
            isModified = false;

            suppressTextChanged = false;

            UpdateTitle();
        }

        private bool CheckUnsavedChanges()
        {
            if (!isModified) return true;

            string filename = string.IsNullOrEmpty(currentFilePath)
                                ? "Untitled"
                                : currentFilePath;

            DialogResult result = MessageBox.Show(
                $"{filename} has been modified and not yet saved.\nDo you want to save your changes?",
                "YuuyaPad",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
                return SaveFile(currentFilePath); // Save and Continue

            if (result == DialogResult.No)
                return true;       // Continue without Saving

            return false;          // Cancel
        }

        private void InitializeEncodingMenu()
        {
            // Add a click event
            menuItem50.Click += EncodingMenu_Click;  // UTF-8
            menuItem51.Click += EncodingMenu_Click;  // UTF-8 (BOM)
            menuItem52.Click += EncodingMenu_Click;  // Shift-JIS
            menuItem53.Click += EncodingMenu_Click;  // EUC-JP
            menuItem54.Click += EncodingMenu_Click;  // Unicode

            // Default: UTF-8
            menuItem50.Checked = true;
        }

        private void EncodingMenu_Click(object sender, EventArgs e)
        {
            // Uncheck all
            menuItem50.Checked = false;
            menuItem51.Checked = false;
            menuItem52.Checked = false;
            menuItem53.Checked = false;
            menuItem54.Checked = false;

            // Check selected menu item
            MenuItem item = (MenuItem)sender;
            item.Checked = true;

            if (item == menuItem50)
                currentEncoding = new UTF8Encoding(false); // UTF-8 (Without BOM)

            else if (item == menuItem51)
                currentEncoding = new UTF8Encoding(true); // UTF-8 (With BOM)

            else if (item == menuItem52)
                currentEncoding = Encoding.GetEncoding("shift_jis"); // Shift-JIS

            else if (item == menuItem53)
                currentEncoding = Encoding.GetEncoding("euc-jp"); // EUC-JP

            else if (item == menuItem54)
                currentEncoding = Encoding.Unicode; // Unicode
        }

        private void InitializeRichTextBoxContextMenu()
        {
            rtbContextMenu = new ContextMenu();

            // Create Menu
            MenuItem cut = new MenuItem("&Cut", (s, e) => richTextBox1.Cut());
            MenuItem copy = new MenuItem("C&opy", (s, e) => richTextBox1.Copy());
            MenuItem paste = new MenuItem("&Paste", (s, e) => richTextBox1.Paste());
            MenuItem selectAll = new MenuItem("&Select All", (s, e) => richTextBox1.SelectAll());

            // Add to right-click menu
            rtbContextMenu.MenuItems.Add(cut);
            rtbContextMenu.MenuItems.Add(copy);
            rtbContextMenu.MenuItems.Add(paste);
            rtbContextMenu.MenuItems.Add("-");
            rtbContextMenu.MenuItems.Add(selectAll);

            // Assign the context menu to the RichTextBox
            richTextBox1.ContextMenu = rtbContextMenu;

            // Update menu enable/disable state on right-click
            richTextBox1.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    cut.Enabled = richTextBox1.SelectionLength > 0;
                    copy.Enabled = richTextBox1.SelectionLength > 0;
                    paste.Enabled = Clipboard.ContainsText();
                    selectAll.Enabled = richTextBox1.TextLength > 0;
                }
            };
        }

        public bool FindText(string text, bool matchCase, bool wrap, bool wholeWord)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            RichTextBoxFinds options = RichTextBoxFinds.None;

            if (matchCase)
                options |= RichTextBoxFinds.MatchCase;

            if (wholeWord)
                options |= RichTextBoxFinds.WholeWord;

            int startPos = richTextBox1.SelectionStart + richTextBox1.SelectionLength;

            // First, search downwards from the current position
            int index = richTextBox1.Find(text, startPos, options);

            // Not found -> If wrap is enabled, search from the beginning
            if (index == -1 && wrap)
            {
                index = richTextBox1.Find(text, 0, options);
            }

            // Not Found
            if (index == -1)
                return false;

            // Select the location where it was found
            richTextBox1.SelectionStart = index;
            richTextBox1.SelectionLength = text.Length;
            richTextBox1.ScrollToCaret();

            return true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveWindowsSize();
        }

        private void LoadWindowSize()
        {
            AppSettings.Load();

            if (AppSettings.KeepWindowSize == 1)
            {
                AppSettings.LoadWindowSize();

                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(AppSettings.WindowX, AppSettings.WindowY);
                this.Size = new Size(AppSettings.WindowWidth, AppSettings.WindowHeight);

                if (AppSettings.WindowMaximized)
                    this.WindowState = FormWindowState.Maximized;
            }
        }

        private void SaveWindowsSize()
        {
            if (AppSettings.KeepWindowSize == 1)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    AppSettings.WindowX = this.Location.X;
                    AppSettings.WindowY = this.Location.Y;
                    AppSettings.WindowWidth = this.Size.Width;
                    AppSettings.WindowHeight = this.Size.Height;
                    AppSettings.WindowMaximized = false;
                }
                else if (this.WindowState == FormWindowState.Maximized)
                {
                    AppSettings.WindowMaximized = true;
                }

                AppSettings.SaveWindowSize();
            }
        }

        private void GetRunAdmin()
        {
            if (CheckAdmin.IsRunAsAdmin())
            {
                if (!this.Text.EndsWith(" [Admin]"))
                {
                    this.Text += " [Admin]";
                }
            }
        }

        private void Placeholder()
        {
            // Displayed when trying to access a feature under construction
            MessageBox.Show("This feature is still under construction",
                "YuuyaPad",
                MessageBoxButtons.OK,
                MessageBoxIcon.None);
        }

        public static bool IsRunAsAdmin()
        {
            try
            {
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(id);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Prevent beeps from playing when a rich text box has focus using Backspace, Delete, arrow keys, etc.
            if (richTextBox1.Focused)
            {
                switch (keyData)
                {
                    case Keys.Back:
                        if (richTextBox1.SelectionStart == 0 && richTextBox1.SelectionLength == 0)
                            return true;
                        break;

                    case Keys.Delete:
                        if (richTextBox1.SelectionStart >= richTextBox1.TextLength)
                            return true;
                        break;

                    case Keys.Left:
                        if (richTextBox1.SelectionStart == 0 && richTextBox1.SelectionLength == 0)
                            return true;
                        break;

                    case Keys.Right:
                        if (richTextBox1.SelectionStart >= richTextBox1.TextLength)
                            return true;
                        break;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Debug Features
        /// </summary>
        /// <usage>This contains code that is for debugging purposes only.</usage>
        /// <desc>This code will only run in the "Debug" configuration, not in the "Release" configuration.</desc>
        /// <note>It is used for development purposes and is not recommended for any other purposes.</note>

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