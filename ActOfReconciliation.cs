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
    public partial class FormAktSverki : Form
    {
        public FormAktSverki()
        {
            InitializeComponent();
        }

        private void FormAktSverki_Load(object sender, EventArgs e)
        {

        }

        private void dataGridViewAktSverki_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
        }

        private void dataGridViewAktSverki_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {

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

            ExcelApp.Cells[2, 3] = label1.Text;
            ExcelApp.Cells[2, 3].Font.Bold = true;
            
            ExcelApp.Cells[3, 3] = labelPeriod.Text;
            ExcelApp.Cells[3, 3].Font.Bold = true;

            ExcelApp.Cells[5, 2].Value = "Дата"; ExcelApp.Cells[5, 2].ColumnWidth = 10;
            ExcelApp.Cells[5, 3].Value = "Номер"; ExcelApp.Cells[5, 3].ColumnWidth = 15;
            ExcelApp.Cells[5, 4].Value = "Документ"; ExcelApp.Cells[5, 4].ColumnWidth = 60;
            ExcelApp.Cells[5, 5].Value = "Отгрузка"; ExcelApp.Cells[5, 6].ColumnWidth = 15;
            ExcelApp.Cells[5, 6].Value = "Оплата"; ExcelApp.Cells[5, 7].ColumnWidth = 15;
 
            for (int i = 1; i < 7; i++)
            {
                ExcelApp.Cells[5, i + 1].Font.Bold = true;
                ExcelApp.Cells[5, i + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                ExcelApp.Cells[5, i + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                ExcelApp.Cells[5, i + 1].VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
            }

            for (int i = 0; i < dataGridViewAktSverki.Rows.Count; i++)
            {
                ExcelApp.Cells[i + 6, 2].Value = dataGridViewAktSverki.Rows[i].Cells["Date"].Value;
                ExcelApp.Cells[i + 6, 3].Value = dataGridViewAktSverki.Rows[i].Cells["Number"].Value;
                ExcelApp.Cells[i + 6, 4].Value = dataGridViewAktSverki.Rows[i].Cells["Document"].Value;
                ExcelApp.Cells[i + 6, 5].Value = Math.Round(Convert.ToDouble(dataGridViewAktSverki.Rows[i].Cells["shipping"].Value), 2);
                ExcelApp.Cells[i + 6, 6].Value = Math.Round(Convert.ToDouble(dataGridViewAktSverki.Rows[i].Cells["Payment"].Value), 2);
                for (int Q = 1; Q < 7; Q++)
                {
                    ExcelApp.Cells[i + 6, Q + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    ExcelApp.Cells[i + 6, Q + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    ExcelApp.Cells[i + 6, Q + 1].VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                }
                ExcelApp.Cells[i + 6, 4].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;

            }

           //Вызываем нашу созданную эксельку.
            ExcelApp.Visible = true;
            ExcelApp.UserControl = true;  

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string topic = "AktSverki.html";
            HelpNavigator navigator = HelpNavigator.Topic;
            Help.ShowHelp(this, "AVDClient.chm", navigator, topic);
        }
    }
}
