namespace ZoryaLIS
{
    partial class lisHome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(lisHome));
            tabLIs = new TabControl();
            tabBS430 = new TabPage();
            tabBS240E = new TabPage();
            tabBS430i = new TabPage();
            tabZybioZ3 = new TabPage();
            tabZybioZ50 = new TabPage();
            tabX350 = new TabPage();
            menuStrip1 = new MenuStrip();
            homeToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutUsToolStripMenuItem = new ToolStripMenuItem();
            
            tabLIs.SuspendLayout();
            tabBS430.SuspendLayout();
            tabBS240E.SuspendLayout();
            tabBS430i.SuspendLayout();
            tabZybioZ3.SuspendLayout();
            tabZybioZ50.SuspendLayout();
            tabX350.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabLIs
            // 
            tabLIs.Controls.Add(tabBS430);
            tabLIs.Controls.Add(tabBS240E);
            tabLIs.Controls.Add(tabBS430i);
            tabLIs.Controls.Add(tabZybioZ3);
            tabLIs.Controls.Add(tabZybioZ50);
            tabLIs.Controls.Add(tabX350);
            tabLIs.Dock = DockStyle.Fill;
            tabLIs.Location = new Point(0, 24);
            tabLIs.Name = "tabLIs";
            tabLIs.SelectedIndex = 0;
            tabLIs.Size = new Size(800, 426);
            tabLIs.TabIndex = 0;
            // 
            // tabBS430
            // 

            tabBS430.Location = new Point(4, 24);
            tabBS430.Name = "tabBS430";
            tabBS430.Padding = new Padding(3);
            tabBS430.Size = new Size(792, 398);
            tabBS430.TabIndex = 0;
            tabBS430.Text = "Mindray BS430";
            tabBS430.UseVisualStyleBackColor = true;
            // 
            // tabBS240E
            // 
            tabBS240E.Controls.Add(bS240e1);
            tabBS240E.Location = new Point(4, 24);
            tabBS240E.Name = "tabBS240E";
            tabBS240E.Padding = new Padding(3);
            tabBS240E.Size = new Size(792, 398);
            tabBS240E.TabIndex = 1;
            tabBS240E.Text = "Mindray BS240E";
            tabBS240E.UseVisualStyleBackColor = true;
            // 
            // tabBS430i
            // 
            tabBS430i.Location = new Point(4, 24);
            tabBS430i.Name = "tabBS430i";
            tabBS430i.Size = new Size(792, 398);
            tabBS430i.TabIndex = 2;
            tabBS430i.Text = "Mindray CL1200i";
            tabBS430i.UseVisualStyleBackColor = true;
            // 
            // tabZybioZ3
            // 
            tabZybioZ3.Location = new Point(4, 24);
            tabZybioZ3.Name = "tabZybioZ3";
            tabZybioZ3.Size = new Size(792, 398);
            tabZybioZ3.TabIndex = 3;
            tabZybioZ3.Text = "Zybio Z3";
            tabZybioZ3.UseVisualStyleBackColor = true;
            // 
            // tabZybioZ50
            // 
            tabZybioZ50.Location = new Point(4, 24);
            tabZybioZ50.Name = "tabZybioZ50";
            tabZybioZ50.Size = new Size(792, 398);
            tabZybioZ50.TabIndex = 4;
            tabZybioZ50.Text = "Zybio Z50";
            tabZybioZ50.UseVisualStyleBackColor = true;
            // 
            // tabX350
            // 
            tabX350.Location = new Point(4, 24);
            tabX350.Name = "tabX350";
            tabX350.Size = new Size(792, 398);
            tabX350.TabIndex = 5;
            tabX350.Text = "SysMax X350";
            tabX350.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { homeToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { closeToolStripMenuItem });
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.Size = new Size(52, 20);
            homeToolStripMenuItem.Text = "&Home";
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(103, 22);
            closeToolStripMenuItem.Text = "&Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutUsToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutUsToolStripMenuItem
            // 
            aboutUsToolStripMenuItem.Name = "aboutUsToolStripMenuItem";
            aboutUsToolStripMenuItem.Size = new Size(123, 22);
            aboutUsToolStripMenuItem.Text = "&About Us";
            aboutUsToolStripMenuItem.Click += aboutUsToolStripMenuItem_Click;
            
            // 
            // bS240e1
            // 
            bS240e1.Dock = DockStyle.Fill;
            bS240e1.Location = new Point(3, 3);
            bS240e1.Name = "bS240e1";
            bS240e1.Size = new Size(786, 392);
            bS240e1.TabIndex = 0;
            // 
            // lisHome
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabLIs);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "lisHome";
            Text = "Zorya LIS";
            tabLIs.ResumeLayout(false);
            tabBS430.ResumeLayout(false);
            tabBS240E.ResumeLayout(false);
            tabBS430i.ResumeLayout(false);
            tabZybioZ3.ResumeLayout(false);
            tabZybioZ50.ResumeLayout(false);
            tabX350.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabLIs;
        private TabPage tabBS430;
        private TabPage tabBS240E;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem homeToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutUsToolStripMenuItem;
        private TabPage tabBS430i;
        private TabPage tabZybioZ3;
        private TabPage tabZybioZ50;
        private TabPage tabX350;
        private Controllers.BS240E bS240e1;
        
    }
}
