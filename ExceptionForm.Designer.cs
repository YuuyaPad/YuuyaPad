using System.Windows.Forms;

namespace CustomExceptionApp
{
    partial class ExceptionForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblContext;
        private TextBox txtMessage;
        private TextBox txtStackTrace;
        private Button btnClose;
        private Button btnCopy;
        private Button btnExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblContext = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.txtStackTrace = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblContext
            // 
            this.lblContext.AutoSize = true;
            this.lblContext.Font = new System.Drawing.Font("MS UI Gothic", 10F, System.Drawing.FontStyle.Bold);
            this.lblContext.Location = new System.Drawing.Point(12, 9);
            this.lblContext.Name = "lblContext";
            this.lblContext.Size = new System.Drawing.Size(130, 14);
            this.lblContext.TabIndex = 0;
            this.lblContext.Text = "例外が発生しました:";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(12, 35);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(560, 50);
            this.txtMessage.TabIndex = 1;
            // 
            // txtStackTrace
            // 
            this.txtStackTrace.Location = new System.Drawing.Point(12, 95);
            this.txtStackTrace.Multiline = true;
            this.txtStackTrace.Name = "txtStackTrace";
            this.txtStackTrace.ReadOnly = true;
            this.txtStackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtStackTrace.Size = new System.Drawing.Size(560, 250);
            this.txtStackTrace.TabIndex = 2;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(372, 355);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "続行(&C)";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(16, 355);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(100, 30);
            this.btnCopy.TabIndex = 3;
            this.btnCopy.Text = "エラーをコピー(&O)";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(478, 355);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(94, 30);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "終了(&Q)";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // ExceptionForm
            // 
            this.ClientSize = new System.Drawing.Size(584, 391);
            this.Controls.Add(this.lblContext);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtStackTrace);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExceptionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "例外が発生しました";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
