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
    public partial class SendMessage : Form
    {
        public MainForm baseForm;
        string destination = "Менеджер";
        public SendMessage(MainForm baseForm, string destination)
        {
            InitializeComponent();
            this.baseForm = baseForm;
            this.destination = destination;
        }

         private void button2_Click(object sender, EventArgs e)
        {
            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();

            baseForm.AuthorizeService(cService, customer);

            FormWaiting f_FormWaiting = new FormWaiting();
            f_FormWaiting.labelDescription.Text = "Выполняется отправка сообщения на сервер";
            f_FormWaiting.Show(this);
            f_FormWaiting.Update();

            string _s_ = "";
            try
            {
                _s_ = cService.SendMessage(customer, textBoxTopic.Text, textBoxMessage.Text, destination);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (_s_ != "")
            {
                MessageBox.Show(this, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            f_FormWaiting.Close();
            this.Close();
        }

         private void button1_Click(object sender, EventArgs e)
         {
             this.Close();
         }

         private void button3_Click(object sender, EventArgs e)
         {
             string topic = "SendMessage.html";
             HelpNavigator navigator = HelpNavigator.Topic;
             Help.ShowHelp(this, "AVDClient.chm", navigator, topic);
         }
    }
}
