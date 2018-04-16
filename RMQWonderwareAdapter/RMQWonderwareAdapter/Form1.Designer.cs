﻿namespace RMQWonderwareAdapter
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewTags = new System.Windows.Forms.DataGridView();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.timerFlushTextboxes = new System.Windows.Forms.Timer(this.components);
            this.timerFlushLogs = new System.Windows.Forms.Timer(this.components);
            this.timerCloseLogFiles = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSubscriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.removeAllSubscriptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeValueToTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerDeadmanSwitch = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTags)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 4;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelMain.Controls.Add(this.labelStatus, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 2;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1261, 466);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.labelStatus, 4);
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatus.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(3, 419);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(1255, 47);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Status: OK";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            this.tableLayoutPanelMain.SetColumnSpan(this.splitContainer1, 4);
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridViewTags);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainer1.Size = new System.Drawing.Size(1255, 413);
            this.splitContainer1.SplitterDistance = 549;
            this.splitContainer1.TabIndex = 4;
            // 
            // dataGridViewTags
            // 
            this.dataGridViewTags.AllowUserToAddRows = false;
            this.dataGridViewTags.AllowUserToDeleteRows = false;
            this.dataGridViewTags.AllowUserToResizeColumns = false;
            this.dataGridViewTags.AllowUserToResizeRows = false;
            this.dataGridViewTags.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewTags.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewTags.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewTags.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewTags.Name = "dataGridViewTags";
            this.dataGridViewTags.ReadOnly = true;
            this.dataGridViewTags.RowHeadersVisible = false;
            this.dataGridViewTags.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTags.ShowEditingIcon = false;
            this.dataGridViewTags.Size = new System.Drawing.Size(549, 413);
            this.dataGridViewTags.TabIndex = 3;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(702, 413);
            this.textBoxLog.TabIndex = 0;
            // 
            // timerFlushTextboxes
            // 
            this.timerFlushTextboxes.Enabled = true;
            this.timerFlushTextboxes.Interval = 1000;
            this.timerFlushTextboxes.Tick += new System.EventHandler(this.timerFlushTextboxes_Tick);
            // 
            // timerFlushLogs
            // 
            this.timerFlushLogs.Enabled = true;
            this.timerFlushLogs.Interval = 5000;
            this.timerFlushLogs.Tick += new System.EventHandler(this.timerFlushLogs_Tick);
            // 
            // timerCloseLogFiles
            // 
            this.timerCloseLogFiles.Enabled = true;
            this.timerCloseLogFiles.Interval = 86400000;
            this.timerCloseLogFiles.Tick += new System.EventHandler(this.timerCloseLogFiles_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.diagnosticsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1261, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // diagnosticsToolStripMenuItem
            // 
            this.diagnosticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSubscriptionToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeAllSubscriptionsToolStripMenuItem,
            this.writeValueToTagToolStripMenuItem});
            this.diagnosticsToolStripMenuItem.Name = "diagnosticsToolStripMenuItem";
            this.diagnosticsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.diagnosticsToolStripMenuItem.Text = "&Diagnostics";
            // 
            // addSubscriptionToolStripMenuItem
            // 
            this.addSubscriptionToolStripMenuItem.Name = "addSubscriptionToolStripMenuItem";
            this.addSubscriptionToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.addSubscriptionToolStripMenuItem.Text = "&Add Subscription";
            this.addSubscriptionToolStripMenuItem.Click += new System.EventHandler(this.addSubscriptionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
            // 
            // removeAllSubscriptionsToolStripMenuItem
            // 
            this.removeAllSubscriptionsToolStripMenuItem.Name = "removeAllSubscriptionsToolStripMenuItem";
            this.removeAllSubscriptionsToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.removeAllSubscriptionsToolStripMenuItem.Text = "&Remove all subscriptions";
            this.removeAllSubscriptionsToolStripMenuItem.Click += new System.EventHandler(this.removeAllSubscriptionsToolStripMenuItem_Click);
            // 
            // writeValueToTagToolStripMenuItem
            // 
            this.writeValueToTagToolStripMenuItem.Name = "writeValueToTagToolStripMenuItem";
            this.writeValueToTagToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.writeValueToTagToolStripMenuItem.Text = "Write value to tag";
            this.writeValueToTagToolStripMenuItem.Click += new System.EventHandler(this.writeValueToTagToolStripMenuItem_Click);
            // 
            // timerDeadmanSwitch
            // 
            this.timerDeadmanSwitch.Enabled = true;
            this.timerDeadmanSwitch.Interval = 5000;
            this.timerDeadmanSwitch.Tick += new System.EventHandler(this.timerDeadmanSwitch_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1261, 490);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "RMQ Wonderware Adapter - converts RabbitMQ messages into PLC tags";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTags)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Timer timerFlushTextboxes;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Timer timerFlushLogs;
        private System.Windows.Forms.Timer timerCloseLogFiles;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem diagnosticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSubscriptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem removeAllSubscriptionsToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridViewTags;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem writeValueToTagToolStripMenuItem;
        private System.Windows.Forms.Timer timerDeadmanSwitch;
    }
}

