namespace AVDClient
{
    partial class FormBasket
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewBascet = new System.Windows.Forms.DataGridView();
            this.articleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.manufacDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qtyGridViewBascet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceGridViewBascet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalGridViewBascet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.basketBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.myDatabaseDataSet = new AVDClient.MyDatabaseDataSet();
            this.basketTableAdapter = new AVDClient.MyDatabaseDataSetTableAdapters.basketTableAdapter();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBascet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.basketBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.myDatabaseDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewBascet
            // 
            this.dataGridViewBascet.AllowUserToAddRows = false;
            this.dataGridViewBascet.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewBascet.AutoGenerateColumns = false;
            this.dataGridViewBascet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBascet.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.articleDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.iDDataGridViewTextBoxColumn,
            this.manufacDataGridViewTextBoxColumn,
            this.qtyGridViewBascet,
            this.priceGridViewBascet,
            this.totalGridViewBascet});
            this.dataGridViewBascet.DataSource = this.basketBindingSource;
            this.dataGridViewBascet.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewBascet.Location = new System.Drawing.Point(3, 50);
            this.dataGridViewBascet.Name = "dataGridViewBascet";
            this.dataGridViewBascet.Size = new System.Drawing.Size(844, 345);
            this.dataGridViewBascet.TabIndex = 0;
            this.dataGridViewBascet.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewBascet_CellContentClick);
            this.dataGridViewBascet.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewBascet_CellEndEdit);
            // 
            // articleDataGridViewTextBoxColumn
            // 
            this.articleDataGridViewTextBoxColumn.DataPropertyName = "Article";
            this.articleDataGridViewTextBoxColumn.HeaderText = "Кат.номер";
            this.articleDataGridViewTextBoxColumn.Name = "articleDataGridViewTextBoxColumn";
            this.articleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Наименование";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.Width = 300;
            // 
            // iDDataGridViewTextBoxColumn
            // 
            this.iDDataGridViewTextBoxColumn.DataPropertyName = "ID";
            this.iDDataGridViewTextBoxColumn.HeaderText = "ID";
            this.iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
            this.iDDataGridViewTextBoxColumn.ReadOnly = true;
            this.iDDataGridViewTextBoxColumn.Visible = false;
            // 
            // manufacDataGridViewTextBoxColumn
            // 
            this.manufacDataGridViewTextBoxColumn.DataPropertyName = "Manufac";
            this.manufacDataGridViewTextBoxColumn.HeaderText = "Производитель";
            this.manufacDataGridViewTextBoxColumn.Name = "manufacDataGridViewTextBoxColumn";
            this.manufacDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // qtyGridViewBascet
            // 
            this.qtyGridViewBascet.DataPropertyName = "Qty";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            this.qtyGridViewBascet.DefaultCellStyle = dataGridViewCellStyle1;
            this.qtyGridViewBascet.HeaderText = "Количество";
            this.qtyGridViewBascet.Name = "qtyGridViewBascet";
            // 
            // priceGridViewBascet
            // 
            this.priceGridViewBascet.DataPropertyName = "Price";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle2.Format = "N2";
            dataGridViewCellStyle2.NullValue = null;
            this.priceGridViewBascet.DefaultCellStyle = dataGridViewCellStyle2;
            this.priceGridViewBascet.HeaderText = "Цена";
            this.priceGridViewBascet.Name = "priceGridViewBascet";
            this.priceGridViewBascet.ReadOnly = true;
            // 
            // totalGridViewBascet
            // 
            this.totalGridViewBascet.DataPropertyName = "Total";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle3.Format = "N2";
            dataGridViewCellStyle3.NullValue = null;
            this.totalGridViewBascet.DefaultCellStyle = dataGridViewCellStyle3;
            this.totalGridViewBascet.HeaderText = "Сумма";
            this.totalGridViewBascet.Name = "totalGridViewBascet";
            this.totalGridViewBascet.ReadOnly = true;
            // 
            // basketBindingSource
            // 
            this.basketBindingSource.DataMember = "basket";
            this.basketBindingSource.DataSource = this.myDatabaseDataSet;
            // 
            // myDatabaseDataSet
            // 
            this.myDatabaseDataSet.DataSetName = "MyDatabaseDataSet";
            this.myDatabaseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // basketTableAdapter
            // 
            this.basketTableAdapter.ClearBeforeFill = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(133, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "Добавить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(153, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(133, 30);
            this.button3.TabIndex = 2;
            this.button3.Text = "Очистить корзину";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(434, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(133, 30);
            this.button4.TabIndex = 2;
            this.button4.Text = "Закрыть корзину";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(296, 12);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(133, 30);
            this.button5.TabIndex = 2;
            this.button5.Text = "Отправить в обработку";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // FormBasket
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 404);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridViewBascet);
            this.Name = "FormBasket";
            this.Text = "Корзина";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBascet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.basketBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.myDatabaseDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewBascet;
        private MyDatabaseDataSet myDatabaseDataSet;
        private System.Windows.Forms.BindingSource basketBindingSource;
        private MyDatabaseDataSetTableAdapters.basketTableAdapter basketTableAdapter;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.DataGridViewTextBoxColumn articleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn manufacDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn qtyGridViewBascet;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceGridViewBascet;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalGridViewBascet;

    }
}