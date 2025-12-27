using System;
using System.Windows.Forms;
using YuuyaPad.Utils;

namespace YuuyaPad
{
    public partial class FindDialog : Form
    {
        private Form1 ownerForm;

        public FindDialog(Form1 owner)
        {
            InitializeComponent();

            this.ownerForm = owner;

            this.StartPosition = FormStartPosition.Manual;
            this.KeyPreview = true;

            this.KeyDown += FindDialog_KeyDown;

            // Disable search button when text is empty
            button1.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
            textBox1.TextChanged += (s, e) => button1.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ownerForm == null) return;

            string keyword = textBox1.Text;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            // Create a flag
            RichTextBoxFinds flags = RichTextBoxFinds.None;
            if (checkBox1.Checked) // Case-sensitive
                flags |= RichTextBoxFinds.MatchCase;
            if (checkBox3.Checked) // Word by word
                flags |= RichTextBoxFinds.WholeWord;

            ownerForm.lastSearchWrap = checkBox2.Checked;

            // Passing search parameters to Form1
            ownerForm.SetSearchParams(keyword, flags);

            // Actually search for the next
            ownerForm.FindNext();

            // Restore focus to ensure search result highlights are visible
            ownerForm.Focus();
            ownerForm.Editor?.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }

        private void FindDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick(); // Search
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                button2.PerformClick(); // Close
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FindDialog_Load(object sender, EventArgs e)
        {
            SystemMenuHelper.KeepMoveAndCloseOnly(this);
        }
    }
}
