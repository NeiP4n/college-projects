namespace BdManga
{
    partial class Base
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Base));
            ListViewItem listViewItem1 = new ListViewItem("");
            cuiTabControl1 = new CuoreUI.Controls.cuiTabControl();
            tabPage1 = new TabPage();
            flowLayoutPanel2 = new FlowLayoutPanel();
            tabPage2 = new TabPage();
            Searchtextbox = new TextBox();
            treeViewGenres = new TreeView();
            flowLayoutPanelManga = new FlowLayoutPanel();
            tabPage3 = new TabPage();
            cuiButtonexit = new CuoreUI.Controls.cuiButton();
            SetChapterButton = new CuoreUI.Controls.cuiButton();
            labelname = new CuoreUI.Controls.cuiLabel();
            cuiPictureBox3 = new CuoreUI.Controls.cuiPictureBox();
            listView2 = new ListView();
            listView1 = new ListView();
            imageList1 = new ImageList(components);
            cuiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            SuspendLayout();
            // 
            // cuiTabControl1
            // 
            cuiTabControl1.AddButtonBackgroundColor = Color.White;
            cuiTabControl1.AllowNoTabs = false;
            cuiTabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            cuiTabControl1.Appearance = TabAppearance.Buttons;
            cuiTabControl1.BackgroundColor = Color.FromArgb(10, 10, 10);
            cuiTabControl1.Controls.Add(tabPage1);
            cuiTabControl1.Controls.Add(tabPage2);
            cuiTabControl1.Controls.Add(tabPage3);
            cuiTabControl1.DeletionColor = Color.White;
            cuiTabControl1.DeletionTabBackgroundColor = Color.Crimson;
            cuiTabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            cuiTabControl1.HoveredTabBackColor = Color.FromArgb(38, 38, 38);
            cuiTabControl1.HoveredTabTextBackColor = Color.FromArgb(92, 92, 92);
            cuiTabControl1.ItemSize = new Size(126, 42);
            cuiTabControl1.Location = new Point(0, 0);
            cuiTabControl1.Name = "cuiTabControl1";
            cuiTabControl1.Rounding = 8;
            cuiTabControl1.SelectedIndex = 0;
            cuiTabControl1.SelectedTabBackColor = Color.FromArgb(255, 106, 0);
            cuiTabControl1.SelectedTabTextBackColor = Color.White;
            cuiTabControl1.Size = new Size(1002, 611);
            cuiTabControl1.SizeMode = TabSizeMode.Fixed;
            cuiTabControl1.TabIndex = 0;
            cuiTabControl1.tabNamingConvention = "tabPage";
            cuiTabControl1.UnselectedTabBackColor = Color.FromArgb(24, 24, 24);
            cuiTabControl1.UnselectedTabTextBackColor = Color.FromArgb(64, 64, 64);
            // 
            // tabPage1
            // 
            tabPage1.AutoScroll = true;
            tabPage1.BackColor = SystemColors.ActiveCaptionText;
            tabPage1.Controls.Add(flowLayoutPanel2);
            tabPage1.Location = new Point(4, 46);
            tabPage1.Margin = new Padding(10);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(10);
            tabPage1.Size = new Size(994, 561);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Избранное";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Location = new Point(8, 8);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(978, 471);
            flowLayoutPanel2.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.BackColor = SystemColors.ActiveCaptionText;
            tabPage2.Controls.Add(Searchtextbox);
            tabPage2.Controls.Add(treeViewGenres);
            tabPage2.Controls.Add(flowLayoutPanelManga);
            tabPage2.Location = new Point(4, 46);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(994, 561);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Каталог";
            // 
            // Searchtextbox
            // 
            Searchtextbox.Location = new Point(333, 19);
            Searchtextbox.Name = "Searchtextbox";
            Searchtextbox.Size = new Size(287, 27);
            Searchtextbox.TabIndex = 8;
            Searchtextbox.Text = "Поиск...";
            Searchtextbox.TextChanged += Searchtextbox_TextChanged;
            // 
            // treeViewGenres
            // 
            treeViewGenres.BackColor = SystemColors.InfoText;
            treeViewGenres.Dock = DockStyle.Right;
            treeViewGenres.ForeColor = SystemColors.Info;
            treeViewGenres.LineColor = Color.DimGray;
            treeViewGenres.Location = new Point(778, 3);
            treeViewGenres.Name = "treeViewGenres";
            treeViewGenres.Size = new Size(213, 555);
            treeViewGenres.TabIndex = 0;
            // 
            // flowLayoutPanelManga
            // 
            flowLayoutPanelManga.Location = new Point(3, 67);
            flowLayoutPanelManga.Name = "flowLayoutPanelManga";
            flowLayoutPanelManga.Size = new Size(748, 491);
            flowLayoutPanelManga.TabIndex = 7;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = SystemColors.ActiveCaptionText;
            tabPage3.Controls.Add(cuiButtonexit);
            tabPage3.Controls.Add(SetChapterButton);
            tabPage3.Controls.Add(labelname);
            tabPage3.Controls.Add(cuiPictureBox3);
            tabPage3.Controls.Add(listView2);
            tabPage3.Controls.Add(listView1);
            tabPage3.Location = new Point(4, 46);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(994, 561);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Профиль";
            // 
            // cuiButtonexit
            // 
            cuiButtonexit.CheckButton = false;
            cuiButtonexit.Checked = false;
            cuiButtonexit.CheckedBackground = Color.FromArgb(64, 64, 64);
            cuiButtonexit.CheckedForeColor = Color.DimGray;
            cuiButtonexit.CheckedImageTint = Color.Black;
            cuiButtonexit.CheckedOutline = Color.Black;
            cuiButtonexit.Content = "Выход из приложения";
            cuiButtonexit.DialogResult = DialogResult.None;
            cuiButtonexit.Font = new Font("Microsoft Sans Serif", 9.75F);
            cuiButtonexit.ForeColor = Color.White;
            cuiButtonexit.HoverBackground = Color.Black;
            cuiButtonexit.HoveredImageTint = Color.White;
            cuiButtonexit.HoverForeColor = Color.White;
            cuiButtonexit.HoverOutline = Color.Empty;
            cuiButtonexit.Image = null;
            cuiButtonexit.ImageAutoCenter = true;
            cuiButtonexit.ImageExpand = new Point(0, 0);
            cuiButtonexit.ImageOffset = new Point(0, 0);
            cuiButtonexit.ImageTint = Color.White;
            cuiButtonexit.Location = new Point(795, 3);
            cuiButtonexit.Name = "cuiButtonexit";
            cuiButtonexit.NormalBackground = Color.Black;
            cuiButtonexit.NormalOutline = Color.Empty;
            cuiButtonexit.OutlineThickness = 1.6F;
            cuiButtonexit.PressedBackground = Color.FromArgb(64, 0, 0);
            cuiButtonexit.PressedForeColor = Color.White;
            cuiButtonexit.PressedImageTint = Color.White;
            cuiButtonexit.PressedOutline = Color.Empty;
            cuiButtonexit.Rounding = new Padding(8);
            cuiButtonexit.Size = new Size(191, 56);
            cuiButtonexit.TabIndex = 7;
            cuiButtonexit.TextOffset = new Point(0, 0);
            cuiButtonexit.Click += cuiButtonexit_Click;
            // 
            // SetChapterButton
            // 
            SetChapterButton.CheckButton = false;
            SetChapterButton.Checked = false;
            SetChapterButton.CheckedBackground = Color.FromArgb(64, 64, 64);
            SetChapterButton.CheckedForeColor = Color.DimGray;
            SetChapterButton.CheckedImageTint = Color.Black;
            SetChapterButton.CheckedOutline = Color.Black;
            SetChapterButton.Content = "Установить главу";
            SetChapterButton.DialogResult = DialogResult.None;
            SetChapterButton.Font = new Font("Microsoft Sans Serif", 9.75F);
            SetChapterButton.ForeColor = Color.White;
            SetChapterButton.HoverBackground = Color.Black;
            SetChapterButton.HoveredImageTint = Color.White;
            SetChapterButton.HoverForeColor = Color.White;
            SetChapterButton.HoverOutline = Color.Empty;
            SetChapterButton.Image = null;
            SetChapterButton.ImageAutoCenter = true;
            SetChapterButton.ImageExpand = new Point(0, 0);
            SetChapterButton.ImageOffset = new Point(0, 0);
            SetChapterButton.ImageTint = Color.White;
            SetChapterButton.Location = new Point(772, 215);
            SetChapterButton.Name = "SetChapterButton";
            SetChapterButton.NormalBackground = Color.Black;
            SetChapterButton.NormalOutline = Color.Empty;
            SetChapterButton.OutlineThickness = 1.6F;
            SetChapterButton.PressedBackground = Color.FromArgb(64, 0, 0);
            SetChapterButton.PressedForeColor = Color.White;
            SetChapterButton.PressedImageTint = Color.White;
            SetChapterButton.PressedOutline = Color.Empty;
            SetChapterButton.Rounding = new Padding(8);
            SetChapterButton.Size = new Size(191, 56);
            SetChapterButton.TabIndex = 6;
            SetChapterButton.TextOffset = new Point(0, 0);
            SetChapterButton.Click += SetChapterButton_Click;
            // 
            // labelname
            // 
            labelname.Content = "Profile_Name";
            labelname.Font = new Font("Segoe UI", 16F);
            labelname.ForeColor = SystemColors.Control;
            labelname.HorizontalAlignment = CuoreUI.Controls.cuiLabel.HorizontalAlignments.Center;
            labelname.Location = new Point(298, 196);
            labelname.Margin = new Padding(4, 5, 4, 5);
            labelname.Name = "labelname";
            labelname.Size = new Size(350, 39);
            labelname.TabIndex = 5;
            // 
            // cuiPictureBox3
            // 
            cuiPictureBox3.Content = (Image)resources.GetObject("cuiPictureBox3.Content");
            cuiPictureBox3.CornerRadius = 8;
            cuiPictureBox3.ImageTint = Color.White;
            cuiPictureBox3.Location = new Point(399, 51);
            cuiPictureBox3.Margin = new Padding(4, 5, 4, 5);
            cuiPictureBox3.Name = "cuiPictureBox3";
            cuiPictureBox3.OutlineThickness = 1F;
            cuiPictureBox3.PanelOutlineColor = Color.Empty;
            cuiPictureBox3.Rotation = 0;
            cuiPictureBox3.Size = new Size(143, 135);
            cuiPictureBox3.TabIndex = 0;
            // 
            // listView2
            // 
            listView2.BackColor = SystemColors.InactiveCaptionText;
            listView2.Dock = DockStyle.Fill;
            listView2.Font = new Font("Segoe UI", 16F);
            listView2.ForeColor = SystemColors.InactiveBorder;
            listView2.Items.AddRange(new ListViewItem[] { listViewItem1 });
            listView2.Location = new Point(0, 296);
            listView2.Name = "listView2";
            listView2.Size = new Size(994, 265);
            listView2.TabIndex = 4;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // listView1
            // 
            listView1.BackColor = SystemColors.ActiveCaptionText;
            listView1.Dock = DockStyle.Top;
            listView1.Location = new Point(0, 0);
            listView1.Name = "listView1";
            listView1.Size = new Size(994, 296);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // Base
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1002, 611);
            Controls.Add(cuiTabControl1);
            Name = "Base";
            Text = "Base";
            cuiTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private CuoreUI.Controls.cuiTabControl cuiTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private CuoreUI.Controls.cuiPictureBox cuiPictureBox3;
        private ImageList imageList1;
        private FlowLayoutPanel flowLayoutPanelManga;
        private TreeView treeViewGenres;
        private FlowLayoutPanel flowLayoutPanel2;
        private TextBox Searchtextbox;
        private ListView listView1;
        private ListView listView2;
        private CuoreUI.Controls.cuiLabel labelname;
        private CuoreUI.Controls.cuiButton SetChapterButton;
        private CuoreUI.Controls.cuiButton cuiButtonexit;
    }
}