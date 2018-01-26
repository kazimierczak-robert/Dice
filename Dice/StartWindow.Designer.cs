namespace DiceClient
{
    partial class StartWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartWindow));
            this.LName = new System.Windows.Forms.Label();
            this.TBServer = new System.Windows.Forms.TextBox();
            this.BConnect = new System.Windows.Forms.Button();
            this.TBLogin = new System.Windows.Forms.TextBox();
            this.BCreate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CBIPAddress = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PBLogo = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // LName
            // 
            this.LName.AutoSize = true;
            this.LName.Location = new System.Drawing.Point(65, 118);
            this.LName.Name = "LName";
            this.LName.Size = new System.Drawing.Size(29, 13);
            this.LName.TabIndex = 1;
            this.LName.Text = "Dice";
            // 
            // TBServer
            // 
            this.TBServer.ForeColor = System.Drawing.Color.Gray;
            this.TBServer.Location = new System.Drawing.Point(9, 12);
            this.TBServer.MaxLength = 15;
            this.TBServer.Name = "TBServer";
            this.TBServer.Size = new System.Drawing.Size(103, 20);
            this.TBServer.TabIndex = 3;
            this.TBServer.Text = "Adres IPv4 serwera";
            this.TBServer.Enter += new System.EventHandler(this.Textbox_Enter);
            this.TBServer.Leave += new System.EventHandler(this.Textbox_Leave);
            // 
            // BConnect
            // 
            this.BConnect.Location = new System.Drawing.Point(8, 39);
            this.BConnect.Name = "BConnect";
            this.BConnect.Size = new System.Drawing.Size(105, 23);
            this.BConnect.TabIndex = 4;
            this.BConnect.Text = "Połącz z serwerem";
            this.BConnect.UseVisualStyleBackColor = true;
            this.BConnect.Click += new System.EventHandler(this.BConnect_Click);
            // 
            // TBLogin
            // 
            this.TBLogin.ForeColor = System.Drawing.Color.Gray;
            this.TBLogin.Location = new System.Drawing.Point(30, 142);
            this.TBLogin.MaxLength = 13;
            this.TBLogin.Name = "TBLogin";
            this.TBLogin.Size = new System.Drawing.Size(104, 20);
            this.TBLogin.TabIndex = 2;
            this.TBLogin.Text = "Login";
            this.TBLogin.Enter += new System.EventHandler(this.Textbox_Enter);
            this.TBLogin.Leave += new System.EventHandler(this.Textbox_Leave);
            // 
            // BCreate
            // 
            this.BCreate.Location = new System.Drawing.Point(8, 39);
            this.BCreate.Name = "BCreate";
            this.BCreate.Size = new System.Drawing.Size(105, 23);
            this.BCreate.TabIndex = 5;
            this.BCreate.Text = "Utwórz grę";
            this.BCreate.UseVisualStyleBackColor = true;
            this.BCreate.Click += new System.EventHandler(this.BCreate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 239);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Lub";
            // 
            // CBIPAddress
            // 
            this.CBIPAddress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBIPAddress.FormattingEnabled = true;
            this.CBIPAddress.Location = new System.Drawing.Point(9, 12);
            this.CBIPAddress.Name = "CBIPAddress";
            this.CBIPAddress.Size = new System.Drawing.Size(103, 21);
            this.CBIPAddress.TabIndex = 7;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TBServer);
            this.groupBox1.Controls.Add(this.BConnect);
            this.groupBox1.Location = new System.Drawing.Point(21, 167);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(120, 69);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CBIPAddress);
            this.groupBox2.Controls.Add(this.BCreate);
            this.groupBox2.Location = new System.Drawing.Point(21, 251);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(120, 69);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            // 
            // PBLogo
            // 
            this.PBLogo.Image = global::DiceClient.Properties.Resources.diceLogo;
            this.PBLogo.Location = new System.Drawing.Point(17, 3);
            this.PBLogo.Name = "PBLogo";
            this.PBLogo.Size = new System.Drawing.Size(128, 128);
            this.PBLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PBLogo.TabIndex = 0;
            this.PBLogo.TabStop = false;
            // 
            // StartWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(162, 331);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TBLogin);
            this.Controls.Add(this.LName);
            this.Controls.Add(this.PBLogo);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StartWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dice";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PBLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox PBLogo;
        private System.Windows.Forms.Label LName;
        private System.Windows.Forms.TextBox TBServer;
        private System.Windows.Forms.Button BConnect;
        private System.Windows.Forms.TextBox TBLogin;
        private System.Windows.Forms.Button BCreate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CBIPAddress;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

