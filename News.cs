using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;

namespace AVDClient
{
    public partial class News : Form
    {
        public MainForm baseForm;
        string iD = "";

        public News(MainForm baseForm, string iD)
        {
            InitializeComponent();
            this.baseForm = baseForm;
            this.iD = iD;
        }

          private void button1_Click(object sender, EventArgs e)
         {
             this.Close();
         }

          private void News_Shown(object sender, EventArgs e)
          {
              
              SqlCeCommand com_sel = new SqlCeCommand("SELECT * FROM News WHERE ID= @ID", baseForm.ConnCe);
              com_sel.Parameters.AddWithValue("@ID", iD);

              SqlCeDataReader reader = com_sel.ExecuteReader();
              reader.Read();
              string documentText = reader["html"].ToString();
              textBoxNewsTopic.Text = reader["Info"].ToString();
              textBoxDateNews.Text = ((DateTime)reader["Date"]).ToString("D");

              for (int i = 1; i <= (int)reader["ImgsCount"]; i++)
              {
                  string fileName = System.IO.Path.GetTempFileName();
                  documentText = documentText.Replace(String.Concat(@"image00", i.ToString()), String.Concat(@"file:\\", fileName));
                  FileStream fs = new FileStream(fileName, FileMode.Create);
                  BinaryWriter w = new BinaryWriter(fs);
                  w.Write((byte[])reader[String.Concat("image00", i.ToString())]);
               }

              webBrowserNews.DocumentText = documentText;

          }
    }
}
