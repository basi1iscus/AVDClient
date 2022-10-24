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

namespace AVDClient
{
    public partial class GoodsImage : Form
    {
        public int imgCount = 0;
        public WebStore.WebStore cService;
        public string GoodsId;
        public WebStore.Customers customer;
        public int index = 0;
        public GoodsImage()
        {
            InitializeComponent();
        }

        private void goodImage_Click(object sender, EventArgs e)
        {

        }

        private void showImage()
        {
            byte[] Data;
            try
            {
               imgCount = cService.GetImage(customer, GoodsId, index, out Data); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }


                 MemoryStream MS = new MemoryStream(Data);
                BinaryWriter STG = new BinaryWriter(MS);
                STG.Write(Data);

                Bitmap Image = new Bitmap(MS);
                this.goodImage.Image = (Image)Image;
 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (index == 0) 
                index=imgCount-1;
            else
                index--;
            
            showImage();
         }

        private void button2_Click(object sender, EventArgs e)
        {
            if (index == imgCount-1) 
                index=0;
            else
                index++;
            
            showImage();
        
        }

        private void GoodsImage_Shown(object sender, EventArgs e)
        {
            if (imgCount <= 1)
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void GoodsImage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                this.Close();
            }

        }
    }
}
