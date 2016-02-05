using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CDO;
using ADODB;

namespace RSS2mail
{
    public partial class MainForm : Form
    {
        private OleDbConnection conn = null;
        private int fcInterval = 0;
        private int msInterval = 0;
        private WebClient wc = null;
        private System.Threading.Timer t1 = null;        // timer for download feed urls
        private System.Threading.Timer t2 = null;        // timer for send mails
        private bool jobDone = true;
        private bool canSend = true;

        private struct MailConfig
        {
            public string smtpserver;
            public int smtpport;
            public string sendusername;
            public string sendpassword;
            public string sendemailaddress;
        }

        private MailConfig mc = new MailConfig();

        public MainForm()
        {
            InitializeComponent();

            string filePath = @".\feeds.mdb";
            if (!File.Exists(filePath)) CreateDB(filePath);        // file doesn't exist, need to create it from the embedded resource
            else Trace.WriteLine("Database found: '" + filePath + "'");
            string tmp = ConfigurationManager.ConnectionStrings["feedsDB"].ConnectionString;
            string cStr = String.Format(tmp, filePath, "starT123!!");
            conn = new OleDbConnection(cStr);
            conn.Open();
            //Trace.WriteLine("Server version: {0}", conn.ServerVersion);
            //conn.Close();

            fcInterval = Convert.ToInt32(ConfigurationManager.AppSettings["fcInterval"]);
            msInterval = Convert.ToInt32(ConfigurationManager.AppSettings["msInterval"]);

            mc.sendemailaddress = "\"Ladislav Heller\" <heller.ladislav@zoznam.sk";
            mc.smtpserver = ConfigurationManager.AppSettings["smtpServer"];
            mc.smtpport = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
            mc.sendusername = ConfigurationManager.AppSettings["smtpUserName"];
            mc.sendpassword = ConfigurationManager.AppSettings["smtpPassword"];

            wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            //wc.Encoding = Encoding.GetEncoding("iso-8859-1");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region Destroy objects
            if (t1 != null)             // destroy timer T1
            {
                jobDone = false;
                t1.Dispose();
            }
            if (t2 != null)             // destroy timer T2
            {
                canSend = false;
                t2.Dispose();
            }
            if (conn != null)           // close and destroy DB connection
            {
                conn.Close();
                conn = null;
            }
            #endregion
            Trace.WriteLine("Bye bye!");
        }

        private void ShowErrorMessage(string method, string message)
        {
            MessageBox.Show(message + "\n\r\n\rPlease check the debug output for more information.\n\rContact: 'ladislav.heller@gmail.com'", method, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void CreateDB(string path)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                System.IO.Stream mdb = asm.GetManifestResourceStream("RSS2mail.Resources.feeds.mdb");
                #region Write template Access DB to disk
                using (StreamWriter sw = new StreamWriter(path))
                {
                    int bs = 0x4000;                                    // buffer size
                    byte[] buf = new byte[bs];                          // buffer
                    int br = 0;                                         // number of bytes read
                    while ((br = mdb.Read(buf, 0, buf.Length)) > 0)
                    {
                        sw.BaseStream.Write(buf, 0, br);
                    }
                }
                #endregion
                mdb.Close();
                Trace.WriteLine("Database file created successfully!");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                ShowErrorMessage("CreateDB", ex.Message);
            }
        }

        private XmlDocument GetFeedFromURL(string uri)
        {
            Trace.WriteLine("Reading info from RSS >> " + uri);
            EncodingInfo eif = null;
            XmlDocument feed = new XmlDocument();
            Encoding e = null;
            try
            {
                // determine the correct encoding of rss-feed based on response content-type
                byte[] data = wc.DownloadData(uri);
                string ct = wc.ResponseHeaders["Content-Type"];
                #region Find proper encoding
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    if (ct.ToLower().Contains(ei.Name.ToLower()))
                    {
                        eif = ei;
                        break;
                    }
                }
                #endregion
                if (eif != null) e = Encoding.GetEncoding(eif.Name);
                else e = Encoding.UTF8;
                feed.LoadXml(e.GetString(data));
                return feed;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return null;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            t1 = new System.Threading.Timer(new TimerCallback(TimerProcFeeds));
            t1.Change(0, fcInterval);

            t2 = new System.Threading.Timer(new TimerCallback(TimerProcMails));
            t2.Change(0, msInterval);
        }

        private void TimerProcFeeds(object state)
        {
            Trace.WriteLine("Checking feeds...");
            if (jobDone)
            {
                jobDone = false;
                Hashtable uris = new Hashtable();

                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("SELECT ID,FEED FROM [FEEDS]");
                OleDbDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        byte ID = Convert.ToByte(r["ID"]);
                        string URL = r["FEED"].ToString().Trim(new char[] { '#' });
                        uris.Add(ID, URL);
                    }
                    r.Close();
                    #region Process feeds
                    foreach (DictionaryEntry de in uris)
                    {
                        string _uri = de.Value.ToString();
                        XmlDocument feed = GetFeedFromURL(_uri);
                        if (feed == null) continue;

                        XmlNodeList nodes = feed.SelectNodes("/rss/channel/item");
                        #region Process links
                        foreach (XmlNode xn in nodes)
                        {
                            string title = xn["title"].InnerText;
                            string link = xn["link"].InnerText;
                            cmd.CommandText = String.Format("INSERT INTO [URLS] (URL, CREATED, FEEDID, SUBJECT) VALUES ('{0}', '{1}', {2}, '{3}')", link, DateTime.Now, de.Key, title);
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (OleDbException dbex)
                            {
                                if ((dbex.ErrorCode & 0x7FFFFFFF) == 0x4005) Trace.WriteLine("URL is already in database >> " + link);
                                else Trace.WriteLine(dbex.ToString());
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex.ToString());
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    Trace.Write("No RSS feeds in the database found.");
                    r.Close();
                }
                jobDone = true;
            }
        }

        private void TimerProcMails(object state)
        {
            Trace.WriteLine("Sending emails to recipients...");
            if (canSend)
            {
                canSend = false;
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT ID,URL,SUBJECT FROM [URLS] WHERE PROCESSED = 0";
                OleDbDataReader r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        int ID = Convert.ToInt32(r["ID"]);
                        string URL = r["URL"].ToString().Trim(new char[] { '#' });
                        string subject = r["SUBJECT"].ToString();
                        string rec = "heller.ladislav@zoznam.sk";
                        if (SendMail(rec, subject, URL, mc))
                        {
                            // Set PROCESSED field of the actual record in [URLS] table to value 1
                        }
                    }
                    r.Close();
                }
                else
                {
                    Trace.WriteLine("No URLs in the database found.");
                    r.Close();
                }
                canSend = true;
            }
        }

        private void btnAddFeed_Click(object sender, EventArgs e)
        {
            AddFeed frmAddFeed = new AddFeed(conn);
            frmAddFeed.ShowDialog();
        }

        private bool SendMail(string to, string subject, string url, MailConfig pmc)
        {
            bool success = true;
            DateTime before = DateTime.Now;
            try
            {
                CDO.Message msg = new CDO.Message();
                CDO.Configuration conf = msg.Configuration;
                Fields fs = conf.Fields;
                #region Set field values
                fs["http://schemas.microsoft.com/cdo/configuration/languagecode"].Value = "hu";
                fs["http://schemas.microsoft.com/cdo/configuration/postusing"].Value = CdoPostUsing.cdoPostUsingPort;
                fs["http://schemas.microsoft.com/cdo/configuration/sendemailaddress"].Value = pmc.sendemailaddress;
                fs["http://schemas.microsoft.com/cdo/configuration/sendpassword"].Value = pmc.sendpassword;
                fs["http://schemas.microsoft.com/cdo/configuration/sendusername"].Value = pmc.sendusername;
                fs["http://schemas.microsoft.com/cdo/configuration/sendusing"].Value = CdoSendUsing.cdoSendUsingPort;
                fs["http://schemas.microsoft.com/cdo/configuration/smtpaccountname"].Value = "Zoznam";
                fs["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"].Value = CdoProtocolsAuthentication.cdoBasic;
                fs["http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout"].Value = 10;
                fs["http://schemas.microsoft.com/cdo/configuration/smtpserver"].Value = pmc.smtpserver;
                fs["http://schemas.microsoft.com/cdo/configuration/smtpserverport"].Value = pmc.smtpport;
                fs["http://schemas.microsoft.com/cdo/configuration/usemessageresponsetext"].Value = true;
                fs["http://schemas.microsoft.com/cdo/configuration/smtpusessl"].Value = false;
                fs["urn:schemas:calendar:timezoneid"].Value = CdoTimeZoneId.cdoEasternEurope;
                #endregion
                fs.Update();
                msg.Subject = subject;
                msg.CreateMHTMLBody(url, CdoMHTMLFlags.cdoSuppressNone, null, null);
                msg.To = to;
                Console.WriteLine("Sending message...");
                msg.Send();
                TimeSpan total = DateTime.Now.Subtract(before);
                Trace.WriteLine("Message sent successfully. Total time: " + total.ToString());
                return success;
            }
            catch (COMException cex)
            {
                Trace.WriteLine(cex.ToString());
                return false;
            }
        }
    }
}
