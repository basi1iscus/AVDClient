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
using System.Web.Services;
using System.Net;
using System.IO;
using Ionic.Zip;
using Microsoft.Office.Interop;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Management;
using System.Security.Cryptography;
using System.Deployment.Application;

namespace AVDClient
{

    public partial class MainForm : Form
    {

        string version = "1.1.16";

        public SqlCeConnection ConnCe;
        // DataTable DataTabAuto;
        DataTable DataTabOriginal = new DataTable("Original");
        DataTable DataTabAnalogs = new DataTable("Analogs");
        float Currency = 1;
        float Currency_USD = 1;
        float Currency_EUR = 1;
        float Currency_Course_Klient = 1;

        string Currency_Klient = "EUR";
        string current_Currency = "EUR";
        bool databaseOpen = false;
        string catalogState = "Купить";
        ImportExcel activeForm; 

        public MainForm()
        {

            InitializeComponent();
            if (!(databaseOpen = OpenDataBase()))
                return;
           //ApdateDataBase();
            //OpenDataBase();
            FormTreeCatalog();
            settext_labelbasket();
            SetCurrency();
            setNameMenuChangeMark();
        }

        string GetUserName()
        {
            return Environment.UserName;
        }

        string GetComputerName()
        {
            return Environment.MachineName;
        }

        string VolumeSerialNumber()
        {
            try
            {
                string drive = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 1);
                ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"" + drive + ":\"");
                disk.Get();
                return (disk["VolumeSerialNumber"].ToString());

            }
            catch (Exception)
            {
                return "";
            }
        }

        string GetHashString(string s)
        {
            //переводим строку в байт-массим  
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            //создаем объект для получения средст шифрования  
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();

            //вычисляем хеш-представление в байтах  
            byte[] byteHash = CSP.ComputeHash(bytes);

            string hash = string.Empty;

            //формируем одну цельную строку из массива  
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);

            return hash;
        }

        public string GetRegistrationKey()
        {
            string A = "";
            A = A + GetComputerName();
            string B = VolumeSerialNumber();
            if (B.Equals(""))
                return "";
            else
                return GetHashString(A + B);

        }

        public string GetExternalIP()
        {
             try
            {
//                WebClient client = new WebClient();
 //               Stream data = client.OpenRead("http://whatismyip.org/");
 //               StreamReader reader = new StreamReader(data);
   //             return reader.ReadLine();

                    String direction = "";
                    WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                    using (WebResponse response = request.GetResponse())
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        direction = stream.ReadToEnd();
                    }

                    //Search for the ip in the html
                    int first = direction.IndexOf("Address: ") + 9;
                    int last = direction.LastIndexOf("</body>");
                    direction = direction.Substring(first, last - first);
            
                    return direction;


            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private bool OpenDataBase()
        {
            string dataDirectory = "";
            try
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                dataDirectory = ad.DataDirectory + "\\";
            }
            catch (Exception ex)
            {
            }

            string pass = GetHashString(GetComputerName() + VolumeSerialNumber());
            if (File.Exists(dataDirectory+"dt.sdf"))
            {
                FileInfo file = new System.IO.FileInfo(dataDirectory + "dt.sdf");
                long size = file.Length;

                SqlConnectionStringBuilder strConn = new SqlConnectionStringBuilder();
                strConn["Data Source"] = "|DataDirectory|\\dt.sdf";
                strConn["Password"] = pass;
                ConnCe = new SqlCeConnection(strConn.ConnectionString);
                try
                {
                    ConnCe.Open();
                }
                catch (Exception ex)
                {
                    if (size < 250000)
                    {
                        SqlCeEngine engine = new SqlCeEngine("Data Source = |DataDirectory|\\dt.sdf");

                        // Specify null destination connection string for in-place compaction
                        //
                        try
                        {
                            engine.Compact(null);
                        }
                        catch (Exception ex1)
                        {
                            MessageBox.Show(this, "Вы работаете с незарегистрированной версией AVDClient или параметры оборудования изменились. Переустановите программу и зарегистрируйте ее снова." + ex1.Message, "Ошибка", MessageBoxButtons.OK);
                            Application.Exit();
                            return false;
                        }

                        // Specify connection string for new database options
                        //
                        engine.Compact("Data Source=; Password =" + pass);
                        strConn = new SqlConnectionStringBuilder();
                        strConn["Data Source"] = "|DataDirectory|\\dt.sdf";
                        strConn["Password"] = pass;
                        ConnCe = new SqlCeConnection(strConn.ConnectionString);
                        try
                        {
                            ConnCe.Open();
                        }
                        catch (Exception ex1)
                        {
                            MessageBox.Show(this, ex1.Message, "Ошибка", MessageBoxButtons.OK);
                            Application.Exit();
                            return false;
                        }
                   }                    
                    else 
                    {
                        MessageBox.Show(this, "Вы работаете с незарегистрированной версией AVDClient или параметры оборудования изменились. Переустановите программу и зарегистрируйте ее снова." + ex.Message, "Ошибка", MessageBoxButtons.OK);               
                        Application.Exit();
                        return false;
                    }
                }
            }
            else
            {
                MessageBox.Show(this, "Ошибка. Не найдена база данных. Работа невозможна", "Ошибка", MessageBoxButtons.OK);
                Application.Exit();
                return false;
            }

            Data.ConnCe = ConnCe;
            return true;

        }
    
        private void UpdateDataBase(ZipFile zip, string pass)
        {
            string line;
            SqlCeCommand com = new SqlCeCommand();
            com.Connection = ConnCe;
            com.CommandText = "DELETE FROM Groups";
            com.ExecuteNonQuery();
            com.CommandText = "DELETE FROM Goods";
            com.ExecuteNonQuery();
            com.CommandText = "DELETE FROM original";
            com.ExecuteNonQuery();
            com.CommandText = "DELETE FROM analogs";
            com.ExecuteNonQuery();
            com.CommandText = "DELETE FROM CrossArticle";
            com.ExecuteNonQuery();

            ZipEntry entry = zip["groups.txt"];
            entry.Password = pass;
            MemoryStream fileMS = new MemoryStream();

            entry.Extract(fileMS);
            fileMS.Position = 0;
            StreamReader file = new StreamReader(fileMS);

          com.CommandText = "INSERT INTO Groups (Id, Name, ParentID) VALUES(@Id, @Name, @ParenID)";
          com.Parameters.AddWithValue("@Id", "");
          com.Parameters.AddWithValue("@Name", "");
          com.Parameters.AddWithValue("@ParenID", "");
//          System.IO.StreamReader file =
//          new System.IO.StreamReader(@"D:\Base\Temp\groups.txt");
                while ((line = file.ReadLine()) != null)
                {
                    string [] split = line.Split('~');
                    com.Parameters["@Id"].Value = split[0];
                    com.Parameters["@Name"].Value = split[2];
                    com.Parameters["@ParenID"].Value = split[3];
                    com.ExecuteNonQuery();
                }
               file.Close();

            com.CommandText = "INSERT INTO Goods (Id, Name, Article, ParentID, Manufac, Price, Rest, Rest2, Expected, FilterNew, FilterAction, FilterReceipt, QtyPhoto, Article4search, DateReceipt) VALUES(@Id, @Name, @Article, @ParenID, @Manufac, @Price, @Rest, @Rest2, @Expected, @FilterNew,  @FilterAction,  @FilterReceipt,  @QtyPhoto, @Article4search, @DateReceipt)";
            com.Parameters.Clear();
            com.Parameters.AddWithValue("@Id", "");
            com.Parameters.AddWithValue("@Name", "");
            com.Parameters.AddWithValue("@Article", "");
            com.Parameters.AddWithValue("@Article4search", "");
            com.Parameters.AddWithValue("@ParenID", "");
            com.Parameters.AddWithValue("@Manufac", "");
            com.Parameters.AddWithValue("@Price", 0.00);
            com.Parameters.AddWithValue("@Rest", "");
            com.Parameters.AddWithValue("@Rest2", "");
            com.Parameters.AddWithValue("@Expected", "");
            com.Parameters.AddWithValue("@FilterNew", "");
            com.Parameters.AddWithValue("@FilterAction", "");
            com.Parameters.AddWithValue("@FilterReceipt", "");
            com.Parameters.AddWithValue("@QtyPhoto", "");
            com.Parameters.AddWithValue("@DateReceipt", "");

            fileMS.Close();
            fileMS.Dispose();
            entry = zip["goods.txt"];
            entry.Password = pass;
            fileMS = new MemoryStream();

            entry.Extract(fileMS);
            fileMS.Position = 0;
            file = new StreamReader(fileMS);
  
            toolStripStatusUpdateDatabase.Text = "Выполняется загрузка товаров в базу данных";
            toolStripStatusProgressUpdate.Visible = true;
            statusStrip1.Update();
            long maximum = fileMS.Length;
            long load = 0;

//            file = new System.IO.StreamReader(@"D:\Base\Temp\goods.txt");
                while ((line = file.ReadLine()) != null)
                {
                    load += (line.Length+2) * 2;
                    toolStripStatusProgressUpdate.Value = Convert.ToInt16( load * 100 / maximum);
 
                    string[] split = line.Split('~');
                    com.Parameters["@Id"].Value = split[0];
                    com.Parameters["@Name"].Value = split[3];
                    com.Parameters["@Article"].Value = split[2];
                    com.Parameters["@Article4search"].Value = split[13];
                    com.Parameters["@ParenID"].Value = split[8];
                    com.Parameters["@Manufac"].Value = split[4];
                    try
                    {
                        com.Parameters["@Price"].Value = Convert.ToSingle(split[9]);
                    }
                    catch (Exception e)
                    {
                        com.Parameters["@Price"].Value = 0.0;
                    }

                    com.Parameters["@Rest"].Value = split[10];
                    com.Parameters["@Rest2"].Value = split[11];
                    com.Parameters["@Expected"].Value = split[12];
                    com.Parameters["@FilterNew"].Value = split[15];
                    com.Parameters["@FilterAction"].Value = split[14];
                    com.Parameters["@FilterReceipt"].Value = split[16];
                    try
                    {
                        com.Parameters["@QtyPhoto"].Value = Convert.ToInt32(split[17]);
                    }
                    catch (Exception e)
                    {
                        com.Parameters["@QtyPhoto"].Value = 0;
                    }
                    try { 
                        com.Parameters["@DateReceipt"].Value = Convert.ToDateTime(split[18]);
                      }   
                      catch (FormatException) {
                         com.Parameters["@DateReceipt"].Value = new DateTime(2000,1,1);
                      }

                    com.ExecuteNonQuery();
                }

               file.Close();

               com.CommandText = "INSERT INTO original (Id, article, article4search, brand) VALUES(@Id, @article, @article4search, @brand)";

               com.Parameters.Clear();
               com.Parameters.AddWithValue("@Id", "");
               com.Parameters.AddWithValue("@article", "");
               com.Parameters.AddWithValue("@article4search", "");
               com.Parameters.AddWithValue("@brand", "");

               fileMS.Close();
               fileMS.Dispose();
               entry = zip["original.txt"];
               entry.Password = pass;
               fileMS = new MemoryStream();

               entry.Extract(fileMS);
               fileMS.Position = 0;
               file = new StreamReader(fileMS);

               toolStripStatusUpdateDatabase.Text = "Выполняется загрузка оригинальных номеров в базу данных";
               statusStrip1.Update();
               maximum = fileMS.Length;
               load = 0;
               
               //file = new System.IO.StreamReader(@"D:\Base\Temp\original.txt");
               while ((line = file.ReadLine()) != null)
                {
                    load += line.Length * 2;
                    toolStripStatusProgressUpdate.Value = Convert.ToInt16(load * 100 / maximum);

                    string[] split = line.Split('~');
                    com.Parameters["@Id"].Value = split[0];
                    com.Parameters["@article"].Value = split[1];
                    com.Parameters["@article4search"].Value = split[2];
                    com.Parameters["@brand"].Value = split[3];
                    com.ExecuteNonQuery();
                }

               file.Close();

               com.CommandText = "INSERT INTO analogs (Id, groupid) VALUES(@Id, @groupid)";
               com.Parameters.Clear();
               com.Parameters.AddWithValue("@Id", "");
               com.Parameters.AddWithValue("@groupid", "");

               fileMS.Close();
               fileMS.Dispose();
               entry = zip["analogs.txt"];
               entry.Password = pass;
               fileMS = new MemoryStream();

               entry.Extract(fileMS);
               fileMS.Position = 0;
               file = new StreamReader(fileMS);

               toolStripStatusUpdateDatabase.Text = "Выполняется загрузка аналогов в базу данных";
               statusStrip1.Update();
               maximum = fileMS.Length;
               load = 0;

               //file = new System.IO.StreamReader(@"D:\Base\Temp\analogs.txt");
                while ((line = file.ReadLine()) != null)
                {
                    load += line.Length * 2;
                    toolStripStatusProgressUpdate.Value = Convert.ToInt16(load * 100 / maximum);

                    string[] split = line.Split('~');
                    com.Parameters["@Id"].Value = split[0];
                    com.Parameters["@groupid"].Value = split[1];
                    com.ExecuteNonQuery();
                }

                file.Close();

                com.CommandText = "INSERT INTO CrossArticle (id, article, article4search) VALUES(@Id, @article, @article4search)";
                com.Parameters.Clear();
                com.Parameters.AddWithValue("@Id", "");
                com.Parameters.AddWithValue("@article", "");
                com.Parameters.AddWithValue("@article4search", "");

                fileMS.Close();
                fileMS.Dispose();
                entry = zip["cross.txt"];
                entry.Password = pass;
                fileMS = new MemoryStream();

                entry.Extract(fileMS);
                fileMS.Position = 0;
                file = new StreamReader(fileMS);
                //file = new System.IO.StreamReader(@"D:\Base\Temp\cross.txt");

                toolStripStatusUpdateDatabase.Text = "Выполняется загрузка кросс-номеров в базу данных";
                statusStrip1.Update();
                maximum = fileMS.Length;
                load = 0;

                while ((line = file.ReadLine()) != null)
                {
                    load += line.Length * 2;
                    toolStripStatusProgressUpdate.Value = Convert.ToInt16(load * 100 / maximum);

                    string[] split = line.Split('~');
                    com.Parameters["@Id"].Value = split[0];
                    com.Parameters["@article"].Value = split[1];
                    com.Parameters["@article4search"].Value = split[2];
                    com.ExecuteNonQuery();
                }
                file.Close();
                fileMS.Close();
                fileMS.Dispose();
                toolStripStatusProgressUpdate.Value = 100;

                SqlCeCommand com_upd = new SqlCeCommand("UPDATE constant SET UpdateCatalogDate=@UpdateDate", Data.ConnCe);

                DateTime now = DateTime.Now;
                com_upd.Parameters.AddWithValue("@UpdateDate", now);

                com_upd.ExecuteNonQuery();

            toolStripStatusProgressUpdate.Visible = false;
            toolStripStatusUpdateDatabase.Visible = false;
            toolStripStatusUpdateData.Text = "Каталог товаров обновлен " + now.ToString();
            FormTreeCatalog();     
            //ConnCe.Close();
                                         
        }

        private void SetCurrency()
        { 
            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM Currency", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            while (reader.Read())
            {
                Currency_EUR = Convert.ToSingle(reader["Kurs_EUR"]);
                Currency_USD = Convert.ToSingle(reader["Kurs_USD"]);
                
            }

           }

        private void FormTreeCatalog()
        {

            SqlCeCommand com_del = new SqlCeCommand("DELETE FROM News WHERE Date<@Date", ConnCe);
            com_del.Parameters.Add("Date", DateTime.Now.AddDays(-30));

            com_del.ExecuteNonQuery();

            treeViewGroups.Nodes.Clear();
            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM Groups WHERE ParentID = @ParentID", ConnCe);
            com_.Parameters.Add("ParentID", "");
            SqlCeDataReader reader = com_.ExecuteReader();
            int i = 1;
            while (reader.Read())
            {
                TreeNode node = new TreeNode("Node" + i);
                node.Name = reader.GetString(0);
                node.Text = reader.GetString(1);
                string ParentID = reader.GetString(0);
                i++;
                this.treeViewGroups.Nodes.Add(node);
                FillTreeViewGroups(node, ParentID, ConnCe);
            }
        }

        void FillTreeViewGroups(TreeNode parentNode, string ParentID, SqlCeConnection ConnCe)
        {
            SqlCeCommand com1 = new SqlCeCommand("SELECT * FROM Groups WHERE ParentID = @ParentID", ConnCe);
            com1.Parameters.Add("ParentID", ParentID);
            SqlCeDataReader reader2 = com1.ExecuteReader();
            
            int j = 1;
            while (reader2.Read())
            {
                //parentNode.ImageIndex = 1;
                parentNode.BackColor = Color.FromArgb(252, 212, 4);
                TreeNode node = new TreeNode("Node" + j);
                node.Text = reader2.GetString(1);
                node.Name = reader2.GetString(0);
                node.ImageIndex = 2;
                node.BackColor = Color.FromArgb(255, 249, 211);
                parentNode.Nodes.Add(node);
 
                string nodeParentID = reader2.GetString(0);
                FillTreeViewGroups(node, nodeParentID, ConnCe);

            }
        }

        private void Set_Labels_Course()
        {
            labelUSD.Text = Convert.ToString(Currency_USD);
            labelEUR.Text = Convert.ToString(Currency_EUR);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
//            Form1_Shown(sender, e);
            if (!databaseOpen)
            {
                this.Close();
                Application.Exit();
                return;
            }

            if (!checkRegistrationStatus())
            {
                this.Close();
                Application.Exit();
                return;
            }

            this.ordersHTableAdapter.Fill(this.myDatabaseDataSet.OrdersH);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "myDatabaseDataSet.basket". При необходимости она может быть перемещена или удалена.
            this.basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
            newsTableAdapter.Fill(this.myDatabaseDataSet.News);

            set_Info();

            comboBoxCurrency.Text = Currency_Klient;

            Set_Labels_Course();
        }

        private bool firstStart()
        {
            FormLogin f_Login = new FormLogin();
            f_Login.firstStart = true;
            f_Login.baseForm = this;
            f_Login.ShowDialog(this);
            return f_Login.canClose;
        }

        private bool checkRegistrationStatus()
        {
            bool registrationStatus = true;

            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
            {
                if (Convert.ToString(reader["Login"]) == "" || Convert.ToDateTime(reader["UpdateDate"]).Equals(new DateTime(2000, 1, 1)))
                    registrationStatus = firstStart();
                if (registrationStatus)
                {
                    toolStripStatusUpdateData.Text = "Каталог товаров обновлен " + Convert.ToDateTime(reader["UpdateCatalogDate"]).ToString();
                    toolStripStatusCustomerUpdateDate.Text = "Данные партнера обновлены " + Convert.ToDateTime(reader["UpdateDate"]).ToString();

                    if (Convert.ToDateTime(reader["UpdateDate"]).Equals(new DateTime(2000, 1, 1)))
                        fullUpdate();
                }

            }
            else registrationStatus = false;

            return registrationStatus;
        }
    
        private void fullUpdate()
        {

            DialogResult res = MessageBox.Show(this, "Будет выполнено полное обновление базы данных. Продолжать?", "Обновление базы данных", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }

            UpdateCustomerInfo(false);
            UpdateCatalog();
        }

        private void set_Info()
        {
            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
            {
                Currency_Klient = Convert.ToString(reader["Currency"]);
                labelManagerName.Text = Convert.ToString(reader["ManagerName"]);
                labelManagerPhone.Text = "Телефон: " + Convert.ToString(reader["ManagerPhone"]);
                labelManagerEmail.Text = "E-mail: " + Convert.ToString(reader["ManagerEmail"]);
                labelManagerICQ.Text = "ICQ: " + Convert.ToString(reader["ManagerICQ"]);
                labelManagerSkipe.Text = "Skype: " + Convert.ToString(reader["ManagerSkipe"]);

                labelSummaCredit.Text = "Сумма кредита: " + Convert.ToString(reader["SummaCredit"]) + " " + Currency_Klient;
                labelDayCredit.Text = "Срок кредита: " + Convert.ToString(reader["DayCredit"]) + " дн.";
                labelDeliveryInfo.Text = "Cпособ доставки: " + Convert.ToString(reader["DeliveryInfo"]);
                
                labelFIO.Text = "Добрый день, " + Convert.ToString(reader["FIO"]);
            }

            SqlCeDataAdapter com_dolg = new SqlCeDataAdapter("SELECT *, CAST(@Currency_Klient AS  nchar(3)) AS Currency FROM OrdersDolg ORDER BY Date DESC", ConnCe);
            com_dolg.SelectCommand.Parameters.Add("@Currency_Klient", current_Currency);

            DataTable DataTabDolg = new DataTable("Dolg");

            com_dolg.Fill(DataTabDolg);

            dataGridViewDolg.DataSource = DataTabDolg;
            dataGridViewDolg.Columns["Date"].HeaderText         = "Дата";
            dataGridViewDolg.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            dataGridViewDolg.Columns["Date"].Width = 80;
            
            dataGridViewDolg.Columns["Number"].HeaderText       = "Номер";
            dataGridViewDolg.Columns["Number"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
       //    dataGridViewDolg.Columns["Number"].DefaultCellStyle.Format = "N2";
         
            dataGridViewDolg.Columns["PayDate"].HeaderText      = "Срок оплаты";
            dataGridViewDolg.Columns["PayDate"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
     
            dataGridViewDolg.Columns["Days"].HeaderText         = "Дней";
            dataGridViewDolg.Columns["Days"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;


            dataGridViewDolg.Columns["SummaDolg"].HeaderText    = "Долг";
            dataGridViewDolg.Columns["SummaDolg"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            dataGridViewDolg.Columns["SummaDolg"].DefaultCellStyle.Format = "N2";


            dataGridViewDolg.Columns["Prosrocheno"].HeaderText  = "Просрочено";
            dataGridViewDolg.Columns["Prosrocheno"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            dataGridViewDolg.Columns["Prosrocheno"].DefaultCellStyle.Format = "N2";

            
            dataGridViewDolg.Columns["Today"].HeaderText        = "Сегодня";
            dataGridViewDolg.Columns["Today"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            dataGridViewDolg.Columns["Today"].DefaultCellStyle.Format = "N2";

            dataGridViewDolg.Columns["Currency"].HeaderText     = "Валюта";
            dataGridViewDolg.Columns["Currency"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            
            Double sum = 0;

            for (int i = 0; i < dataGridViewDolg.Rows.Count; ++i)
            {

                sum += Convert.ToDouble(dataGridViewDolg.Rows[i].Cells["SummaDolg"].Value);

            }

            labelSummaDolg.Text = "Ваша задолженность по отгруженным товарам составляет: " + String.Format("{0:N2}",sum) + " " + Currency_Klient;
          
        }

        private void treeViewGroups_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //--------------------
            Delete_Filter();
            DataTabOriginal.Clear();
            DataTabAnalogs.Clear();
            SetFilterAuto();

            SqlCeDataAdapter com_cmb;
            if (treeViewGroups.SelectedNode.Nodes.Count == 0)
            {
                com_cmb = new SqlCeDataAdapter("SELECT distinct Manufac FROM Goods where ParentId=@ParentId", ConnCe);
                com_cmb.SelectCommand.Parameters.Add("@ParentId", treeViewGroups.SelectedNode.Name);
            }
            else
            {
                com_cmb = new SqlCeDataAdapter("SELECT distinct Manufac FROM Goods", ConnCe);
            }
            
            DataTable DataTabManufac = new DataTable("Manufac");
            com_cmb.Fill(DataTabManufac);

            DataRow allManufac = DataTabManufac.NewRow();
            allManufac["Manufac"] = "<все производители>";
            DataTabManufac.Rows.InsertAt(allManufac, 0);
            comboBoxManufac.DataSource = DataTabManufac;
            comboBoxManufac.DisplayMember = "Manufac";
            comboBoxManufac.ValueMember = "Manufac";
   
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void checkBoxManufac_CheckedChanged(object sender, EventArgs e)
        {
            //SetFilterAuto();
        }

        public string DeleteSpecialChars(string string4search)
        {

            string4search = string4search.Replace(" ", "");
            string4search = string4search.Replace(" ", "");
            string4search = string4search.Replace("-", "");
            string4search = string4search.Replace("/", "");
            string4search = string4search.Replace(".", "");
            return string4search;
        }

        private void SetFilterAuto()
        {
            string strcondition = "";

            //ParentId - если не выбран артикул и не нижний уровень


     //       if (treeViewGroups.SelectedNode == null)
    ///            return;

            if (!checkBoxArticle.Checked || textBoxArticle.Text.Trim().Length < 4)
            {
                if (treeViewGroups.SelectedNode != null)
                    if (treeViewGroups.SelectedNode.Nodes.Count == 0 && !checkBoxNew1.Checked && !checkBoxManufac.Checked && !checkBoxReceipt1.Checked && !checkBoxAction1.Checked)
                        strcondition = strcondition + " ParentId=@ParentId";
            }
            else
                strcondition = strcondition + //" Article Like '%" + textBoxArticle.Text.Trim() + "%'";

                    " (Goods.Id IN" +
                             " (SELECT        Id" +
                               " FROM            Goods AS Goods_1" +
                               " WHERE        (Article4search LIKE '%" + DeleteSpecialChars(textBoxArticle.Text) + "%')" +
                               " UNION" +
                               " SELECT        id" +
                               " FROM            original" +
                               " WHERE        (article4search LIKE '%" + DeleteSpecialChars(textBoxArticle.Text) + "%')" +
                               " UNION" +
                               " SELECT        id" +
                               " FROM            CrossArticle" +
                               " WHERE        (article4search LIKE '%" + DeleteSpecialChars(textBoxArticle.Text) + "%')))";
 
            if (checkBoxManufac.Checked)
                if (strcondition != "")
                    strcondition = strcondition + " and Manufac = @Manufac";
                else
                    strcondition = " Manufac = @Manufac";

            if (checkBoxNew.Checked)
                if (strcondition != "")
                    strcondition = strcondition + " and FilterNew = '1'";
                else
                    strcondition = strcondition + " FilterNew = '1'";

            if (checkBoxAction.Checked)
                if (strcondition != "")
                    strcondition = strcondition + " and FilterAction = '1'";
                else
                    strcondition = strcondition + " FilterAction = '1'";


            if (checkBoxReceipt.Checked)
                if (strcondition != "")
                    strcondition = strcondition + " and FilterReceipt = '1'";
                else 
                    strcondition = strcondition + " FilterReceipt = '1'";

            if (checkBoxName.Checked)
                if (strcondition != "")
                    strcondition = strcondition + " and Name Like '%" + textBoxName.Text.Trim() + "%'";
                else
                    strcondition = " Name Like '%" + textBoxName.Text.Trim() + "%'";

            DataTable DataTabAuto = new DataTable("Goods");

            if (strcondition == "")
                strcondition = " 1=2";

            SqlCeDataAdapter com = new SqlCeDataAdapter("SELECT Goods.Id," +
                                                                "ParentId,"+
                                                                "Article,"+
                                                                "Name,"+
                                                                "Manufac,"+
                                                                "'"+catalogState+"' AS Buy," +
                                                                "Rest,"+
                                                                "Rest2,"+
                                                                "Expected," +
                                                                "basket.Qty AS Ordered," +
                                                                "Goods.Price," +
                                                                "Goods.Price*@Currency AS ValPrice," +
                                                                "Goods.Price*@Currency*@MarkUp AS PriceMarkUp," +
                                                                "CAST(@Currency_Klient AS  nchar(3)) AS Currency,"+
                                                                "CASE WHEN QtyPhoto>0 THEN 'Фото' ELSE '' END AS Img," +
                                                                "QtyPhoto,"+
                                                                "FilterNew,"+
                                                                "FilterAction,"+
                                                                "FilterReceipt, "+
                                                                "DateReceipt " +
                                                                "FROM Goods LEFT OUTER JOIN basket " +
                                                                "ON Goods.Id = basket.Id "+
                                                                "where" + strcondition, ConnCe);

            if (checkBoxReceipt.Checked) 
                com.SelectCommand.CommandText = com.SelectCommand.CommandText + " union " +
                                                                "SELECT ''," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "'' AS Buy," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "NULL AS Ordered," +
                                                                "''," +
                                                                "NULL," +
                                                                "NULL," +
                                                                "''," +
                                                                "'' AS Img," +
                                                                "NULL," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "DateReceipt " +
                                                                "FROM Goods " +
                                                                "where " + strcondition + " ORDER BY DateReceipt DESC, Manufac, Article";
            if (checkBoxNew.Checked)
                com.SelectCommand.CommandText = com.SelectCommand.CommandText + " union " +
                                                                "SELECT ''," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "'' AS Buy," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "NULL AS Ordered," +
                                                                "''," +
                                                                "NULL," +
                                                                "NULL," +
                                                                "''," +
                                                                "'' AS Img," +
                                                                "NULL," +
                                                                "''," +
                                                                "''," +
                                                                "''," +
                                                                "DateReceipt " +
                                                                "FROM Goods " +
                                                                "where " + strcondition + " ORDER BY DateReceipt DESC, Manufac, Article";
            if (checkBoxAction.Checked)
                com.SelectCommand.CommandText = com.SelectCommand.CommandText + " ORDER BY Manufac, Article";

            if (treeViewGroups.SelectedNode != null)
            {
                com.SelectCommand.Parameters.Add("@ParentId", treeViewGroups.SelectedNode.Name);
            }

               
//                com.SelectCommand.Parameters.Add("@Manufac", comboBoxManufac.Text);
            if (checkBoxManufac.Checked)
                com.SelectCommand.Parameters.Add("@Manufac", ((System.Data.DataTable)((comboBoxManufac).DataSource)).Rows[comboBoxManufac.SelectedIndex][0]);

            com.SelectCommand.Parameters.Add("@Currency", Currency_Course_Klient);
            //com.SelectCommand.Parameters.Add("@State", catalogState);
                
                if (textBoxMarkUp.Text.ToString() == "")
                {
                    com.SelectCommand.Parameters.Add("@MarkUp", 1);
                }
                else com.SelectCommand.Parameters.Add("@MarkUp", 1 + Convert.ToDouble(textBoxMarkUp.Text) / 100);
                com.SelectCommand.Parameters.Add("@Currency_Klient", current_Currency);
                // com.SelectCommand.Parameters.Add("@Name",   textBoxName.Text.Trim());

                
                com.Fill(DataTabAuto);
            


            dataGridViewGoods.DataSource = DataTabAuto;

            //if (textBoxMarkUp.Text.ToString() == "")
            //{
            //    dataGridViewGoods.Columns["AvtoPriceMarkUp"].Visible = false;
            //}
            //else
            //    dataGridViewGoods.Columns["AvtoPriceMarkUp"].Visible = true;

            if (ShowPriceToolStripMenuItem.CheckState == CheckState.Unchecked)
            {
                dataGridViewGoods.Columns["AutoValPrice"].Visible = true;
            }
            else
            {
                dataGridViewGoods.Columns["AutoValPrice"].Visible = false;
            }
  

    }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //if (textBoxName.Text.Trim().Length < 4)
            //{
            //    checkBoxName.Checked = false;
            //}
            //else
            //{
            //    checkBoxName.Checked = true;
            //}
            //SetFilterAuto();
        }

        private void checkBoxName_CheckedChanged(object sender, EventArgs e)
        {
            //SetFilterAuto();
            if (checkBoxName.Checked && treeViewGroups.SelectedNode.Nodes.Count > 0 && !checkBoxManufac.Checked)
                checkBoxName.Checked = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //SetFilterAuto();

        }

        private void textBoxArticle_TextChanged(object sender, EventArgs e)
        {
            //if (textBoxArticle.Text.Trim().Length < 4)
            //{
            //    checkBoxArticle.Checked = false;
            //}
            //else
            //{
            //    checkBoxArticle.Checked = true;
            //}
            //SetFilterAuto();
        }

        private void dataGridViewGoods_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

            int AvtoID = dataGridViewGoods.Columns["AvtoID"].Index;
            string Id = dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoID"].Index].Value.ToString();

            DataTabOriginal.Clear();
            SqlCeDataAdapter com = new SqlCeDataAdapter("SELECT article, brand FROM original where Id=@Id", ConnCe);
            com.SelectCommand.Parameters.Add("@Id", Id);
            com.Fill(DataTabOriginal);
            dataGridViewOriginal.DataSource = DataTabOriginal;

            DataTabAnalogs.Clear();
            SqlCeCommand com1 = new SqlCeCommand("SELECT groupid FROM analogs WHERE id=@Id", ConnCe);
            com1.Parameters.Add("Id", Id);
            SqlCeDataReader reader = com1.ExecuteReader();

            if (reader.Read())
            {
                string groupid = reader.GetString(0);
                SqlCeDataAdapter com2 = new SqlCeDataAdapter("SELECT analogs.id,"+
                                                                    "Goods.Article,"+
                                                                    "Goods.Name,"+
                                                                    "Goods.Manufac,"+
                                                                    "'Купить' AS Buy,"+
                                                                    "Goods.Rest,"+
                                                                    "Goods.Rest2,"+
                                                                    "Goods.Expected,"+
                                                                    "basket.Qty AS Ordered,"+
                                                                    "Goods.Price,"+
                                                                    "Goods.Price*@Currency AS ValPrice,"+
                                                                    "Goods.Price*@Currency*@MarkUp AS PriceMarkUp," +
                                                                    "CAST(@current_Currency AS  nchar(3)) AS Currency," +
                                                                    "CASE WHEN QtyPhoto>0 THEN 'Фото' ELSE '' END AS Img," +
                                                                    "Goods.QtyPhoto" +
                          
                                                                    "   FROM analogs " +
                                                         "     LEFT OUTER JOIN Goods "+
                                                         "       ON analogs.id = Goods.Id" +
                                                         "     LEFT OUTER JOIN basket" +
                                                         "       ON analogs.id = basket.Id" +
                                                         "   WHERE analogs.groupid = @groupid AND analogs.id <> @id", ConnCe);
                com2.SelectCommand.Parameters.Add("@groupid", groupid);
                com2.SelectCommand.Parameters.Add("@id", Id);

                com2.SelectCommand.Parameters.Add("@Currency", Currency_Course_Klient);

                com2.SelectCommand.Parameters.Add("@current_Currency", current_Currency);
                if (textBoxMarkUp.Text.ToString() == "")
                {
                    com2.SelectCommand.Parameters.Add("@MarkUp", 1);
                }
                else com2.SelectCommand.Parameters.Add("@MarkUp", 1 + Convert.ToDouble(textBoxMarkUp.Text) / 100);

                com2.Fill(DataTabAnalogs);
                dataGridViewAnalogs.DataSource = DataTabAnalogs;
                
                if (ShowPriceToolStripMenuItem.CheckState == CheckState.Unchecked)
                {
                    dataGridViewAnalogs.Columns["AnalogValPrice"].Visible = true;
                }
                else
                {
                    dataGridViewAnalogs.Columns["AnalogValPrice"].Visible = false;
                }
            }
            this.Update();
        }

        private void showGoodsImage(string GoodsId, string GoodsName)
        {
            byte[] Data;

            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();

            AuthorizeService(cService, customer);
            int index = 0;
            int imgCount = 0;

            try
            {
               imgCount = cService.GetImage(customer, GoodsId, index, out Data); 
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
                        
            
            if (imgCount > 0)
            {
                MemoryStream MS = new MemoryStream(Data);
                BinaryWriter STG = new BinaryWriter(MS);
                STG.Write(Data);

                Bitmap Image = new Bitmap(MS);
                GoodsImage Form_GoodsImage = new GoodsImage();
                Form_GoodsImage.imgCount = imgCount;
                Form_GoodsImage.cService = cService;
                Form_GoodsImage.customer = customer;
                Form_GoodsImage.GoodsId = GoodsId;
                Form_GoodsImage.GoodsName.Text = GoodsName;
                Form_GoodsImage.goodImage.Image = (Image)Image;

                // Form_GoodsImage.textBoxPrice.Text = Convert.ToString(Convert.ToInt32(dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoPrice"].Index].Value) * Currency_Course_Klient);

                Form_GoodsImage.ShowDialog(this);
            }
        }

        private void BuyGoods(int RowIndex)
        {
            string Id = dataGridViewGoods.Rows[RowIndex].Cells[dataGridViewGoods.Columns["AvtoID"].Index].Value.ToString();
            if (Id == "")
                return;

            if (catalogState == "Выбрать" && activeForm != null && !activeForm.IsDisposed && activeForm.Visible)
            {
                catalogState = "Купить";
                SetFilterAuto();
                activeForm.acticleSelected(Id);
                activeForm.Activate();
                return;
            }
            else if (catalogState == "Выбрать")
            {
                catalogState = "Купить";
                SetFilterAuto();
                activeForm.Dispose();
                activeForm = null;
            }

            
            Data.Id = Id;
            Data.ConnCe = ConnCe;
            FormEnterQty Form_EnterQty = new FormEnterQty();

            Form_EnterQty.textBoxQty.Text = "1";
            Form_EnterQty.textBoxPrice.Text = Convert.ToString(Math.Round(Convert.ToDouble(dataGridViewGoods.Rows[RowIndex].Cells[dataGridViewGoods.Columns["AvtoPrice"].Index].Value), 2));

            Form_EnterQty.ShowDialog(this);
            if (Form_EnterQty.basketQty >= 0.0)
                dataGridViewGoods.Rows[RowIndex].Cells[dataGridViewGoods.Columns["AvtoOrdered"].Index].Value = Form_EnterQty.basketQty;

            settext_labelbasket();

        }

        private void dataGridViewGoods_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int AvtoBuy = dataGridViewGoods.Columns["AvtoBuy"].Index;
            int AvtoPhoto = dataGridViewGoods.Columns["AvtoPhoto"].Index;
            if (e.ColumnIndex == AvtoBuy && e.RowIndex >= 0)
            {
                BuyGoods(e.RowIndex);
            }
            if (e.ColumnIndex == AvtoPhoto && e.RowIndex >= 0)
            {
                string Id = dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoID"].Index].Value.ToString();
                string GoodsName = dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoNomName"].Index].Value.ToString();

                showGoodsImage(Id, GoodsName);

            }

        }

        private void dataGridViewAnalogs_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int AvtoBuy = dataGridViewAnalogs.Columns["AnalogBuy"].Index;
            int AvtoPhoto = dataGridViewAnalogs.Columns["AnalogPhoto"].Index;
            if (e.ColumnIndex == AvtoBuy && e.RowIndex >= 0)
            {
                string Id = dataGridViewAnalogs.Rows[e.RowIndex].Cells[dataGridViewAnalogs.Columns["AnalogID"].Index].Value.ToString();
                Data.Id = Id;
                Data.ConnCe = ConnCe;
                FormEnterQty Form_EnterQty = new FormEnterQty();

                Form_EnterQty.textBoxQty.Text = "1";
                Form_EnterQty.textBoxPrice.Text = Convert.ToString(Math.Round(Convert.ToInt32(dataGridViewAnalogs.Rows[e.RowIndex].Cells[dataGridViewAnalogs.Columns["AnalogPrice"].Index].Value)/1.0, 2));

                Form_EnterQty.ShowDialog(this);

                if (Form_EnterQty.basketQty >= 0.0)
                    dataGridViewAnalogs.Rows[e.RowIndex].Cells[dataGridViewAnalogs.Columns["AnalogOrdered"].Index].Value = Form_EnterQty.basketQty;

                settext_labelbasket();

                this.Update();

            }
            if (e.ColumnIndex == AvtoPhoto && e.RowIndex >= 0)
            {
                string Id = dataGridViewAnalogs.Rows[e.RowIndex].Cells[dataGridViewAnalogs.Columns["AnalogID"].Index].Value.ToString();
                string GoodsName = dataGridViewAnalogs.Rows[e.RowIndex].Cells[dataGridViewAnalogs.Columns["AnalogNomName"].Index].Value.ToString();

                showGoodsImage(Id, GoodsName);
                this.Update();
            }
        }

        private void settext_labelbasket()
        {
             SqlCeCommand com = new SqlCeCommand();
            com.Connection = ConnCe;
            com.CommandText = "SELECT SUM (Total) AS Total FROM basket";

            var result = com.ExecuteScalar();
            if (!DBNull.Value.Equals(result))
                this.labelbasket.Text = "В корзине " + Convert.ToString(Math.Round(Convert.ToDouble(result), 2)) + " " + Currency_Klient;
            else
                this.labelbasket.Text = "В корзине пусто";
        }

        private void dataGridViewGoods_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {

            if (e.RowIndex >= 0 && !dataGridViewGoods.Rows[e.RowIndex].IsNewRow)
               if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterNew"].Index].Value.ToString() == "1") 
                {
                    dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
                }
               else if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterAction"].Index].Value.ToString() == "1") 
                {
                    dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(252, 212, 4);
                }
               else if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterReceipt"].Index].Value.ToString() == "1") 
                {
                    dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(173, 137, 148);
                }
            //if (!DBNull.Value.Equals(dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoQtyPhoto"].Index].Value))           
            //{
            //    if (Convert.ToInt16(dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoQtyPhoto"].Index].Value) == 0)
            //    {
            //        dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoPhoto"].Index].Value = "";
            //    }
              
            //}
            //else
            
            //    dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoPhoto"].Index].Value = "";

            if (!DBNull.Value.Equals(dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoNomName"].Index].Value))
                if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoNomName"].Index].Value.ToString() == "")
                    dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoNomName"].Index].Value = ((DateTime)dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["ColumnDateReceipt"].Index].Value).ToString("D");
          
        }

        private void checkBoxNew_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxNew1.Checked = checkBoxNew.Checked;
            if (checkBoxNew1.Checked)
            {
                if (checkBoxAction.Checked)
                {
                    checkBoxAction.Checked = false;
                    checkBoxAction1.Checked = false;
                }
                if (checkBoxReceipt.Checked)
                {
                    checkBoxReceipt.Checked = false;
                    checkBoxReceipt1.Checked = false;
                }
            }
            SetFilterAuto();

        }
 
        private void comboBoxCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_Currency = comboBoxCurrency.Text;
            if (comboBoxCurrency.Text == "USD")
                Currency = Currency_USD;
            else if (comboBoxCurrency.Text == "EUR")
                Currency = Currency_EUR;
            else
                Currency = 1;

            if (Currency_Klient == current_Currency)
                Currency_Course_Klient = 1;
            else if (current_Currency.Equals("Грн") && Currency_Klient.Equals("USD"))
                Currency_Course_Klient = Currency_USD;
            else if (current_Currency.Equals("Грн") && Currency_Klient.Equals("EUR"))
                Currency_Course_Klient = Currency_EUR;
            else if (current_Currency.Equals("USD") && Currency_Klient.Equals("Грн"))
                Currency_Course_Klient = 1 / Currency_USD;
            else if (current_Currency.Equals("USD") && Currency_Klient.Equals("EUR"))
                Currency_Course_Klient = Currency_EUR / Currency_USD;
            else if (current_Currency.Equals("EUR") && Currency_Klient.Equals("Грн"))
                Currency_Course_Klient = 1 / Currency_EUR;
            else if (current_Currency.Equals("EUR") && Currency_Klient.Equals("USD"))
                Currency_Course_Klient = Currency_USD / Currency_EUR;

            int cr =-1;
            if (dataGridViewGoods.CurrentRow!=null)
                cr = dataGridViewGoods.CurrentRow.Index;
            SetFilterAuto();
            
            if (cr != -1)
                dataGridViewGoods.CurrentCell = dataGridViewGoods.Rows[cr].Cells["AvtoNomName"];
        }

        private void checkBoxReceipt_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxReceipt1.Checked = checkBoxReceipt.Checked;
            if (checkBoxReceipt.Checked)
            {
                if (checkBoxNew.Checked)
                {
                    checkBoxNew.Checked = false;
                    checkBoxNew1.Checked = false;
                }
                if (checkBoxAction.Checked)
                {
                    checkBoxAction.Checked = false;
                    checkBoxAction1.Checked = false;
                }
            }
            SetFilterAuto();
        }

        private void UpdateCatalogComplete(Object sender,
                                 WebStore.UpdateDatabaseCompletedEventArgs e)
        {

            string result = "";
            try
            {
                result = e.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
            if (result != "")
            {
                MessageBox.Show(this, e.Result, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            DialogResult res = MessageBox.Show(this, "Получено обнобление базы данных. Обновить данные?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }

//            FormWaiting f_FormWaiting = new FormWaiting();
//            f_FormWaiting.Owner = this;
//            f_FormWaiting.labelDescription.Text = "Выполняется загрузка в базу данных";
//            f_FormWaiting.Show();

            toolStripStatusUpdateDatabase.Text = "Выполняется загрузка в базу данных";

            MemoryStream MS = new MemoryStream();
            BinaryWriter STG = new BinaryWriter(MS);
            STG.Write(e.Data);
            MS.Position = 0;
            ZipFile zip = ZipFile.Read(MS);

            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();

            AuthorizeService(cService, customer);

            UpdateDataBase(zip, customer.Login);

            MS.Close();
            STG.Close();

            MessageBox.Show(this, "Выполнение обмена выполнено успешно", "Обмен выполнен", MessageBoxButtons.OK);
        }

        private void UpdateCatalog()
        {

//            byte[] data; 
//            FormWaiting f_FormWaiting = new FormWaiting();
//            f_FormWaiting.Owner = this;
//            f_FormWaiting.labelDescription.Text = "Выполняется соединение и получение данных с сервера";
//            f_FormWaiting.ShowDialog();

            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();

            AuthorizeService(cService, customer);
            toolStripStatusUpdateDatabase.Visible = true;
            toolStripStatusUpdateDatabase.Text = "выполняется запрос к серверу на обновление базы данных";
            cService.UpdateDatabaseCompleted += new WebStore.UpdateDatabaseCompletedEventHandler(this.UpdateCatalogComplete);

            try
            {
                cService.UpdateDatabaseAsync(customer);
//                cService.UpdateDatabase(customer, out data);
            }  
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "Обновить данные?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }
            
            UpdateCatalog();        
       }

        private void Delete_Filter()
         {

            checkBoxName.Checked = false;
            checkBoxManufac.Checked = false;
            checkBoxArticle.Checked = false;

            checkBoxNew.Checked = false;
            checkBoxReceipt.Checked = false;
            checkBoxAction.Checked = false;

        }

        private void Show_New()
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             Delete_Filter();
            checkBoxNew.Checked = true;

            SetFilterAuto();           
        }

        private void Show_Receipt()
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             Delete_Filter();
             checkBoxReceipt.Checked = true;

             SetFilterAuto();
         }

        private void Show_Action()
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             Delete_Filter();
             checkBoxAction.Checked = true;
            checkBoxReceipt.Checked = false;

             SetFilterAuto();
         }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Show_Receipt();
          
        }

        private void последниеПоступленияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show_Receipt();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Show_New();

        }

        private void новыеПозицииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show_New();

        }

        private void акционныеТоварыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show_Action();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Show_Action();

        }

        private void checkBoxAction_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxAction1.Checked = checkBoxAction.Checked;
            if (checkBoxAction.Checked)
            {
                if (checkBoxNew.Checked)
                {
                    checkBoxNew.Checked = false;
                    checkBoxNew1.Checked = false;
                }
                if (checkBoxReceipt.Checked)
                {
                    checkBoxReceipt.Checked = false;
                    checkBoxReceipt1.Checked = false;
                }
            }
            SetFilterAuto();
        }

        private void tabControlMain_TabIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageBasket"])
            {
                basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
            }

            if (tabControlMain.SelectedTab == tabControlMain.TabPages["tabPageInfo"])
            {
                basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
            }


        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            SaveBasketChange();
            tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
           
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

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "Очистить корзину?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.OK)
            {


                //dataGridViewBascet.Rows.Clear();           
                while (dataGridViewBascet.Rows.Count > 0)
                {
                    for (int i = 0; i < dataGridViewBascet.Rows.Count; i++)

                        dataGridViewBascet.Rows.RemoveAt(0);


                }
                SaveBasketChange();
                settext_labelbasket();
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (dataGridViewBascet.RowCount>0)
                UpdateCustomerInfo(true);
        }

       private void formOrder(WebStore.WebStore cService, WebStore.Customers customer, Form f_FormWaiting)
        {
            WebStore.Order order = new WebStore.Order();
            string _s_ = "";

            order.OrderNumber = "";
            order.Customer = customer;

            SqlCeCommand com_ = new SqlCeCommand("SELECT COUNT(1) AS RecordCnt FROM basket", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
                order.Table = new WebStore.Table[Convert.ToInt32(reader[0])];
            else
                return;

            com_ = new SqlCeCommand("SELECT * FROM basket", ConnCe);
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

            try
            {
                _s_ = cService.FormOrder(customer, order);
            }
            catch (Exception ex)
            {
                MessageBox.Show(f_FormWaiting, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (_s_ != "" && _s_ == "Программа не активирована")
            {
                MessageBox.Show(f_FormWaiting, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }
           

            while (dataGridViewBascet.Rows.Count > 0)
            {
                for (i = 0; i < dataGridViewBascet.Rows.Count; i++)

                    dataGridViewBascet.Rows.RemoveAt(0);
            }

            SaveBasketChange();
            settext_labelbasket();

           if (_s_ != "" )
            {
                MessageBox.Show(f_FormWaiting, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }

        }

        private void корзинаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveBasketChange();
            tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageBasket"];
        }

        public void basketUpdate()
        {
            basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageBasket"])
            {
                basketTableAdapter.Fill(this.myDatabaseDataSet.basket);
            }

            if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageBasket"])
            {
                newsTableAdapter.Fill(this.myDatabaseDataSet.News);
            }

            this.Update();
        }

        private void dataGridViewBascet_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridViewBascet.Columns["QtyGridViewBascet"].Index)
            {
                double Price = Convert.ToDouble(dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["PriceGridViewBascet"].Index].Value);
                double Qty = Convert.ToDouble(dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["QtyGridViewBascet"].Index].Value);
                dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["TotalGridViewBascet"].Index].Value = Price * Qty;
                SaveBasketChange();
                settext_labelbasket();
            }
        }

        private void dataGridViewGoods_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.Insert) ||
                (e.Control && e.KeyCode == Keys.C))
                e.SuppressKeyPress = true;
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuyGoods(dataGridViewGoods.CurrentRow.Index);
            }
        }

        private void dataGridViewAnalogs_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.Insert) ||
                (e.Control && e.KeyCode == Keys.C))
                e.SuppressKeyPress = true;
            
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
//                BuyGoods(dataGridViewGoods.CurrentRow.Index);
            }

        }

        private void dataGridViewOriginal_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.Insert) ||
                (e.Control && e.KeyCode == Keys.C))
                e.SuppressKeyPress = true;
        }

        private void dataGridViewBascet_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.Insert) ||
                (e.Control && e.KeyCode == Keys.C))
                e.SuppressKeyPress = true;
        }

        public void AuthorizeService(WebStore.WebStore cService, WebStore.Customers customer)
        {
            string pass = "";

            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
            {
                customer.Login = Convert.ToString(reader["Login"]);
                pass = Convert.ToString(reader["Password"]);
                customer.Code = Convert.ToString(reader["CustomerCode"]);
                customer.RegistrationKey = GetRegistrationKey();
                customer.IP = GetExternalIP();
            }
            else
                return;

            ICredentials credentials = new NetworkCredential(customer.Login, pass);
            cService.Credentials = credentials;
        }
 

        private void обновитьЖурналЗаказовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateCustomerInfo(false);           
 
        }

        private void новостиКомпанииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageInfo"];

        }

        private void актСверкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AktSverki();
        }

        private Boolean обновитьAktSverky(DateTime DateFrom, DateTime DateFor)
        {

            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();
            WebStore.AktSverkyRow[] aktSverky;
            string _s_ = "";

            AuthorizeService(cService, customer);

            try
            {
                _s_ = cService.AktSverky(customer, DateFrom, DateFor, out aktSverky);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            if (_s_ != "")
            {
                MessageBox.Show(this, _s_, "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            SqlCeCommand com_del = new SqlCeCommand("DELETE FROM AktSverki", ConnCe);
            SqlCeCommand com_ins = new SqlCeCommand("INSERT INTO AktSverki (Date, Number, Document, Payment, shipping) VALUES(@Date, @Number, @Document, @Payment, @shipping)", ConnCe);

            com_del.ExecuteNonQuery();

            com_ins.Parameters.AddWithValue("@Date", "");
            com_ins.Parameters.AddWithValue("@Number", "");
            com_ins.Parameters.AddWithValue("@Document", "");
            com_ins.Parameters.AddWithValue("@Payment", "");
            com_ins.Parameters.AddWithValue("@shipping", "");

            foreach (WebStore.AktSverkyRow aktSverkyRow in aktSverky)
            {
                com_ins.Parameters["@Date"].Value = aktSverkyRow.Date;
                com_ins.Parameters["@Number"].Value = aktSverkyRow.Number;
                com_ins.Parameters["@Document"].Value = aktSverkyRow.Document;
                com_ins.Parameters["@Payment"].Value = aktSverkyRow.Payment;
                com_ins.Parameters["@shipping"].Value = aktSverkyRow.Shipping;
                com_ins.ExecuteNonQuery();

            }

            return true;

        }

        private void AktSverki()
        {
            Data.FormAktSverki = false;

            FormEnterDate Form_EnterQty = new FormEnterDate();
            Form_EnterQty.ShowDialog(this);

            if (Data.FormAktSverki)
            //нужно написать обращение к 1с
            {
                DateTime DateFrom = Data.DateFrom;
                DateTime DateFor = Data.DateFor;

                Boolean result =  обновитьAktSverky( DateFrom,  DateFor);
                if (result == false) return;

                FormAktSverki Form_AktSverki = new FormAktSverki();

                SqlCeDataAdapter com_AktSverki = new SqlCeDataAdapter("SELECT Date,Number,Document,shipping,Payment  FROM AktSverki", ConnCe);
                DataTable DataTabAktSverki = new DataTable("AktSverki");

                com_AktSverki.Fill(DataTabAktSverki);

                Form_AktSverki.dataGridViewAktSverki.DataSource = DataTabAktSverki;
                Form_AktSverki.labelPeriod.Text = "c " + String.Format("{0:d MMMM  yyyy}", DateFrom) + " года по " + String.Format("{0:d MMMM  yyyy}", DateFor) + " года";
                
                Form_AktSverki.dataGridViewAktSverki.Columns["Date"].HeaderText = "Дата";
                Form_AktSverki.dataGridViewAktSverki.Columns["Date"].Width = 70;

                Form_AktSverki.dataGridViewAktSverki.Columns["Number"].HeaderText = "Номер";
                Form_AktSverki.dataGridViewAktSverki.Columns["Number"].Width = 90;
                
                Form_AktSverki.dataGridViewAktSverki.Columns["Document"].HeaderText = "Документ";
                Form_AktSverki.dataGridViewAktSverki.Columns["Document"].Width = 470;

                Form_AktSverki.dataGridViewAktSverki.Columns["Payment"].HeaderText = "Оплата";
                Form_AktSverki.dataGridViewAktSverki.Columns["Payment"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                Form_AktSverki.dataGridViewAktSverki.Columns["Payment"].DefaultCellStyle.Format = "N2"; 
                Form_AktSverki.dataGridViewAktSverki.Columns["Payment"].Width = 80;

                Form_AktSverki.dataGridViewAktSverki.Columns["shipping"].HeaderText = "Отгрузка";
                Form_AktSverki.dataGridViewAktSverki.Columns["shipping"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                Form_AktSverki.dataGridViewAktSverki.Columns["shipping"].DefaultCellStyle.Format = "N2";  
                Form_AktSverki.dataGridViewAktSverki.Columns["shipping"].Width = 80;
               
  
                //Form_AktSverki.dataGridViewAktSverki.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                Form_AktSverki.ShowDialog(this);
            }          
        }
 
        private void toolStripAktSverki_Click_1(object sender, EventArgs e)
        {
            AktSverki();
        }

        private void newsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
 //           int NewsInfo = newsDataGridView.Columns["newsDataGridViewInfo"].Index;
 //           if (e.ColumnIndex == newsDataGridView.Columns["newsDataGridViewInfo"].Index)
            if (e.RowIndex>=0)
            {
                News news = new News(this, (string)newsDataGridView.Rows[e.RowIndex].Cells["newsDataGridViewID"].Value);
                news.ShowDialog(this);
                

                //string DocumentText = newsDataGridView.Rows[e.RowIndex].Cells[newsDataGridView.Columns["newsDataGridViewhtml"].Index].Value.ToString();

                //SqlCeCommand com_sel = new SqlCeCommand("SELECT * FROM News WHERE ID= @ID", ConnCe);
                //com_sel.Parameters.AddWithValue("@ID", (string)newsDataGridView.Rows[e.RowIndex].Cells["newsDataGridViewID"].Value);

                //SqlCeDataReader reader = com_sel.ExecuteReader();
                //reader.Read();


                //for (int i = 1; i <= (int) newsDataGridView.Rows[e.RowIndex].Cells["newsDataGridViewImgsCount"].Value; i++)
                //{
                //    string fileName = System.IO.Path.GetTempFileName();
                //    DocumentText = DocumentText.Replace(String.Concat(@"image00", i.ToString()), String.Concat(@"file:\\",fileName));
                //    FileStream fs = new FileStream(fileName, FileMode.Create);
                //    BinaryWriter w = new BinaryWriter(fs);
                //    w.Write((byte[])reader[String.Concat("image00", i.ToString())]);
                //    //String.Concat("image00", i.ToString());
                //    //string src = imglObject.GetAttribute("src");
                //    //src = src.Substring(String.Compare(src, ":"));

                //    //imglObject.SetAttribute("src", System.IO.Path.GetTempFileName());

                //}

                //webBrowserNews.DocumentText = DocumentText;
                              
             }
        }

        private void webBrowserNews_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //webBrowserNews.Refresh();

            //if (webBrowserNews.Document.Images.Count != 0)
            //{
            //    for (int i = 0; i < webBrowserNews.Document.Images.Count; i++)
            //    {
            //        var imglObject = webBrowserNews.Document.Images[i];
            //        string src = imglObject.GetAttribute("src");
            //        src = src.Substring(String.Compare(src, ":"));

            //        imglObject.SetAttribute("src", System.IO.Path.GetTempFileName());

            //    }
            //    //нужно заменить на картинку
            //}
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            
            ordersTTableAdapter.Fill(this.myDatabaseDataSet.OrdersT, dataGridView1.Rows[e.RowIndex].Cells["OrderID"].Value.ToString());

            dataGridView2.Columns["orderIDDataGridViewTextBoxColumn"].Visible = false;
        }

        private void обновитьБазуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "Отправить запрос на обновление данных?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }

            UpdateCatalog();
        }
             
        private void textBoxMarkUp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar)) && !((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf(".") == -1) && (((TextBox)sender).Text.Length != 0)))
            {
                if (e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            } 
        }

        private void textBoxMarkUp_Leave(object sender, EventArgs e)
        {
            SetFilterAuto();
        }

        private void setNameMenuChangeMark()
        {
            if (textBoxMarkUp.Text == "")
            {
                ChangeMarkUpStripMenuItem.Text = "Установить процент наценки";
            }
            else ChangeMarkUpStripMenuItem.Text = "Изменить процент наценки ( " + textBoxMarkUp.Text + " %)";
            
        }

        private void textBoxMarkUp_TextChanged(object sender, EventArgs e)
        {
            setNameMenuChangeMark();
            SetFilterAuto();
        }

        private void журналОтгруженныхНакладныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabControlMain.TabPages["tabPageOrder"];
        }

        private void импортИзФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importFromExcel();
        }

        private void UpdateCustomerInfo(bool bFormOrder)
        {
            FormWaiting f_FormWaiting = new FormWaiting();
            f_FormWaiting.Owner = this;

            WebStore.Customers customer = new WebStore.Customers();
            WebStore.WebStore cService = new WebStore.WebStore();

            AuthorizeService(cService, customer);

            f_FormWaiting.Show(this);

            if (bFormOrder)
            {
                f_FormWaiting.labelDescription.Text = "Отправка заказа на сервер";
                f_FormWaiting.Update();
                formOrder(cService, customer, f_FormWaiting);
            }

            DateTime updateDate = new DateTime(2014,1,1);
            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
               updateDate = Convert.ToDateTime(reader["UpdateDate"]);

            f_FormWaiting.labelDescription.Text = "Выполняется обновление партнерской информации";
            f_FormWaiting.Update();
            UpdateInfo(cService, customer, f_FormWaiting);

            f_FormWaiting.labelDescription.Text = "Выполняется обновление списка новостей";
            f_FormWaiting.Update();
            UpdateNews(cService, customer, updateDate, f_FormWaiting);

            f_FormWaiting.labelDescription.Text = "Выполняется обновление журнала отгрузок";
            f_FormWaiting.Update();
            UpdateOrders(cService, customer, updateDate, f_FormWaiting);

            f_FormWaiting.Close();
            MessageBox.Show(this, "Обновление партнерской информации выполнено", "Обновление выполнено", MessageBoxButtons.OK);

            SqlCeCommand com_upd = new SqlCeCommand("UPDATE constant SET UpdateDate=@UpdateDate", Data.ConnCe);

            DateTime now = DateTime.Now;
            com_upd.Parameters.AddWithValue("@UpdateDate", now);

            com_upd.ExecuteNonQuery();

            toolStripStatusCustomerUpdateDate.Text = "Данные партнера обновлены " + now.ToString();

            UpdateVersion(cService, customer);

        }

        private void UpdateVersion(WebStore.WebStore cService, WebStore.Customers customer)
        {
            string currentVersion="";
            //AuthorizeService(cService, customer);

            try
            {
                currentVersion = cService.GetAVDClientVersion();
            }
            catch (Exception ex)
            {
                return;
            }

            if (currentVersion != "" && currentVersion != version)
            {
                DialogResult res = MessageBox.Show(this, "Доступно обновление программы. Установить новую версию?", "Установить новую версию?", MessageBoxButtons.OKCancel);

                if (res == DialogResult.Cancel)
                    return;

                toolStripStatusUpdateVersion.Visible = true;
                toolStripStatusUpdateVersion.Text = "выполняется запрос к серверу на обновление программы";
                cService.UpdateAVDClientCompleted += new WebStore.UpdateAVDClientCompletedEventHandler(this.UpdateAVDClientComplete);

                try
                {
                    cService.UpdateAVDClientAsync(customer);
                    //                cService.UpdateDatabase(customer, out data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                    return;
                }

            }
        }


        private void UpdateAVDClientComplete(Object sender,
                                  WebStore.UpdateAVDClientCompletedEventArgs e)
        {

            string result = "";
            try
            {
                result = e.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
            if (result != "")
            {
                MessageBox.Show(this, e.Result, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            DialogResult res = MessageBox.Show(this, "Получено обновление программы. Обновить программу?", "Обновить программу?", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }


            string FILE_NAME = Path.GetTempPath()+Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".exe";
            FileStream FS = new FileStream(FILE_NAME, FileMode.CreateNew);
            BinaryWriter STG = new BinaryWriter(FS);
            STG.Write(e.Application);

            FS.Close();
            STG.Close();

            Application.Exit();
            this.Close();

            System.Diagnostics.Process.Start(FILE_NAME);

        }


        private void UpdateInfo(WebStore.WebStore cService, WebStore.Customers customer, Form f_FormWaiting)
        {

            WebStore.CustomerInfo customerInfo = new WebStore.CustomerInfo();
            string _s_ = "";

            AuthorizeService(cService, customer);

            try
            {
                _s_ = cService.GetCustomerInfo(customer, out customerInfo);
            }
 
            catch (Exception ex)
            {
                MessageBox.Show(f_FormWaiting, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (_s_ != "")
            {
                MessageBox.Show(f_FormWaiting, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            SqlCeCommand com_upd = new SqlCeCommand("UPDATE constant SET ManagerName=@ManagerName, ManagerPhone=@ManagerPhone, ManagerEmail=@ManagerEmail, ManagerSkipe=@ManagerSkipe, ManagerICQ=@ManagerICQ, SummaCredit=@SummaCredit, Currency=@Currency, DayCredit=@DayCredit, FIO=@FIO, DeliveryInfo=@DeliveryInfo, SummaDolg=@SummaDolg", ConnCe);

            com_upd.Parameters.AddWithValue("@ManagerName", customerInfo.ManagerName);
            com_upd.Parameters.AddWithValue("@ManagerEmail", customerInfo.ManagerEmail);
            com_upd.Parameters.AddWithValue("@ManagerPhone", customerInfo.ManagerPhone);
            com_upd.Parameters.AddWithValue("@ManagerSkipe", customerInfo.ManagerSkipe);
            com_upd.Parameters.AddWithValue("@ManagerICQ", customerInfo.ManagerICQ);
            com_upd.Parameters.AddWithValue("@SummaCredit", customerInfo.SummaCredit);
            com_upd.Parameters.AddWithValue("@Currency", customerInfo.Currency);
            com_upd.Parameters.AddWithValue("@DayCredit", customerInfo.DayCredit);
            com_upd.Parameters.AddWithValue("@FIO", customerInfo.FIO);
            com_upd.Parameters.AddWithValue("@DeliveryInfo", customerInfo.DeliveryInfo);
            com_upd.Parameters.AddWithValue("@SummaDolg", customerInfo.SummaDolg);

            com_upd.ExecuteNonQuery();

            com_upd.CommandText = "DELETE FROM Currency";
            com_upd.ExecuteNonQuery();

            com_upd.CommandText = "INSERT INTO Currency (Kurs_USD, Kurs_EUR) VALUES(@Kurs_USD, @Kurs_EUR)";
            com_upd.Parameters.Clear();

            com_upd.Parameters.AddWithValue("@Kurs_USD", customerInfo.USD);
            com_upd.Parameters.AddWithValue("@Kurs_EUR", customerInfo.EUR);
            com_upd.ExecuteNonQuery();

            SetCurrency();
            Set_Labels_Course();

            SqlCeCommand com_del = new SqlCeCommand("DELETE FROM OrdersDolg", ConnCe);
            SqlCeCommand com_ins = new SqlCeCommand("INSERT INTO OrdersDolg (Date, Number, PayDate, Days, SummaDolg,Prosrocheno,Today) VALUES(@Date, @Number, @PayDate, @Days, @SummaDolg, @Prosrocheno, @Today)", ConnCe);

            com_del.ExecuteNonQuery();

            com_ins.Parameters.AddWithValue("@Date", "");
            com_ins.Parameters.AddWithValue("@Number", "");
            com_ins.Parameters.AddWithValue("@PayDate", "");
            com_ins.Parameters.AddWithValue("@Days", "");
            com_ins.Parameters.AddWithValue("@SummaDolg", "");
            com_ins.Parameters.AddWithValue("@Prosrocheno", "");
            com_ins.Parameters.AddWithValue("@Today", "");

            if (customerInfo.OrdersDolg!=null)
                foreach (WebStore.OrdersDolgRow ordersDolgRow in customerInfo.OrdersDolg)
                {
                    com_ins.Parameters["@Date"].Value = ordersDolgRow.Date;
                    com_ins.Parameters["@Number"].Value = ordersDolgRow.Number;
                    com_ins.Parameters["@PayDate"].Value = ordersDolgRow.PayDate;
                    com_ins.Parameters["@Days"].Value = ordersDolgRow.Days;
                    com_ins.Parameters["@SummaDolg"].Value = ordersDolgRow.SummaDolg;
                    com_ins.Parameters["@Prosrocheno"].Value = ordersDolgRow.Prosrocheno;
                    com_ins.Parameters["@Today"].Value = ordersDolgRow.Today;
                    com_ins.ExecuteNonQuery();
                }
            set_Info();
        }

        private void UpdateNews(WebStore.WebStore cService, WebStore.Customers customer, DateTime updateDate, Form f_FormWaiting)
        {
            WebStore.News[] newsList;
            string _s_ = "";

            try
            {
                _s_ = cService.GetNewsList(customer, updateDate.AddDays(-3), out newsList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(f_FormWaiting, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (_s_ != "")
            {
                MessageBox.Show(f_FormWaiting, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }

 //           SqlCeCommand com_modif = new SqlCeCommand("ALTER TABLE News ALTER COLUMN html nvarchar(20000)", ConnCe);
 //           com_modif.ExecuteNonQuery();
            SqlCeCommand com_del = new SqlCeCommand("DELETE FROM News WHERE ID = @ID", ConnCe);
            SqlCeCommand com_ins = new SqlCeCommand("INSERT INTO News (ID, Date, Info, html, Image001, Image002,Image003,Image004,Image005,Image006,Image007,Image008,Image009,Image010,ImgsCount) VALUES(@ID, @Date, @Info, @html, @Image001, @Image002,@Image003,@Image004,@Image005,@Image006,@Image007,@Image008,@Image009,@Image010,@ImgsCount)", ConnCe);

//            com_ins.Parameters.Clear();
            byte[] emptyImg = new byte[0];

            com_del.Parameters.AddWithValue("@ID", "");

            com_ins.Parameters.AddWithValue("@ID", "");
            com_ins.Parameters.AddWithValue("@Date", "");
            com_ins.Parameters.AddWithValue("@Info", "");
            com_ins.Parameters.AddWithValue("@html", "");
            com_ins.Parameters.AddWithValue("@Image001", emptyImg);
            com_ins.Parameters.AddWithValue("@Image002", emptyImg);
            com_ins.Parameters.AddWithValue("@Image003", emptyImg);
            com_ins.Parameters.AddWithValue("@Image004", emptyImg);
            com_ins.Parameters.AddWithValue("@Image005", emptyImg);
            com_ins.Parameters.AddWithValue("@Image006", emptyImg);
            com_ins.Parameters.AddWithValue("@Image007", emptyImg);
            com_ins.Parameters.AddWithValue("@Image008", emptyImg);
            com_ins.Parameters.AddWithValue("@Image009", emptyImg);
            com_ins.Parameters.AddWithValue("@Image010", emptyImg);
            com_ins.Parameters.AddWithValue("@ImgsCount", 0);

            foreach (WebStore.News news in newsList)
            {
                com_del.Parameters["@ID"].Value = news.ID;
                com_del.ExecuteNonQuery();

                com_ins.Parameters["@ID"].Value = news.ID;
                com_ins.Parameters["@Date"].Value = news.Date;
                com_ins.Parameters["@Info"].Value = news.Info;
                com_ins.Parameters["@html"].Value = news.html;
                com_ins.Parameters["@ImgsCount"].Value = news.ImgsCount;
                if (news.ImgsCount > 0) com_ins.Parameters["@Image001"].Value = news.Image001; else com_ins.Parameters["@Image001"].Value = emptyImg;
                if (news.ImgsCount > 1) com_ins.Parameters["@Image002"].Value = news.Image002; else com_ins.Parameters["@Image002"].Value = emptyImg;
                if (news.ImgsCount > 2) com_ins.Parameters["@Image003"].Value = news.Image003; else com_ins.Parameters["@Image003"].Value = emptyImg;
                if (news.ImgsCount > 3) com_ins.Parameters["@Image004"].Value = news.Image004; else com_ins.Parameters["@Image004"].Value = emptyImg;
                if (news.ImgsCount > 4) com_ins.Parameters["@Image005"].Value = news.Image005; else com_ins.Parameters["@Image005"].Value = emptyImg;
                if (news.ImgsCount > 5) com_ins.Parameters["@Image006"].Value = news.Image006; else com_ins.Parameters["@Image006"].Value = emptyImg;
                if (news.ImgsCount > 6) com_ins.Parameters["@Image007"].Value = news.Image007; else com_ins.Parameters["@Image007"].Value = emptyImg;
                if (news.ImgsCount > 7) com_ins.Parameters["@Image008"].Value = news.Image008; else com_ins.Parameters["@Image008"].Value = emptyImg;
                if (news.ImgsCount > 8) com_ins.Parameters["@Image009"].Value = news.Image009; else com_ins.Parameters["@Image009"].Value = emptyImg;
                if (news.ImgsCount > 9) com_ins.Parameters["@Image010"].Value = news.Image010; else com_ins.Parameters["@Image010"].Value = emptyImg;
                com_ins.ExecuteNonQuery();

            }

            newsTableAdapter.Fill(this.myDatabaseDataSet.News);

        }

        private void UpdateOrders(WebStore.WebStore cService, WebStore.Customers customer, DateTime updateDate, Form f_FormWaiting)
        {
 
            WebStore.Invoice[] invList;
            string _s_ = "";

            try
            {
                _s_ = cService.GetInvoiceList(customer, updateDate.AddDays(-7), out invList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(f_FormWaiting, ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (_s_ != "")
            {
                MessageBox.Show(f_FormWaiting, _s_, "Ошибка", MessageBoxButtons.OK);
                return;
            }


            SqlCeCommand com_del = new SqlCeCommand("DELETE FROM OrdersH WHERE OrderID = @OrderID", ConnCe);
            SqlCeCommand com_ins = new SqlCeCommand("INSERT INTO OrdersH (Number, Date, Total, DeliveryInfo, Status, DeliveryDate, OrderID, Currency) VALUES(@Number, @Date, @Total, @DeliveryInfo, @Status, @DeliveryDate, @OrderID, @Currency)", ConnCe);
            SqlCeCommand comT_del = new SqlCeCommand("DELETE FROM OrdersT WHERE OrderID = @OrderID", ConnCe);
            SqlCeCommand comT_ins = new SqlCeCommand("INSERT INTO OrdersT (OrderID, ID, Qty, Price, Total, Num) VALUES(@OrderID, @ID, @Qty, @Price, @Total, @Num)", ConnCe);

            com_del.Parameters.AddWithValue("@OrderID", "");

            com_ins.Parameters.AddWithValue("@Number", "");
            com_ins.Parameters.AddWithValue("@Date", "");
            com_ins.Parameters.AddWithValue("@Total", "");
            com_ins.Parameters.AddWithValue("@DeliveryInfo", "");
            com_ins.Parameters.AddWithValue("@DeliveryDate", "");
            com_ins.Parameters.AddWithValue("@OrderID", "");
            com_ins.Parameters.AddWithValue("@Status", "");
            com_ins.Parameters.AddWithValue("@Currency", "");

            comT_del.Parameters.AddWithValue("@OrderID", "");

            comT_ins.Parameters.AddWithValue("@OrderID", "");
            comT_ins.Parameters.AddWithValue("@ID", "");
            comT_ins.Parameters.AddWithValue("@Qty", "");
            comT_ins.Parameters.AddWithValue("@Price", "");
            comT_ins.Parameters.AddWithValue("@Total", "");
            comT_ins.Parameters.AddWithValue("@Num", "");

            foreach (WebStore.Invoice invoice in invList)
            {
                com_del.Parameters["@OrderID"].Value = invoice.InvoiceID;
                com_del.ExecuteNonQuery();

                com_ins.Parameters["@Number"].Value = invoice.Number;
                com_ins.Parameters["@Date"].Value = invoice.Date;
                com_ins.Parameters["@Total"].Value = invoice.Total;
                com_ins.Parameters["@DeliveryInfo"].Value = invoice.DeliveryInfo;
                com_ins.Parameters["@DeliveryDate"].Value = invoice.DeliveryDate;
                com_ins.Parameters["@OrderID"].Value = invoice.InvoiceID;
                com_ins.Parameters["@Status"].Value = invoice.Status;
                com_ins.Parameters["@Currency"].Value = invoice.Currency;
                com_ins.ExecuteNonQuery();

                comT_del.Parameters["@OrderID"].Value = invoice.InvoiceID;
                comT_del.ExecuteNonQuery();
                if (invoice.Table != null)
                    foreach (WebStore.Table tableRow in invoice.Table)
                    {

                        comT_ins.Parameters["@OrderID"].Value = invoice.InvoiceID;
                        comT_ins.Parameters["@ID"].Value = tableRow.Code;
                        comT_ins.Parameters["@Qty"].Value = tableRow.Quantity;
                        comT_ins.Parameters["@Price"].Value = tableRow.Price;
                        comT_ins.Parameters["@Total"].Value = tableRow.Total;
                        comT_ins.Parameters["@Num"].Value = tableRow.Num;
                        comT_ins.ExecuteNonQuery();
                    }

            }

            ordersHTableAdapter.Fill(this.myDatabaseDataSet.OrdersH);
        }

        private void buttonUpdateInfo_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "Обновить торговые условия?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }
            UpdateCustomerInfo(false);
        }

        private void обновлениеТорговыхУсловийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "Обновить торговые условия?", "Вопрос", MessageBoxButtons.OKCancel);

            if (res == DialogResult.Cancel)
            {
                return;
            }

            UpdateCustomerInfo(false);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            UpdateCustomerInfo(false);      
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Вы уверены, что хотите выйти?", "AVD Client", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
            {
                Application.Exit();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout f_about = new FormAbout();
            f_about.ShowDialog(this);
        }

        private void сайтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://avdtrade.com.ua/");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("AVDClient.chm");
        }
        
        private void настройкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLogin f_Login = new FormLogin();
            f_Login.baseForm = this;
            f_Login.ShowDialog(this);
         }

        private void textBoxArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               Delete_Filter();
               if (textBoxArticle.Text.Trim().Length < 4)
                    checkBoxArticle.Checked = false;
                else
                    checkBoxArticle.Checked = true;
                SetFilterAuto();
            }

        }

        private void tabControlMain_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
         {
             TabPage CurrentTab = tabControlMain.TabPages[e.Index];
             Rectangle ItemRect = tabControlMain.GetTabRect(e.Index);
             SolidBrush FillBrush = new SolidBrush(Color.Red);
             SolidBrush TextBrush = new SolidBrush(Color.White);
             StringFormat sf = new StringFormat();
             sf.Alignment = StringAlignment.Center;
             sf.LineAlignment = StringAlignment.Center;

             //If we are currently painting the Selected TabItem we'll
             //change the brush colors and inflate the rectangle.
             if (System.Convert.ToBoolean(e.State & DrawItemState.Selected))
             {
                 FillBrush.Color = Color.White;
                 TextBrush.Color = Color.Red;
                 ItemRect.Inflate(2, 2);
             }

             //Set up rotation for left and right aligned tabs
             if (tabControl1.Alignment == TabAlignment.Left || tabControl1.Alignment == TabAlignment.Right)
             {
                 float RotateAngle = 90;
                 if (tabControl1.Alignment == TabAlignment.Left)
                     RotateAngle = 270;
                 PointF cp = new PointF(ItemRect.Left + (ItemRect.Width / 2), ItemRect.Top + (ItemRect.Height / 2));
                 e.Graphics.TranslateTransform(cp.X, cp.Y);
                 e.Graphics.RotateTransform(RotateAngle);
                 ItemRect = new Rectangle(-(ItemRect.Height / 2), -(ItemRect.Width / 2), ItemRect.Height, ItemRect.Width);
             }

             //Next we'll paint the TabItem with our Fill Brush
             e.Graphics.FillRectangle(FillBrush, ItemRect);

             //Now draw the text.
             e.Graphics.DrawString(CurrentTab.Text, e.Font, TextBrush, (RectangleF)ItemRect, sf);

             //Reset any Graphics rotation
             e.Graphics.ResetTransform();

             //Finally, we should Dispose of our brushes.
             FillBrush.Dispose();
             TextBrush.Dispose();
         }
     
        private void ShowPriceToolStripMenuItem_Click(object sender, EventArgs e)
         {
             if (ShowPriceToolStripMenuItem.CheckState == CheckState.Unchecked)
             {
                 ShowPriceToolStripMenuItem.Text = "Спрятать закупочную цену";
                 dataGridViewGoods.Columns["AutoValPrice"].Visible = true;
                 dataGridViewAnalogs.Columns["AnalogValPrice"].Visible = true;
              }
             else
             {
                 ShowPriceToolStripMenuItem.Text = "Отобразить закупочную цену";
                 //ShowPriceToolStripMenuItem.CheckState = CheckState.Unchecked;
                 dataGridViewGoods.Columns["AutoValPrice"].Visible = false;
                 dataGridViewAnalogs.Columns["AnalogValPrice"].Visible = false;
             }


             
         }

        private void ChangeMarkUpStripMenuItem_Click(object sender, EventArgs e)
         {
            FormMarkUp f_MarkUp = new FormMarkUp();
           f_MarkUp.Owner = this;
            //f_MarkUp.textBoxMarkUp.Text = textBoxMarkUp.Text;  
           f_MarkUp.ShowDialog(this);

         }

        private void buttonExcell_Click(object sender, EventArgs e)
         {

             Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
             Microsoft.Office.Interop.Excel.Workbook ExcelWorkBook;
             Microsoft.Office.Interop.Excel.Worksheet ExcelWorkSheet;
             //Книга.
             ExcelWorkBook = ExcelApp.Workbooks.Add(System.Reflection.Missing.Value);
             //Таблица.
             ExcelWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelWorkBook.Worksheets.get_Item(1);

             ExcelApp.Cells[2, 3] = "Расходная накладная № " + dataGridView1.CurrentRow.Cells["Number"].Value + " от " + dataGridView1.CurrentRow.Cells["Date"].FormattedValue;
             ExcelApp.Cells[2, 3].Font.Bold = true;

             ExcelApp.Cells[5, 2].Value = "№ п/п"; ExcelApp.Cells[5, 2].ColumnWidth = 8;
             ExcelApp.Cells[5, 3].Value = "Кат. номер"; ExcelApp.Cells[5, 3].ColumnWidth = 15;
             ExcelApp.Cells[5, 4].Value = "Товар";ExcelApp.Cells[5, 4].ColumnWidth = 60;
             ExcelApp.Cells[5, 5].Value = "Производитель"; ExcelApp.Cells[5, 5].ColumnWidth = 30;
             ExcelApp.Cells[5, 6].Value = "Кол-во";ExcelApp.Cells[5, 6].ColumnWidth = 15;
             ExcelApp.Cells[5, 7].Value = "Цена";ExcelApp.Cells[5, 7].ColumnWidth = 15;
             ExcelApp.Cells[5, 8].Value = "Сумма";ExcelApp.Cells[5, 8].ColumnWidth = 15;

             for (int i = 1; i < 8; i++)
             {
                 ExcelApp.Cells[5, i + 1].Font.Bold = true;
                 ExcelApp.Cells[5, i + 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                 ExcelApp.Cells[5, i + 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter; 
                 ExcelApp.Cells[5, i + 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
             }

             for (int i = 0; i < dataGridView2.Rows.Count; i++)
             {
                 ExcelApp.Cells[i + 6, 2].Value = dataGridView2.Rows[i].Cells["numDataGridViewTextBoxColumn"].Value;
                 ExcelApp.Cells[i + 6, 3].NumberFormat = "@";
                 ExcelApp.Cells[i + 6, 3].Value = dataGridView2.Rows[i].Cells["_ArticleDataGridViewTextBoxColumn"].Value.ToString();
                 ExcelApp.Cells[i + 6, 4].Value = dataGridView2.Rows[i].Cells["_NameDataGridViewTextBoxColumn"].Value.ToString();
                 ExcelApp.Cells[i + 6, 5].Value = dataGridView2.Rows[i].Cells["_ManufacDataGridViewTextBoxColumn"].Value.ToString();
                 ExcelApp.Cells[i + 6, 6].Value = dataGridView2.Rows[i].Cells["qtyDataGridViewTextBoxColumn"].Value;
                 ExcelApp.Cells[i + 6, 7].Value = Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells["priceDataGridViewTextBoxColumn"].Value),2);
                 ExcelApp.Cells[i + 6, 8].Value =  Math.Round(Convert.ToDouble(dataGridView2.Rows[i].Cells["totalDataGridViewTextBoxColumn1"].Value),2);

                 for (int Q = 1; Q < 8; Q++)
                {
                    ExcelApp.Cells[i + 6, Q + 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    ExcelApp.Cells[i + 6, Q + 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    ExcelApp.Cells[i + 6, Q + 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                }
                 ExcelApp.Cells[i + 6, 4].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

             }

             ExcelApp.Cells[dataGridView2.Rows.Count + 6, 8].Value = Math.Round(Convert.ToDouble(dataGridView1.CurrentRow.Cells["totalDataGridViewTextBoxColumn"].Value),2);
             ExcelApp.Cells[dataGridView2.Rows.Count + 6, 8].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
             ExcelApp.Cells[dataGridView2.Rows.Count + 6, 8].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
             ExcelApp.Cells[dataGridView2.Rows.Count + 6, 8].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

             //Вызываем нашу созданную эксельку.
             ExcelApp.Visible = true;
             ExcelApp.UserControl = true;  
         }

        private void splitContainer4_SplitterMoved(object sender, SplitterEventArgs e)
         {
             this.Update();

         }

        private void splitContainer5_SplitterMoved(object sender, SplitterEventArgs e)
         {
             this.Update();

         }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
         {
             this.Update();

         }

        private void Form1_Activated(object sender, EventArgs e)
         {
             this.Update();
         }


        private void Form1_Resize(object sender, EventArgs e)
         {
             this.Update();
         }

        private void dataGridViewAnalogs_Scroll(object sender, ScrollEventArgs e)
         {
             dataGridViewAnalogs.UpdateRowHeightInfo(0, true);
         }

        private void dataGridViewAnalogs_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
         {
             dataGridViewAnalogs.Update();
         }

        private void dataGridViewAnalogs_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
         {
             dataGridViewAnalogs.Update();
         }

        private void dataGridViewAnalogs_RowEnter(object sender, DataGridViewCellEventArgs e)
         {
             dataGridViewAnalogs.Update();
         }

        private void dataGridViewOriginal_RowEnter(object sender, DataGridViewCellEventArgs e)
         {
             dataGridViewOriginal.Update();
         }

        private void dataGridViewOriginal_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
         {
             dataGridViewOriginal.Update();
         }

         private void dataGridViewOriginal_Scroll(object sender, ScrollEventArgs e)
         {
             dataGridViewOriginal.Update();
         }

         private void dataGridViewOriginal_ColumnSortModeChanged(object sender, DataGridViewColumnEventArgs e)
         {
             dataGridViewOriginal.Update();
         }

         private void toolStripButton_Click(object sender, EventArgs e)
         {
             SaveBasketChange();
         }

         private void dataGridViewBascet_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
         {
             dataGridViewBascet.Rows[e.RowIndex].Cells[dataGridViewBascet.Columns["Del"].Index].Value = "X";
         }

         private void dataGridViewBascet_CellContentClick(object sender, DataGridViewCellEventArgs e)
         {
             int BascetDel = dataGridViewBascet.Columns["Del"].Index;
             if (e.ColumnIndex == BascetDel && e.RowIndex >= 0)
             {
                 dataGridViewBascet.Rows.RemoveAt(e.RowIndex);
                 SaveBasketChange();
                 settext_labelbasket();
             }

         }

         private void button2_Click(object sender, EventArgs e)
         {
             Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
             Microsoft.Office.Interop.Excel.Workbook ExcelWorkBook;
             Microsoft.Office.Interop.Excel.Worksheet ExcelWorkSheet;
             //Книга.
             ExcelWorkBook = ExcelApp.Workbooks.Add(System.Reflection.Missing.Value);
             //Таблица.
             ExcelWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ExcelWorkBook.Worksheets.get_Item(1);

             ExcelApp.Cells[2, 3] = "Заказ " + " от " + DateTime.Today;
             ExcelApp.Cells[2, 3].Font.Bold = true;

             ExcelApp.Cells[5, 2].Value = "№ п/п"; ExcelApp.Cells[5, 2].ColumnWidth = 8;
             ExcelApp.Cells[5, 3].Value = "Кат. номер"; ExcelApp.Cells[5, 3].ColumnWidth = 15;
             ExcelApp.Cells[5, 4].Value = "Товар"; ExcelApp.Cells[5, 4].ColumnWidth = 60;
             ExcelApp.Cells[5, 5].Value = "Производитель"; ExcelApp.Cells[5, 5].ColumnWidth = 30;
             ExcelApp.Cells[5, 6].Value = "Кол-во"; ExcelApp.Cells[5, 6].ColumnWidth = 15;
             ExcelApp.Cells[5, 7].Value = "Цена"; ExcelApp.Cells[5, 7].ColumnWidth = 15;
             ExcelApp.Cells[5, 8].Value = "Сумма"; ExcelApp.Cells[5, 8].ColumnWidth = 15;

             for (int i = 1; i < 8; i++)
             {
                 ExcelApp.Cells[5, i + 1].Font.Bold = true;
                 ExcelApp.Cells[5, i + 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                 ExcelApp.Cells[5, i + 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                 ExcelApp.Cells[5, i + 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
             }

             double total=0;
             for (int i = 0; i < dataGridViewBascet.Rows.Count; i++)
             {
                 ExcelApp.Cells[i + 6, 2].Value = i+1;
                 ExcelApp.Cells[i + 6, 3].Value = dataGridViewBascet.Rows[i].Cells["articleDataGridViewTextBoxColumn"].Value;
                 ExcelApp.Cells[i + 6, 4].Value = dataGridViewBascet.Rows[i].Cells["nameDataGridViewTextBoxColumn"].Value;
                 ExcelApp.Cells[i + 6, 5].Value = dataGridViewBascet.Rows[i].Cells["manufacDataGridViewTextBoxColumn"].Value;
                 ExcelApp.Cells[i + 6, 6].Value = dataGridViewBascet.Rows[i].Cells["qtyGridViewBascet"].Value;
                 ExcelApp.Cells[i + 6, 7].Value = Math.Round(Convert.ToDouble(dataGridViewBascet.Rows[i].Cells["priceGridViewBascet"].Value), 2);
                 ExcelApp.Cells[i + 6, 8].Value = Math.Round(Convert.ToDouble(dataGridViewBascet.Rows[i].Cells["totalGridViewBascet"].Value), 2);
                 total = total + Math.Round(Convert.ToDouble(dataGridViewBascet.Rows[i].Cells["totalGridViewBascet"].Value), 2);
                 for (int Q = 1; Q < 8; Q++)
                 {
                     ExcelApp.Cells[i + 6, Q + 1].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                     ExcelApp.Cells[i + 6, Q + 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                     ExcelApp.Cells[i + 6, Q + 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                 }
                 ExcelApp.Cells[i + 6, 4].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

             }

             ExcelApp.Cells[dataGridViewBascet.Rows.Count + 6, 8].Value = total;
             ExcelApp.Cells[dataGridViewBascet.Rows.Count + 6, 8].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
             ExcelApp.Cells[dataGridViewBascet.Rows.Count + 6, 8].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
             ExcelApp.Cells[dataGridViewBascet.Rows.Count + 6, 8].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

             //Вызываем нашу созданную эксельку.
             ExcelApp.Visible = true;
             ExcelApp.UserControl = true;  

         }

         private void Form1_Shown(object sender, EventArgs e)
         {
             //SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", ConnCe);
             //SqlCeDataReader reader = com_.ExecuteReader();
             //if (reader.Read())
             //{
             //    if (Convert.ToString(reader["Login"]) == "")
             //        firstStart();

             //    toolStripStatusUpdateData.Text = "Каталог товаров обновлен " + Convert.ToDateTime(reader["UpdateCatalogDate"]).ToString();
             //    toolStripStatusCustomerUpdateDate.Text = "Данные партнера обновлены " + Convert.ToDateTime(reader["UpdateDate"]).ToString();
             
             //    if (Convert.ToDateTime(reader["UpdateDate"]).Equals(new DateTime(2000, 1, 1)))
             //        fullUpdate();
             //}

         }

         TreeNode GetUpperParent(TreeNode node)
         {
             if (node.Level == 0) return node;
             else if (node.Parent.Level == 0) return node.Parent;
             else return GetUpperParent(node.Parent);
         }

         private void treeViewGroups_BeforeSelect(object sender, TreeViewCancelEventArgs e)
         {
             if (treeViewGroups.SelectedNode != null)
             {
                 if (treeViewGroups.SelectedNode != e.Node.Parent)
                 {
                     treeViewGroups.SelectedNode.Collapse();
                     if (e.Node.Parent != treeViewGroups.SelectedNode.Parent)
                         GetUpperParent(treeViewGroups.SelectedNode).Collapse();
                 }
             }

             e.Node.Expand();
         }

         private void treeViewGroups_AfterExpand(object sender, TreeViewEventArgs e)
         {
//             treeViewGroups.SelectedNode = e.Node;
//             treeViewGroups.SelectedNode.Expand();
         }

         private void textBoxName_KeyDown(object sender, KeyEventArgs e)
         {
             if (e.KeyCode == Keys.Enter && (treeViewGroups.SelectedNode.Nodes.Count == 0||checkBoxManufac.Checked))
             {
                 Delete_Filter();
                 if (textBoxName.Text.Trim().Length < 2)
                 {
                     checkBoxName.Checked = false;
                 }
                 else
                 {
                     checkBoxName.Checked = true;
                 }
                 SetFilterAuto();
         }
         }

         private void checkBoxArticle_Click(object sender, EventArgs e)
         {
             if (textBoxArticle.Text.Trim().Length < 4)
             {
                 checkBoxArticle.Checked = false;
             }
             SetFilterAuto();
         }

         private void checkBoxName_Click(object sender, EventArgs e)
         {
             if (textBoxName.Text.Trim().Length < 2)
             {
                 checkBoxName.Checked = false;
             }
             SetFilterAuto();
         }

         private void checkBoxManufac_Click(object sender, EventArgs e)
         {
             SetFilterAuto();
         }

         private void checkBoxNew1_Click(object sender, EventArgs e)
         {
             checkBoxNew.Checked = checkBoxNew1.Checked;
             if (checkBoxNew1.Checked)
             {
                 if (checkBoxAction.Checked)
                 {
                     checkBoxAction.Checked = false;
                     checkBoxAction1.Checked = false;
                 }
                 if (checkBoxReceipt.Checked)
                 {
                     checkBoxReceipt.Checked = false;
                     checkBoxReceipt1.Checked = false;
                 }
             }
             SetFilterAuto();
         }

         private void checkBoxAction1_Click(object sender, EventArgs e)
         {
             checkBoxAction.Checked = checkBoxAction1.Checked;
             if (checkBoxAction1.Checked)
             {
                 if (checkBoxNew.Checked)
                 {
                     checkBoxNew.Checked = false;
                     checkBoxNew1.Checked = false;
                 }
                 if (checkBoxReceipt.Checked)
                 {
                     checkBoxReceipt.Checked = false;
                     checkBoxReceipt1.Checked = false;
                 }
             }
             SetFilterAuto();

         }

         private void checkBoxReceipt1_Click(object sender, EventArgs e)
         {
             checkBoxReceipt.Checked = checkBoxReceipt1.Checked;
             if (checkBoxReceipt1.Checked)
             {
                 if (checkBoxNew.Checked)
                 {
                     checkBoxNew.Checked = false;
                     checkBoxNew1.Checked = false;
                 }
                 if (checkBoxReceipt.Checked)
                 {
                     checkBoxAction.Checked = false;
                     checkBoxAction1.Checked = false;
                 }
             }
             SetFilterAuto();

         }

         private void dataGridViewAnalogs_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
         {
             //if (e.RowIndex >= 0 && !dataGridViewGoods.Rows[e.RowIndex].IsNewRow)
             //{
             //    if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterNew"].Index].Value.ToString() == "1")
             //        dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(183, 212, 68);
             //    else if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterAction"].Index].Value.ToString() == "1")
             //        dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(252, 212, 4);
             //    else if (dataGridViewGoods.Rows[e.RowIndex].Cells[dataGridViewGoods.Columns["AvtoFilterReceipt"].Index].Value.ToString() == "1")
             //        dataGridViewGoods.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(173, 137, 148);
             //}
         }

         private void Form1_SizeChanged(object sender, EventArgs e)
         {
             this.Update();
         }

         TreeNode findInTree(TreeNodeCollection nodes, string name)
         {
             TreeNode foundNode;
             foreach (TreeNode node in nodes)
             {
                 if (node.Name.Equals(name))
                     return node;
                 else if (node.Nodes.Count>0)
                 {
                     foundNode = findInTree(node.Nodes, name);
                     if (foundNode != null)
                         return foundNode;
                 }
             }
             return null;
         }
 
        int findRowInGoods(string goodsId)
         {
             foreach (DataGridViewRow row in dataGridViewGoods.Rows)
                 if (row.Cells["AvtoID"].Value.ToString().Equals(goodsId))
                     return row.Index;
             return -1;
         }
 
        private void showAnalogInGoodsGrid(string goodsId)
         {
             
            int rowIndex = findRowInGoods( goodsId);
             if (rowIndex == -1)
             {
                 treeViewGroups.SelectedNode = null;
                 SqlCeCommand com_ = new SqlCeCommand("SELECT ParentId FROM Goods WHERE ID=@Id", ConnCe);
                 com_.Parameters.AddWithValue("@Id", goodsId);
                 SqlCeDataReader reader = com_.ExecuteReader();
                 if (reader.Read())
                 {
                     TreeNode node = findInTree(treeViewGroups.Nodes, reader["ParentId"].ToString());
                     if (node != null)
                     {
                         treeViewGroups.SelectedNode = node;
                         rowIndex = findRowInGoods(goodsId);
                     }

                 }
                 else
                     return;

             }
             if (rowIndex >= 0)
                 dataGridViewGoods.CurrentCell = dataGridViewGoods.Rows[rowIndex].Cells["AvtoNomName"]; 
//                dataGridViewGoods.Rows[rowIndex].Selected = true;

         }
    
         private void dataGridViewAnalogs_DoubleClick(object sender, EventArgs e)
         {
             string Id = dataGridViewAnalogs.Rows[dataGridViewAnalogs.CurrentRow.Index].Cells[dataGridViewAnalogs.Columns["AnalogID"].Index].Value.ToString();
            showAnalogInGoodsGrid(Id);
         }

         private void dataGridViewGoods_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
         {
             BuyGoods(dataGridViewGoods.CurrentRow.Index);
         }

         private void dataGridViewDolg_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
         {
             if (Convert.ToDouble(dataGridViewDolg.Rows[e.RowIndex].Cells["Prosrocheno"].Value) > 0.0)
             {
                 dataGridViewDolg.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.Red;
                 dataGridViewDolg.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
                 dataGridViewDolg.Rows[e.RowIndex].DefaultCellStyle.Font = new Font(dataGridViewDolg.Font, FontStyle.Bold);
             }
         }

         private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
         {
             SendMessage sendMessage = new SendMessage(this, "Бухгалтер");
             sendMessage.textBoxTopic.Text = "Сообщение об оплате";
             sendMessage.ShowDialog(this);
         }

         private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
         {
             SendMessage sendMessage = new SendMessage(this, "Менеджер");
             sendMessage.textBoxTopic.Text = "Сообщение об возврате";
             sendMessage.ShowDialog(this);
         }

         private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
         {
             SendMessage sendMessage = new SendMessage(this, "Менеджер");
             sendMessage.textBoxTopic.Text = "";
             sendMessage.ShowDialog(this);

         }

         private void comboBoxManufac_SelectionChangeCommitted(object sender, EventArgs e)
         {
             Delete_Filter();
             if (comboBoxManufac.SelectedIndex == 0)
                 checkBoxManufac.Checked = false;
             else
                 checkBoxManufac.Checked = true;

             SetFilterAuto();

         }

         private void importFromExcel()
         {
             OpenFileDialog opf = new OpenFileDialog();
             opf.Filter = "Файлы Excel (*.XLS;*.XLSX)|*.XLS;*.XLSX";
             if (opf.ShowDialog() == DialogResult.OK)
             {
                 ImportExcel importExcel = new ImportExcel(this, opf.FileName);
                 importExcel.Show();
             }

         }

         private void button3_Click(object sender, EventArgs e)
         {
             importFromExcel();
         }

         public void findArticle(string article, ImportExcel activeForm)
         {
             this.activeForm = activeForm;
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             textBoxArticle.Text = article;
             checkBoxArticle.Checked = true;
             catalogState = "Выбрать";
             SetFilterAuto();
             this.Activate();
             this.ActiveControl = dataGridViewGoods;
         }

         private void textBoxArticle_Enter(object sender, EventArgs e)
         {
             int a = 1;
             textBoxArticle.Focus();
         }

         private void textBoxArticle_Leave(object sender, EventArgs e)
         {
             int a = 1;
         }

         private void toolStripMenuItem1_Click(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
         }

         private void поискПоКодуToolStripMenuItem_Click(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             textBoxArticle.Focus();
         }

         private void поискПоИмениToolStripMenuItem_Click(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             textBoxName.Focus();
         }

         private void деревоЗапчастейToolStripMenuItem_Click(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             treeViewGroups.Focus();
         }

         private void аналогиToolStripMenuItem_Click(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             dataGridViewAnalogs.Focus();
         }

         private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
         {
             tabControlMain.SelectedTab = tabControlMain.TabPages["TabPageCatalog"];
             dataGridViewGoods.Focus();
         }

         private void treeViewGroups_KeyDown(object sender, KeyEventArgs e)
         {
             if (e.KeyCode == Keys.Enter)
             {
                 e.SuppressKeyPress = true;
                 dataGridViewGoods.Focus();
             }

         }

         private void фотоToolStripMenuItem_Click(object sender, EventArgs e)
         {
             if (dataGridViewGoods.CurrentRow.Index >= 0)
             {
                 string Id = dataGridViewGoods.Rows[dataGridViewGoods.CurrentRow.Index].Cells[dataGridViewGoods.Columns["AvtoID"].Index].Value.ToString();
                 string GoodsName = dataGridViewGoods.Rows[dataGridViewGoods.CurrentRow.Index].Cells[dataGridViewGoods.Columns["AvtoNomName"].Index].Value.ToString();

                 showGoodsImage(Id, GoodsName);

             }

         }

         private void помощьToolStripMenuItem_Click(object sender, EventArgs e)
         {
             string topic = "Catalog.html";
             HelpNavigator navigator = HelpNavigator.Topic;
             if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageCatalog"])
                 topic = "Catalog.html";
             else if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageInfo"])
                 topic = "GeneralInfo.html";
             else if (tabControlMain.SelectedTab == tabControlMain.TabPages["TabPageBasket"])
                 topic = "Basket.html";
             else if (tabControlMain.SelectedTab == tabControlMain.TabPages["tabPageOrder"])
                 topic = "Sales.html";
             else
                 topic = "GeneralInfo.html";

             Help.ShowHelp(this, "AVDClient.chm", navigator, topic);
         }

         private void содержаниеСправкиToolStripMenuItem_Click(object sender, EventArgs e)
         {
             Help.ShowHelp(this, "AVDClient.chm");
         }

       
    }
    
}
