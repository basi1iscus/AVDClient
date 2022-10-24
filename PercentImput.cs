using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AVDClient
{
    public partial class FormMarkUp : Form
    {
        public FormMarkUp()
        {
              InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            MainForm main = this.Owner as MainForm;
            if (main != null)
            {
                textBoxMarkUp.Text = main.textBoxMarkUp.Text;                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //main.textBoxMarkUp.Text = textBoxMarkUp.Text;
            MainForm main = this.Owner as MainForm;
            if (main != null)
            {
                main.textBoxMarkUp.Text = textBoxMarkUp.Text;
            }
            this.Close();
        }

        private void textBoxMarkUp_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
