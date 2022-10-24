using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;


namespace AVDClient
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
          //try {
          //    WebClient client = new WebClient();
          //    Stream stream = client.OpenRead("http://www.ip-ping.ru/");
          //    StreamReader sr = new StreamReader(stream);
          //    string newLine;
          //    MatchCollection matches;
          //    Regex regex = new Regex("<div class=\"hc2\">(.*)</div>");
          //    while ((newLine = sr.ReadLine()) != null)
          //    {
          //        Match match = regex.Match(newLine);
          //        string str = match.Groups[1].ToString();
          //        if (str != "") { label4.Text = str; }
          //    }

          //    stream.Close();      
          //  WebClient webClient = new WebClient();
          //  Stream stream2 = webClient.OpenRead("http://freegeoip.net/xml/" + label4.Text);
          //  StreamReader sReader = new StreamReader(stream2);
          //  string content = sReader.ReadToEnd();
          //  XmlReadMode xml = new XmlReadMode();
          //  xml.
          //  label5.Text = content;
          ////тут запись данных в куки
          //} catch (Exception ex) {
            
          //}
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
