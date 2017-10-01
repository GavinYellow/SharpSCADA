namespace TagConfig
{
    partial class ScaleParam
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdNone = new System.Windows.Forms.RadioButton();
            this.rdLine = new System.Windows.Forms.RadioButton();
            this.rdSqure = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.nmEUHI = new System.Windows.Forms.NumericUpDown();
            this.nmEULO = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nmRWHI = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nmRWLO = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmEUHI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmEULO)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmRWHI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmRWLO)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdNone);
            this.groupBox1.Controls.Add(this.rdLine);
            this.groupBox1.Controls.Add(this.rdSqure);
            this.groupBox1.Location = new System.Drawing.Point(2, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 40);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "类别";
            // 
            // rdNone
            // 
            this.rdNone.AutoSize = true;
            this.rdNone.Checked = true;
            this.rdNone.Location = new System.Drawing.Point(21, 13);
            this.rdNone.Name = "rdNone";
            this.rdNone.Size = new System.Drawing.Size(37, 17);
            this.rdNone.TabIndex = 2;
            this.rdNone.TabStop = true;
            this.rdNone.Text = "无";
            this.rdNone.UseVisualStyleBackColor = true;
            // 
            // rdLine
            // 
            this.rdLine.AutoSize = true;
            this.rdLine.Location = new System.Drawing.Point(74, 13);
            this.rdLine.Name = "rdLine";
            this.rdLine.Size = new System.Drawing.Size(49, 17);
            this.rdLine.TabIndex = 1;
            this.rdLine.Text = "线性";
            this.rdLine.UseVisualStyleBackColor = true;
            // 
            // rdSqure
            // 
            this.rdSqure.AutoSize = true;
            this.rdSqure.Location = new System.Drawing.Point(139, 13);
            this.rdSqure.Name = "rdSqure";
            this.rdSqure.Size = new System.Drawing.Size(61, 17);
            this.rdSqure.TabIndex = 0;
            this.rdSqure.Text = "平方根";
            this.rdSqure.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "量程上限（EUHI)";
            // 
            // nmEUHI
            // 
            this.nmEUHI.Location = new System.Drawing.Point(12, 69);
            this.nmEUHI.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nmEUHI.Name = "nmEUHI";
            this.nmEUHI.Size = new System.Drawing.Size(80, 20);
            this.nmEUHI.TabIndex = 2;
            // 
            // nmEULO
            // 
            this.nmEULO.Location = new System.Drawing.Point(121, 69);
            this.nmEULO.Name = "nmEULO";
            this.nmEULO.Size = new System.Drawing.Size(80, 20);
            this.nmEULO.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(116, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "量程下限（EULO)";
            // 
            // nmRWHI
            // 
            this.nmRWHI.Location = new System.Drawing.Point(12, 117);
            this.nmRWHI.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nmRWHI.Name = "nmRWHI";
            this.nmRWHI.Size = new System.Drawing.Size(80, 20);
            this.nmRWHI.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "原值上限（RWHI)";
            // 
            // nmRWLO
            // 
            this.nmRWLO.Location = new System.Drawing.Point(121, 117);
            this.nmRWLO.Name = "nmRWLO";
            this.nmRWLO.Size = new System.Drawing.Size(80, 20);
            this.nmRWLO.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(118, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "原值上限（RWLO)";
            // 
            // ScaleParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(212, 156);
            this.Controls.Add(this.nmRWLO);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nmRWHI);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nmEULO);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nmEUHI);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScaleParam";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "量程设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScaleParam_FormClosing);
            this.Load += new System.EventHandler(this.ScaleParam_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmEUHI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmEULO)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmRWHI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmRWLO)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdLine;
        private System.Windows.Forms.RadioButton rdSqure;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nmEUHI;
        private System.Windows.Forms.NumericUpDown nmEULO;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nmRWHI;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nmRWLO;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rdNone;
    }
}