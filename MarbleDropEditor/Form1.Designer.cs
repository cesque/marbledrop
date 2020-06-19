namespace MarbleDropEditor
{
    partial class Form1
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
            this.playButton = new System.Windows.Forms.Button();
            this.monogameEditorComponent1 = new MarbleDropEditor.MonogameEditorComponent();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // playButton
            // 
            this.playButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.Location = new System.Drawing.Point(13, 581);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(378, 35);
            this.playButton.TabIndex = 1;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // monogameEditorComponent1
            // 
            this.monogameEditorComponent1.ForeColor = System.Drawing.Color.Salmon;
            this.monogameEditorComponent1.Location = new System.Drawing.Point(397, 12);
            this.monogameEditorComponent1.MouseHoverUpdatesOnly = false;
            this.monogameEditorComponent1.Name = "monogameEditorComponent1";
            this.monogameEditorComponent1.Size = new System.Drawing.Size(935, 605);
            this.monogameEditorComponent1.TabIndex = 0;
            this.monogameEditorComponent1.Text = "monogameEditorComponent1";
            // 
            // logTextBox
            // 
            this.logTextBox.Font = new System.Drawing.Font("Cascadia Code", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new System.Drawing.Point(13, 396);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(378, 179);
            this.logTextBox.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1344, 629);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.monogameEditorComponent1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MonogameEditorComponent monogameEditorComponent1;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.TextBox logTextBox;
    }
}

