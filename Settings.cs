using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Management;
using System.Security.Cryptography;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace AVDClient
{
    public partial class FormLogin : Form
    {
        public MainForm baseForm;
        public bool firstStart = false;
        public bool canClose = true;
 
        public FormLogin()
        {
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            SqlCeCommand com_ = new SqlCeCommand("SELECT * FROM constant", Data.ConnCe);
            SqlCeDataReader reader = com_.ExecuteReader();
            if (reader.Read())
            {
                textBoxUser.Text = Convert.ToString(reader["Login"]);
                textBoxPassword.Text = Convert.ToString(reader["Password"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool canClose = true;
            if (textBoxUser.Text.Trim() == "")
            {
                MessageBox.Show(this, "Не указано имя пользователя", "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (textBoxPassword.Text.Trim() == "")
            {
                MessageBox.Show(this, "Не указан пароль", "Ошибка", MessageBoxButtons.OK);
                return;
            }

            SqlCeCommand com_upd = new SqlCeCommand("UPDATE constant SET Login=@Login, Password=@Password", Data.ConnCe);

            com_upd.Parameters.AddWithValue("@Login", textBoxUser.Text);
            com_upd.Parameters.AddWithValue("@Password", textBoxPassword.Text);

            com_upd.ExecuteNonQuery();

            if (firstStart)
            {
                canClose = false;
                WebStore.WebStore cService = new WebStore.WebStore();
                WebStore.Customers customer = new WebStore.Customers();

                baseForm.AuthorizeService(cService, customer);

                string extIP = "";
                try
                {
                    WebClient client = new WebClient();
                    Stream data = client.OpenRead("http://whatismyip.org/");
                    StreamReader reader = new StreamReader(data);
                    extIP = reader.ReadLine();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                    //return;
                }
                
                try
                {
                    textBoxRegistrationInfo.Text = cService.AVDClientRegistrationIP(textKey.Text, extIP);
                    if (textBoxRegistrationInfo.Text.Equals("Программа зарегистрирована"))
                        canClose = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK);
                    return;
                }
            }

            if (canClose) 
                this.Close();
            else
                MessageBox.Show(this, "Программа не зарегистрирована. Работа с программой будет возможна после регистрации.", "Ошибка", MessageBoxButtons.OK);
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

        private void button2_Click(object sender, EventArgs e)
        {
            string A = "";
            A = A + GetComputerName();
            string B = VolumeSerialNumber();
            if (B.Equals(""))
                textKey.Text = "";
            else
                textKey.Text = GetHashString(A + B);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (textBoxUser.Text.Trim() == "")
            {
                MessageBox.Show("Не указано имя пользователя", "Ошибка", MessageBoxButtons.OK);
                return;
            }

            if (textBoxPassword.Text.Trim() == "")
            {
                MessageBox.Show("Не указан пароль", "Ошибка", MessageBoxButtons.OK);
                return;
            }

            SqlCeCommand com_upd = new SqlCeCommand("UPDATE constant SET Login=@Login, Password=@Password", Data.ConnCe);

            com_upd.Parameters.AddWithValue("@Login", textBoxUser.Text);
            com_upd.Parameters.AddWithValue("@Password", textBoxPassword.Text);

            com_upd.ExecuteNonQuery();

            WebStore.WebStore cService = new WebStore.WebStore();
            WebStore.Customers customer = new WebStore.Customers();
 
            baseForm.AuthorizeService(cService, customer);

            string extIP = "";
            try
            {
                WebClient client = new WebClient();
                Stream data = client.OpenRead("http://whatismyip.org/");
                StreamReader reader = new StreamReader(data);
                extIP = reader.ReadLine();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                //return;
            }

            try
            {
                textBoxRegistrationInfo.Text = cService.AVDClientRegistrationIP(textKey.Text, extIP);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }

        }

        private void FormLogin_Shown(object sender, EventArgs e)
        {
            textKey.Text = baseForm.GetRegistrationKey();
        }

        private void textBoxUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (firstStart && !textBoxRegistrationInfo.Text.Equals("Программа зарегистрирована"))
                canClose = false;
    
            this.Close();
 
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string topic = "Tune.html";
            HelpNavigator navigator = HelpNavigator.Topic;
            Help.ShowHelp(this, "AVDClient.chm", navigator, topic);
        }

     }
}
