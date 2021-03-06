﻿namespace RSS2mail
{
    partial class MainForm
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
        	this.btnStart = new System.Windows.Forms.Button();
        	this.btnAddFeed = new System.Windows.Forms.Button();
        	this.lblFeedStatus = new System.Windows.Forms.Label();
        	this.lblSendStatus = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// btnStart
        	// 
        	this.btnStart.Location = new System.Drawing.Point(12, 12);
        	this.btnStart.Name = "btnStart";
        	this.btnStart.Size = new System.Drawing.Size(75, 23);
        	this.btnStart.TabIndex = 0;
        	this.btnStart.Text = "Start";
        	this.btnStart.UseVisualStyleBackColor = true;
        	this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
        	// 
        	// btnAddFeed
        	// 
        	this.btnAddFeed.Location = new System.Drawing.Point(197, 12);
        	this.btnAddFeed.Name = "btnAddFeed";
        	this.btnAddFeed.Size = new System.Drawing.Size(75, 23);
        	this.btnAddFeed.TabIndex = 1;
        	this.btnAddFeed.Text = "Add feed...";
        	this.btnAddFeed.UseVisualStyleBackColor = true;
        	this.btnAddFeed.Click += new System.EventHandler(this.btnAddFeed_Click);
        	// 
        	// lblFeedStatus
        	// 
        	this.lblFeedStatus.Location = new System.Drawing.Point(12, 38);
        	this.lblFeedStatus.Name = "lblFeedStatus";
        	this.lblFeedStatus.Size = new System.Drawing.Size(260, 23);
        	this.lblFeedStatus.TabIndex = 2;
        	this.lblFeedStatus.Text = "Feed status:";
        	this.lblFeedStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// lblSendStatus
        	// 
        	this.lblSendStatus.Location = new System.Drawing.Point(12, 61);
        	this.lblSendStatus.Name = "lblSendStatus";
        	this.lblSendStatus.Size = new System.Drawing.Size(260, 23);
        	this.lblSendStatus.TabIndex = 2;
        	this.lblSendStatus.Text = "Send status:";
        	this.lblSendStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// MainForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(284, 262);
        	this.Controls.Add(this.lblSendStatus);
        	this.Controls.Add(this.lblFeedStatus);
        	this.Controls.Add(this.btnAddFeed);
        	this.Controls.Add(this.btnStart);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.Name = "MainForm";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "MainForm";
        	this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label lblSendStatus;
        private System.Windows.Forms.Label lblFeedStatus;

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnAddFeed;
    }
}

