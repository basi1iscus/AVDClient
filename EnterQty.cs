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

namespace AVDClient
{
    public partial class FormEnterQty : Form

    {
        public double basketQty = -1.0;
        public FormEnterQty()
        {
                    
            InitializeComponent();

        }

        private void EnterQvo_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Ok_Click();
        }
        
        private void Ok_Click()
        {
            //запишем данные в корзину

            //string Id = dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoID"].Index].Value.ToString();

            SqlCeCommand com = new SqlCeCommand();
            com.Connection = Data.ConnCe;

            com.CommandText = "SELECT SUM (Qty) AS Qty FROM basket WHERE Id=@Id";
            com.Parameters.AddWithValue("@Id", Data.Id);
            var result = com.ExecuteScalar();
            double Qty = 0.0;
            if (!DBNull.Value.Equals(result))
            {
                Qty = Convert.ToDouble(result);
                com.CommandText = "UPDATE basket SET Price = @Price, Total = @Total, Qty = @Qty WHERE Id=@Id";
            }
            else
                com.CommandText = "INSERT INTO basket (Id, Qty, Price, Total) VALUES(@Id, @Qty, @Price, @Total)";

            com.Parameters.AddWithValue("@Qty", Convert.ToDouble(textBoxQty.Text) + Qty);
            com.Parameters.AddWithValue("@Price", Convert.ToDouble(textBoxPrice.Text));
            com.Parameters.AddWithValue("@Total", (Convert.ToDouble(textBoxQty.Text)+Qty) * Convert.ToDouble(textBoxPrice.Text));

            com.ExecuteNonQuery();
            
            basketQty = Convert.ToDouble(textBoxQty.Text) + Qty;

            this.Close();
        }

        private void textBoxPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxQty_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!(Char.IsDigit(e.KeyChar)) && !((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf(".") == -1) && (((TextBox)sender).Text.Length != 0))) 
            { 
                if (e.KeyChar != (char)Keys.Back) 
                { 
                    e.Handled = true; 
                } 
            }
 
        }

        private void textBoxQty_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Ok_Click();
            }

        }
    }
}
