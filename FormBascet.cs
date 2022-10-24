using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Net;

namespace AVDClient
{
    public partial class FormBasket : Form
    {
        public FormBasket()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "myDatabaseDataSet.basket". При необходимости она может быть перемещена или удалена.
            basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
           // basketTableAdapter.DataBind();
          
        }

        private void dataGridViewBascet_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewBascet.Columns["QtyGridViewBascet"].Index)
            {
                double Price = Convert.ToDouble(dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["PriceGridViewBascet"].Index].Value);
                double Qty = Convert.ToDouble(dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["QtyGridViewBascet"].Index].Value);
                dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["TotalGridViewBascet"].Index].Value = Price * Qty;
               // dataGridViewBascet.Refresh();
                
                
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
          DialogResult res = MessageBox.Show("Очистить корзину?", "YesNo", MessageBoxButtons.YesNo);

          if (res ==  DialogResult.Yes)
            {
     

            //dataGridViewBascet.Rows.Clear();           
            while (dataGridViewBascet.Rows.Count > 0) 
            {
                for (int i = 0; i < dataGridViewBascet.Rows.Count ; i++)

                    dataGridViewBascet.Rows.RemoveAt(0);
                   
 
            }
          }
        }

        private void SaveBasketChange()
        {

     
            SqlCeCommand command_upd = new SqlCeCommand("UPDATE basket SET ID = @ID, Qty = @Qty, Total=@Total WHERE (ID = @ID)");
            SqlCeCommand command_del = new SqlCeCommand("DELETE FROM basket  WHERE (ID = @ID)");
            command_upd.Connection = basketTableAdapter.Connection;
            command_del.Connection = basketTableAdapter.Connection;
            basketTableAdapter.Adapter.UpdateCommand = command_upd;
            basketTableAdapter.Adapter.DeleteCommand = command_del;

            SqlCeParameter parametr; SqlCeParameter parametr_del;

            //Параметр ID
            parametr = new SqlCeParameter("@ID", SqlDbType.NVarChar);
            parametr.SourceColumn = "ID";
            command_upd.Parameters.Add(parametr);

            parametr_del = new SqlCeParameter("@ID", SqlDbType.NVarChar);
            parametr_del.SourceColumn = "ID";
            command_del.Parameters.Add(parametr_del);
            
            //Параметр Price
            parametr = new SqlCeParameter("@Qty", SqlDbType.Int);
            parametr.SourceColumn = "Qty";
            command_upd.Parameters.Add(parametr);           

            //Параметр Price
            parametr = new SqlCeParameter("@Total", SqlDbType.Float);
            parametr.SourceColumn = "Total";
            command_upd.Parameters.Add(parametr);

            basketTableAdapter.Adapter.Update(myDatabaseDataSet.basket);
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
          DialogResult res = MessageBox.Show("Сохранить изменения в корзине?", "YesNo", MessageBoxButtons.YesNo);

          if (res ==  DialogResult.Yes)
            {     
                SaveBasketChange();
               
            }
          Close();
          }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveBasketChange();
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string pass = "";
            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Order order = new WebStore.Order();
            WebStore.Customers customer = new WebStore.Customers();

            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant",  Data.ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
            {
                customer.Login = Convert.ToString(reader["Login"]);
                pass = Convert.ToString(reader["Password"]);
                customer.Code = Convert.ToString(reader["CustomerCode"]);
                order.Customer = customer;
                order.OrderNumber = "";
            }
            else
                return;

            com_ = new SqlCeCommand("SELECT COUNT(1) AS RecordCnt FROM basket", Data.ConnCe);
            reader = com_.ExecuteReader();
            if (reader.Read())
                order.Table = new WebStore.Table[Convert.ToInt32(reader[0])];
            else
                return;

            com_ = new SqlCeCommand("SELECT * FROM basket", Data.ConnCe);
            reader = com_.ExecuteReader();
            int i = 0;
            while (reader.Read())
            {
                order.Table[i] = new WebStore.Table();
                order.Table[i].Code = Convert.ToString(reader["ID"]);
                order.Table[i].Price = Convert.ToSingle(reader["Price"]);
                order.Table[i].Quantity = Convert.ToSingle(reader["Qty"]);
                i++;
            }

            ICredentials credentials = new NetworkCredential(customer.Login, pass);
            cService.Credentials = credentials;
            string s = cService.Test();
            cService.FormOrder(order);
            
            while (dataGridViewBascet.Rows.Count > 0)
            {
                for ( i = 0; i < dataGridViewBascet.Rows.Count; i++)

                    dataGridViewBascet.Rows.RemoveAt(0);


            }

        }

        private void dataGridViewBascet_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

      
    }
}
