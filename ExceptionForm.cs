using System;
using System.Windows.Forms;
using YuuyaPad.Utils;

namespace CustomExceptionApp
{
    public partial class ExceptionForm : Form
    {
        private readonly Exception _exception;
        private readonly string _context;

        public ExceptionForm(Exception ex, string context = "An exception occurred")
        {
            InitializeComponent();
            _exception = ex;
            _context = context;

            lblContext.Text = context;
            txtMessage.Text = ex.Message;
            txtStackTrace.Text = ex.ToString();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                // Copy the exception contents to the clipboard
                string text = $"[{_context}]\r\n{_exception}";
                Clipboard.SetText(text);
            }
            catch
            {
                // If you have problems copying to the clipboard
                MessageBox.Show("Copying to clipboard failed", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Safely close all applications
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close(); // Ignore the exception and continue
        }

        private void ExceptionForm_Load(object sender, EventArgs e)
        {
            SystemMenuHelper.KeepMoveAndCloseOnly(this);
        }
    }
}
