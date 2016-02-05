using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RSS2mail
{
    public partial class AddFeed : Form
    {
        private OleDbConnection conn = null;

        public AddFeed(OleDbConnection _conn)
        {
            InitializeComponent();
            conn = _conn;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnAddFeed_Click(object sender, EventArgs e)
        {
            Trace.WriteLine("Feed to add >> " + tbURL.Text);
            try
            {
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("INSERT INTO [FEEDS] (FEED, CREATED) VALUES ('{0}','{1}')", tbURL.Text, DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), this.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}
