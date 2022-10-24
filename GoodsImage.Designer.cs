namespace AVDClient
{
    partial class GoodsImage 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoodsImage));
            this.GoodsName = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.goodImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.goodImage)).BeginInit();
            this.SuspendLayout();
            // 
            // GoodsName
            // 
            this.GoodsName.AutoSize = true;
            this.GoodsName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.GoodsName.Location = new System.Drawing.Point(13, 9);
            this.GoodsName.Name = "GoodsName";
            this.GoodsName.Size = new System.Drawing.Size(43, 13);
            this.GoodsName.TabIndex = 1;
            this.GoodsName.Text = "Товар";
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.Image = global::AVDClient.Properties.Resources.БиблиотекаКартинок_ДобавитьВправо;
            this.button2.Location = new System.Drawing.Point(157, 272);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(36, 23);
            this.button2.TabIndex = 2;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Image = global::AVDClient.Properties.Resources.БиблиотекаКартинок_ДобавитьВлево;
            this.button1.Location = new System.Drawing.Point(115, 272);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(36, 23);
            this.button1.TabIndex = 2;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // goodImage
            // 
            this.goodImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.goodImage.Location = new System.Drawing.Point(12, 29);
            this.goodImage.Name = "goodImage";
            this.goodImage.Size = new System.Drawing.Size(288, 237);
            this.goodImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.goodImage.TabIndex = 0;
            this.goodImage.TabStop = false;
            this.goodImage.Click += new System.EventHandler(this.goodImage_Click);
            // 
            // GoodsImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(312, 296);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.GoodsName);
            this.Controls.Add(this.goodImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GoodsImage";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Фото товара";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.GoodsImage_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GoodsImage_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.goodImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.PictureBox goodImage;
        public System.Windows.Forms.Label GoodsName;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;


    }
}