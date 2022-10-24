using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace AVDClient
{
    static class Program
    {

        
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

        }
        
	
  }
    static class Data
    {
        public static string Id { get; set; }
        public static SqlCeConnection ConnCe { get; set; }
        public static DateTime DateFrom { get; set; }
        public static DateTime DateFor { get; set; }
        public static Boolean FormAktSverki { get; set; }

    }  


    
}
