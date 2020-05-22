namespace Battleship.Forms
{
    partial class ServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.log = new System.Windows.Forms.TextBox();
            this.sendMessageLabel = new System.Windows.Forms.Label();
            this.sendMessageTextBox = new System.Windows.Forms.TextBox();
            this.sendMessageButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(12, 548);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(976, 140);
            this.log.TabIndex = 0;
            // 
            // sendMessageLabel
            // 
            this.sendMessageLabel.AutoSize = true;
            this.sendMessageLabel.Location = new System.Drawing.Point(12, 525);
            this.sendMessageLabel.Name = "sendMessageLabel";
            this.sendMessageLabel.Size = new System.Drawing.Size(80, 13);
            this.sendMessageLabel.TabIndex = 1;
            this.sendMessageLabel.Text = "Send message:";
            // 
            // sendMessageTextBox
            // 
            this.sendMessageTextBox.Location = new System.Drawing.Point(95, 522);
            this.sendMessageTextBox.Name = "sendMessageTextBox";
            this.sendMessageTextBox.Size = new System.Drawing.Size(214, 20);
            this.sendMessageTextBox.TabIndex = 2;
            // 
            // sendMessageButton
            // 
            this.sendMessageButton.Location = new System.Drawing.Point(315, 520);
            this.sendMessageButton.Name = "sendMessageButton";
            this.sendMessageButton.Size = new System.Drawing.Size(78, 23);
            this.sendMessageButton.TabIndex = 3;
            this.sendMessageButton.Text = "Send";
            this.sendMessageButton.UseVisualStyleBackColor = true;
            this.sendMessageButton.Click += new System.EventHandler(this.SendMessage);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.sendMessageButton);
            this.Controls.Add(this.sendMessageTextBox);
            this.Controls.Add(this.sendMessageLabel);
            this.Controls.Add(this.log);
            this.Name = "ServerForm";
            this.Text = "Battleship";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Label sendMessageLabel;
        private System.Windows.Forms.TextBox sendMessageTextBox;
        private System.Windows.Forms.Button sendMessageButton;
    }
}