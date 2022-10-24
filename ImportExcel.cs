using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace AVDClient
{
    public partial class ImportExcel : Form
    {
        string fileName;
        MainForm baseForm;
        
        public ImportExcel(MainForm baseForm, string fileName)
        {
            this.baseForm = baseForm;
            this.fileName = fileName;

            InitializeComponent();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string Id="";
            double Price = 0.0;
            string article;
            double qty;
            SqlCeDataReader reader;

            if (comboBoxFirstRow.Text.Trim() == "" || comboBoxQty.Text == "")
                return;

            SqlCeCommand com = new SqlCeCommand();
            com.Parameters.AddWithValue("@Id", "");
            com.Parameters.AddWithValue("@Qty", 0.0);
            com.Parameters.AddWithValue("@Price", 0.0);
            com.Parameters.AddWithValue("@Total", 0.0);

            com.Connection = Data.ConnCe;

            for (int i = Convert.ToInt16(comboBoxFirstRow.Text); i <= dataGridViewImportExcel.RowCount; i++)
            {
                article = dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxArticle.Text].Value.ToString();
                if (article.Trim().Equals(""))
                    continue;
                try
                {
                    qty = Convert.ToDouble(dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxQty.Text].Value);
                }
                catch (FormatException)
                {
                    continue;
                }


                if (qty == 0.0)
                    continue;

                //com.CommandText = "SELECT Id, Price FROM Goods WHERE Article4search LIKE '%" + baseForm.DeleteSpecialChars(article) + "%'";
                //reader = com.ExecuteReader();
                //if (reader.Read())
                Id = dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelID"].Value.ToString().Trim();
                if (Id != "")
                {
                    Price = Convert.ToDouble(dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelPrice"].Value);

                    com.CommandText = "SELECT SUM (Qty) AS Qty FROM basket WHERE Id=@Id";
                    com.Parameters["@Id"].Value = Id;
                    var result = com.ExecuteScalar();
                    double QtyInBasket = 0.0;
                    if (!DBNull.Value.Equals(result))
                    {
                        QtyInBasket = Convert.ToDouble(result);
                        com.CommandText = "UPDATE basket SET Price = @Price, Total = @Total, Qty = @Qty WHERE Id=@Id";
                    }
                    else
                        com.CommandText = "INSERT INTO basket (Id, Qty, Price, Total) VALUES(@Id, @Qty, @Price, @Total)";

                    com.Parameters["@Qty"].Value = qty + QtyInBasket;
                    com.Parameters["@Price"].Value = Price;
                    com.Parameters["@Total"].Value = (qty + QtyInBasket) * Price;

                    com.ExecuteNonQuery();
                }

            }

            this.Close();
            baseForm.basketUpdate();

        }

        private void ImportExcel_Load(object sender, EventArgs e)
        {
            DataTable tb = new DataTable();
            if (fileName == "")
                return;
            //string ConStr = String.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; Extended Properties=Excel 8.0;", fileName);
            //System.Data.DataSet ds = new System.Data.DataSet("EXCEL");
            //OleDbConnection cn = new OleDbConnection(ConStr);
            //cn.Open();
            //DataTable schemaTable = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            //string sheet1 = (string)schemaTable.Rows[0].ItemArray[2];
            //string select = String.Format("SELECT * FROM [{0}]", sheet1);
            //OleDbDataAdapter ad = new OleDbDataAdapter(select, cn);
            //ad.Fill(ds);
            //tb = ds.Tables[0];
            //cn.Close();
            Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook ExcelWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet ExcelWorkSheet;

            ExcelWorkBook = ExcelApp.Workbooks.Open(fileName, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false,
                false, 0, true, 1, 0);
            ExcelWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelWorkBook.Worksheets.get_Item(1);

            Microsoft.Office.Interop.Excel.Range ActiveCell = ExcelApp.ActiveCell.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell);
            int rowCount = ActiveCell.Row;
            int columnCount = ActiveCell.Column;

            dataGridViewImportExcel.Columns.Add("ImportExcelNum", "#");
            dataGridViewImportExcel.Columns["ImportExcelNum"].ReadOnly = true;
            dataGridViewImportExcel.Columns["ImportExcelNum"].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);

            string comumnName = "A";
            for (int i = 0; i < columnCount; i++)
            {
                dataGridViewImportExcel.Columns.Add(((char)((int)comumnName[0] + i)).ToString(), ((char)((int)comumnName[0] + i)).ToString());
                comboBoxQty.Items.Add(((char)((int)comumnName[0] + i)).ToString());
                comboBoxArticle.Items.Add(((char)((int)comumnName[0] + i)).ToString());
            }

            dataGridViewImportExcel.Columns.Add("ImportExcelID", "ID");
            dataGridViewImportExcel.Columns["ImportExcelID"].Visible = false;
//            DataGridViewComboBoxColumn colComboBox = new DataGridViewComboBoxColumn();
//            colComboBox.Name = "ImportExcelName";
//            colComboBox.HeaderText = "Наименование";
//            dataGridViewImportExcel.Columns.Add(colComboBox);
            dataGridViewImportExcel.Columns.Add("ImportExcelArticle", "Кат.номер");
            dataGridViewImportExcel.Columns["ImportExcelArticle"].ReadOnly = true;
            dataGridViewImportExcel.Columns["ImportExcelArticle"].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
            dataGridViewImportExcel.Columns.Add("ImportExcelName", "Наименование");
            dataGridViewImportExcel.Columns["ImportExcelName"].ReadOnly = true;
            dataGridViewImportExcel.Columns["ImportExcelName"].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
            dataGridViewImportExcel.Columns.Add("ImportExcelManufac", "Производитель");
            dataGridViewImportExcel.Columns["ImportExcelManufac"].ReadOnly = true;
            dataGridViewImportExcel.Columns["ImportExcelManufac"].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
            dataGridViewImportExcel.Columns.Add("ImportExcelPrice", "Цена");
            dataGridViewImportExcel.Columns["ImportExcelPrice"].ReadOnly = true;
            dataGridViewImportExcel.Columns["ImportExcelPrice"].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
            dataGridViewImportExcel.Columns["ImportExcelPrice"].DefaultCellStyle.Format = "N2";
           
            for (int i = 0; i < rowCount; i++)
            {
                comboBoxFirstRow.Items.Add((i+1).ToString());

                dataGridViewImportExcel.Rows.Add();
                dataGridViewImportExcel.Rows[i].Cells[0].Value = i + 1;
                for (int j = 0; j < columnCount; j++)
                    dataGridViewImportExcel.Rows[i].Cells[j+1].Value = ExcelApp.Cells[i+1, j+1].Text;
            }
 
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar)))
            {
                if (e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string Id = "";
            double Price = 0.0;
            string article;
            double qty;
            SqlCeDataReader reader;

            if (comboBoxFirstRow.Text.Trim() == "" || comboBoxArticle.Text == "")
                return;

            SqlCeCommand com = new SqlCeCommand();
            com.Parameters.AddWithValue("@Id", "");
            com.Parameters.AddWithValue("@Qty", 0.0);
            com.Parameters.AddWithValue("@Price", 0.0);
            com.Parameters.AddWithValue("@Total", 0.0);

            com.Connection = Data.ConnCe;

            for (int i = Convert.ToInt16(comboBoxFirstRow.Text); i <= dataGridViewImportExcel.RowCount; i++)
            {
                article = dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxArticle.Text].Value.ToString();
                if (article.Trim().Equals(""))
                    break;

                com.CommandText = "SELECT Id, Price, Name, Manufac, Article  FROM Goods WHERE Article4search='" + baseForm.DeleteSpecialChars(article) + "'";
//                com.CommandText = "SELECT Id, Price, Name, Manufac, Article  FROM Goods WHERE Article4search LIKE '%" + baseForm.DeleteSpecialChars(article) + "%'";
                reader = com.ExecuteReader();

                dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelID"].Value = "";
                dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelName"].Value = "";
                dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelArticle"].Value = "";
                dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelManufac"].Value = "";
                
                if (reader.Read())
                {
                    //dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelID"].
                    Id = Convert.ToString(reader["Id"]);
                    Price = Convert.ToDouble(reader["Price"]);
                    dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelID"].Value = Convert.ToString(reader["Id"]);
                    dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelName"].Value = Convert.ToString(reader["Name"]);
                    dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelArticle"].Value = Convert.ToString(reader["Article"]);
                    dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelManufac"].Value = Convert.ToString(reader["Manufac"]);
                    dataGridViewImportExcel.Rows[i - 1].Cells["ImportExcelPrice"].Value = Price;
                    if (reader.Read())
                        dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxArticle.Text].Style.BackColor = Color.FromArgb(252, 212, 4);
                    else
                        dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxArticle.Text].Style.BackColor = Color.FromArgb(183, 212, 68);
                }
                else
                    dataGridViewImportExcel.Rows[i - 1].Cells[comboBoxArticle.Text].Style.BackColor = Color.FromArgb(173, 137, 148);
                

            }
        }

        private void goodsSelect(string Id)
        {
            SqlCeCommand com = new SqlCeCommand();
            SqlCeDataReader reader;
            com.Connection = Data.ConnCe;
            com.Parameters.AddWithValue("@Id", Id);
            com.CommandText = "SELECT Id, Price, Name, Manufac, Article  FROM Goods WHERE Id=@Id";
            reader = com.ExecuteReader();
            if (reader.Read())
            {
                dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells["ImportExcelID"].Value = Convert.ToString(reader["Id"]);
                dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells["ImportExcelName"].Value = Convert.ToString(reader["Name"]);
                dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells["ImportExcelArticle"].Value = Convert.ToString(reader["Article"]);
                dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells["ImportExcelManufac"].Value = Convert.ToString(reader["Manufac"]);
                dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells["ImportExcelPrice"].Value = Convert.ToDouble(reader["Price"]);
            }
        }


        private void dataGridViewImportExcel_DoubleClick(object sender, EventArgs e)
        {
            if (comboBoxArticle.Text == "")
                return;
            baseForm.findArticle(dataGridViewImportExcel.Rows[dataGridViewImportExcel.CurrentRow.Index].Cells[comboBoxArticle.Text].Value.ToString(), this);
        }

        public void acticleSelected(string Id)
        {
            goodsSelect(Id);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string topic = "LoadExcel.html";
            HelpNavigator navigator = HelpNavigator.Topic;
            Help.ShowHelp(this, "AVDClient.chm", navigator, topic);
        }

    }
}
