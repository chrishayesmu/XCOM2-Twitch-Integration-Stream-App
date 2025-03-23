using XComStreamApp.Controls;

namespace XComStreamApp
{
    partial class XComStreamAppForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XComStreamAppForm));
            label2 = new Label();
            label1 = new Label();
            label3 = new Label();
            btnLogInWithTwitch = new Button();
            imageList1 = new ImageList(components);
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            groupBox1 = new GroupBox();
            imgTwitchLoadingSpinner = new PictureBox();
            lblTwitchConnectionStatus = new Label();
            btnTwitchLogInOrOut = new Button();
            pnlLeftColumn = new Panel();
            txtSystemEvents = new RichTextBox();
            pnlMiddleColumn = new Panel();
            txtChatEvents = new RichTextBox();
            groupBox2 = new GroupBox();
            lblGameConnectionStatus = new Label();
            tabPage1.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) imgTwitchLoadingSpinner).BeginInit();
            pnlLeftColumn.SuspendLayout();
            pnlMiddleColumn.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            label2.Location = new Point(3, 144);
            label2.Name = "label2";
            label2.Size = new Size(107, 21);
            label2.TabIndex = 2;
            label2.Text = "Viewer events";
            // 
            // label1
            // 
            label1.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            label1.Location = new Point(9, 144);
            label1.Name = "label1";
            label1.Size = new Size(110, 21);
            label1.TabIndex = 1;
            label1.Text = "System events";
            // 
            // label3
            // 
            label3.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            label3.Location = new Point(-142, 3);
            label3.Name = "label3";
            label3.Size = new Size(661, 63);
            label3.TabIndex = 1;
            label3.Text = "You are not currently logged in to Twitch.\r\n\r\nUntil you connect your Twitch account, you cannot use the Twitch Integration app or extension.";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // btnLogInWithTwitch
            // 
            btnLogInWithTwitch.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            btnLogInWithTwitch.ImageAlign = ContentAlignment.MiddleLeft;
            btnLogInWithTwitch.ImageKey = "twitch_icon_flat.png";
            btnLogInWithTwitch.ImageList = imageList1;
            btnLogInWithTwitch.Location = new Point(290, 194);
            btnLogInWithTwitch.Name = "btnLogInWithTwitch";
            btnLogInWithTwitch.Padding = new Padding(6, 0, 0, 0);
            btnLogInWithTwitch.Size = new Size(181, 40);
            btnLogInWithTwitch.TabIndex = 0;
            btnLogInWithTwitch.Text = "Log In With Twitch";
            btnLogInWithTwitch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnLogInWithTwitch.UseVisualStyleBackColor = true;
            btnLogInWithTwitch.Click += btnLogInToTwitch_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer) resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "twitch_icon_flat.png");
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(btnLogInWithTwitch);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(808, 743);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(192, 72);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(192, 72);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "tabPage3";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(192, 72);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(imgTwitchLoadingSpinner);
            groupBox1.Controls.Add(lblTwitchConnectionStatus);
            groupBox1.Controls.Add(btnTwitchLogInOrOut);
            groupBox1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            groupBox1.Location = new Point(9, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(395, 136);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Twitch Connection";
            // 
            // imgTwitchLoadingSpinner
            // 
            imgTwitchLoadingSpinner.Image = (Image) resources.GetObject("imgTwitchLoadingSpinner.Image");
            imgTwitchLoadingSpinner.Location = new Point(43, 71);
            imgTwitchLoadingSpinner.Name = "imgTwitchLoadingSpinner";
            imgTwitchLoadingSpinner.Size = new Size(40, 40);
            imgTwitchLoadingSpinner.SizeMode = PictureBoxSizeMode.StretchImage;
            imgTwitchLoadingSpinner.TabIndex = 3;
            imgTwitchLoadingSpinner.TabStop = false;
            // 
            // lblTwitchConnectionStatus
            // 
            lblTwitchConnectionStatus.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTwitchConnectionStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point,  0);
            lblTwitchConnectionStatus.Location = new Point(6, 44);
            lblTwitchConnectionStatus.Name = "lblTwitchConnectionStatus";
            lblTwitchConnectionStatus.Size = new Size(392, 24);
            lblTwitchConnectionStatus.TabIndex = 1;
            lblTwitchConnectionStatus.Text = "You are not connected to Twitch services.";
            lblTwitchConnectionStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnTwitchLogInOrOut
            // 
            btnTwitchLogInOrOut.Anchor = AnchorStyles.None;
            btnTwitchLogInOrOut.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point,  0);
            btnTwitchLogInOrOut.ImageAlign = ContentAlignment.MiddleLeft;
            btnTwitchLogInOrOut.ImageKey = "twitch_icon_flat.png";
            btnTwitchLogInOrOut.ImageList = imageList1;
            btnTwitchLogInOrOut.Location = new Point(130, 78);
            btnTwitchLogInOrOut.Name = "btnTwitchLogInOrOut";
            btnTwitchLogInOrOut.Padding = new Padding(3, 0, 0, 0);
            btnTwitchLogInOrOut.Size = new Size(145, 30);
            btnTwitchLogInOrOut.TabIndex = 0;
            btnTwitchLogInOrOut.Text = "Log in to Twitch";
            btnTwitchLogInOrOut.TextAlign = ContentAlignment.MiddleRight;
            btnTwitchLogInOrOut.UseVisualStyleBackColor = true;
            // 
            // pnlLeftColumn
            // 
            pnlLeftColumn.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom;
            pnlLeftColumn.Controls.Add(txtSystemEvents);
            pnlLeftColumn.Controls.Add(groupBox1);
            pnlLeftColumn.Controls.Add(label1);
            pnlLeftColumn.Location = new Point(0, 0);
            pnlLeftColumn.Margin = new Padding(0);
            pnlLeftColumn.Name = "pnlLeftColumn";
            pnlLeftColumn.Size = new Size(413, 450);
            pnlLeftColumn.TabIndex = 8;
            // 
            // txtSystemEvents
            // 
            txtSystemEvents.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSystemEvents.DetectUrls = false;
            txtSystemEvents.Location = new Point(9, 168);
            txtSystemEvents.Name = "txtSystemEvents";
            txtSystemEvents.ReadOnly = true;
            txtSystemEvents.Size = new Size(398, 271);
            txtSystemEvents.TabIndex = 7;
            txtSystemEvents.Text = "[12:24:33 PM] Connected to Twitch services";
            txtSystemEvents.SelectionChanged += RichTextBox_SelectionChanged;
            // 
            // pnlMiddleColumn
            // 
            pnlMiddleColumn.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom;
            pnlMiddleColumn.Controls.Add(txtChatEvents);
            pnlMiddleColumn.Controls.Add(groupBox2);
            pnlMiddleColumn.Controls.Add(label2);
            pnlMiddleColumn.Location = new Point(413, 0);
            pnlMiddleColumn.Margin = new Padding(0);
            pnlMiddleColumn.Name = "pnlMiddleColumn";
            pnlMiddleColumn.Size = new Size(421, 450);
            pnlMiddleColumn.TabIndex = 9;
            // 
            // txtChatEvents
            // 
            txtChatEvents.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtChatEvents.DetectUrls = false;
            txtChatEvents.Enabled = false;
            txtChatEvents.Location = new Point(3, 168);
            txtChatEvents.Name = "txtChatEvents";
            txtChatEvents.ReadOnly = true;
            txtChatEvents.Size = new Size(398, 271);
            txtChatEvents.TabIndex = 8;
            txtChatEvents.Text = "[12:24:33 PM] SwfDelicious: !xsay I'm coming for you\n[12:25:07 PM] westonsammy: !rtd";
            // 
            // groupBox2
            // 
            groupBox2.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(lblGameConnectionStatus);
            groupBox2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point,  0);
            groupBox2.Location = new Point(8, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(401, 136);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "XCOM 2 Connection";
            // 
            // lblGameConnectionStatus
            // 
            lblGameConnectionStatus.Anchor =  AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblGameConnectionStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point,  0);
            lblGameConnectionStatus.Location = new Point(6, 44);
            lblGameConnectionStatus.Name = "lblGameConnectionStatus";
            lblGameConnectionStatus.Size = new Size(389, 24);
            lblGameConnectionStatus.TabIndex = 2;
            lblGameConnectionStatus.Text = "You are not connected to XCOM 2.";
            lblGameConnectionStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // XComStreamAppForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(834, 451);
            Controls.Add(pnlMiddleColumn);
            Controls.Add(pnlLeftColumn);
            MinimumSize = new Size(850, 490);
            Name = "XComStreamAppForm";
            Text = "XCOM 2 Twitch Integration";
            FormClosing += OnFormClosing;
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) imgTwitchLoadingSpinner).EndInit();
            pnlLeftColumn.ResumeLayout(false);
            pnlLeftColumn.PerformLayout();
            pnlMiddleColumn.ResumeLayout(false);
            pnlMiddleColumn.PerformLayout();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button btnLogInWithTwitch;
        private Label label2;
        private Label label1;
        private Label label3;
        private ImageList imageList1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabControlEx tabControl2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private GroupBox groupBox1;
        private Button btnTwitchLogInOrOut;
        private Label lblTwitchConnectionStatus;
        private Panel pnlLeftColumn;
        private Panel pnlMiddleColumn;
        private RichTextBox txtSystemEvents;
        private RichTextBox txtChatEvents;
        private PictureBox imgTwitchLoadingSpinner;
        private GroupBox groupBox2;
        private Label lblGameConnectionStatus;
    }
}
