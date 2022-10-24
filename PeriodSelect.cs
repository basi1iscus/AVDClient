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
    public partial class FormEnterDate : Form
    {
        public FormEnterDate()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {   
            Data.FormAktSverki = true;
            Data.DateFor = dateTimePickerFor.Value;
            Data.DateFrom = dateTimePickerFrom.Value;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Data.FormAktSverki = false;
            Close();
        }

        private void FormEnterDate_Load(object sender, EventArgs e)
        {

        }
    }
}
