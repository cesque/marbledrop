using MarbleDrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarbleDropEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            monogameEditorComponent1.Form = this;
        }

        public void Log(string text)
        {
            logTextBox.Text += text + Environment.NewLine;
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.SelectionLength = 0;
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(_ =>
            {
                using (var game = new Game1())
                {
                    game.Run();
                }
            });

            thread.Start();
        }
    }
}
