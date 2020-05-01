namespace Battleship.Forms
{
    partial class MenuForm
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
            this.startGameButton = new System.Windows.Forms.Button();
            this.joinGameButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startGameButton
            // 
            this.startGameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.startGameButton.Location = new System.Drawing.Point(8, 8);
            this.startGameButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.startGameButton.Name = "startGameButton";
            this.startGameButton.Size = new System.Drawing.Size(345, 106);
            this.startGameButton.TabIndex = 0;
            this.startGameButton.Text = "Start Game";
            this.startGameButton.UseVisualStyleBackColor = true;
            this.startGameButton.Click += new System.EventHandler(this.startGameButton_Click);
            // 
            // joinGameButton
            // 
            this.joinGameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.joinGameButton.Location = new System.Drawing.Point(8, 122);
            this.joinGameButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.joinGameButton.Name = "joinGameButton";
            this.joinGameButton.Size = new System.Drawing.Size(345, 106);
            this.joinGameButton.TabIndex = 1;
            this.joinGameButton.Text = "Join Game";
            this.joinGameButton.UseVisualStyleBackColor = true;
            this.joinGameButton.Click += new System.EventHandler(this.joinGameButton_Click);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 238);
            this.Controls.Add(this.joinGameButton);
            this.Controls.Add(this.startGameButton);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MenuForm";
            this.Text = "Battleship";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startGameButton;
        private System.Windows.Forms.Button joinGameButton;
    }
}