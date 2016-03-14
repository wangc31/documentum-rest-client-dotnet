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

namespace Emc.Documentum.Rest.Test
{
    public partial class Logger : Form
    {
        public Logger()
        {
            InitializeComponent();

        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            
        }

        public TextBox getLoggerTextBox()
        {
            return txtLog;
        }

        public void appendText(string text)
        {
            txtLog.AppendText(text);
        }

        private void Logger_Load(object sender, EventArgs e)
        {

        }

        public void setTitle(string text)
        {
            this.Text = text;
        }



    }
}
