using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using ESMERALDAClasses;
using System.IO;
using System.Net.Mail;
using System.Configuration;

namespace LargeDownloadPackager
{
    public partial class LDPService : ServiceBase
    {
        protected Thread myRunThread;
        protected bool _continue;

        public LDPService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("ESMERALDAService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "ESMERALDAService", "Application");
            }
            eventLog1.Source = "ESMERALDAService";
            eventLog1.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Starting HPLAQSService.");
            try
            {
                myRunThread = new Thread(DoProcessing);
                _continue = true;
                myRunThread.Start();
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message + ": " + ex.StackTrace.ToString());
            }
        }

        private void FlagRequest(SqlConnection conn, Guid requestid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_FlagRequest"
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.ExecuteNonQuery();
        }

        // Send an email from the application one or more users.
        public void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body, string fromaddress, string fromuser, string frompassword, string emailport, string emailhost)
        {
            MailMessage mail = new MailMessage();
            try
            {
                mail.From = new MailAddress(fromaddress);
            }
            catch (Exception)
            {
                return;
            }
            if (to != null)
            {
                foreach (string s in to)
                {
                    try
                    {
                        mail.To.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (cc != null)
            {
                foreach (string s in cc)
                {
                    try
                    {
                        mail.CC.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (bcc != null)
            {
                foreach (string s in bcc)
                {
                    try
                    {
                        mail.Bcc.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Port = int.Parse(emailport);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = emailhost;
            client.Credentials = new System.Net.NetworkCredential(fromuser, frompassword);
            client.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
            client.Send(mail);
        }

        // Send an email from the application one or more users.
        public void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body)
        {
            string fromaddress = (string)Properties.Settings.Default["emailaddress"];
            string fromuser = (string)Properties.Settings.Default["emailuser"];
            string frompassword = (string)Properties.Settings.Default["emailpassword"];
            string emailport = (string)Properties.Settings.Default["emailport"];
            string emailhost = (string)Properties.Settings.Default["emailhost"];
            SendEmail(to, cc, bcc, subject, body, fromaddress, fromuser, frompassword, emailport, emailhost);
        }

        private SqlConnection ConnectToDatabaseReadOnly(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Reader; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        private void DoProcessing()
        {
            while (_continue)
            {
                try
                {
                    SqlConnection conn = new SqlConnection((string)Properties.Settings.Default["metadata_conn"]);
                    string save_path = (string)Properties.Settings.Default["save_path"];
                    string link_path = (string)Properties.Settings.Default["link_path"];
                    conn.Open();
                    List<DownloadRequest> reqs = DownloadRequest.LoadUnprocessedRequests(conn);                    
                    if (reqs.Count > 0)
                    {
                        List<Metric> metrics = Metric.LoadExistingMetrics(conn);
                        List<Conversion> conversions = Conversion.LoadAll(conn, metrics);
                        foreach (DownloadRequest d in reqs)
                        {
                            ESMERALDAClasses.Person user = new Person();
                            user.Load(conn, d.userid);
                            if(user == null || string.IsNullOrEmpty(user.GetMetadataValue("cntemail")))
                                continue;
                            string source_type = Utils.GetEntityType(d.sourceid, conn);
                            View v = new View();
                            v.Load(conn, d.sourceid, conversions, metrics);
                            string dbname = v.ParentContainer.database_name;                            
                            SqlConnection dconn = ConnectToDatabaseReadOnly(dbname);
                            FileStream fs = File.Open(save_path + d.filename, FileMode.CreateNew);
                            StreamWriter sw = new StreamWriter(fs);
                            v.WriteDataToStream(d.delimiter, sw, dconn);
                            dconn.Close();
                            fs.Flush();
                            fs.Close();

                            string file_url = link_path + fs;
                            string body = "<p>Your GISR download is ready.  <a href='" + file_url + "'>Click here to download the file.</a>";
                            string subject = "GISR Download Ready";
                            string[] to = new string[1];
                            to[0] = user.GetMetadataValue("cntemail");
                            SendEmail(to, null, null, subject, body);
                            d.FlagRequest(conn);
                        }
                    }
                    conn.Close();
                    Thread.Sleep(10000);
                }
                catch (Exception ex)
                {
                    eventLog1.WriteEntry(ex.Message + ": " + ex.StackTrace.ToString());
                }
            }
        }

        protected override void OnStop()
        {
            _continue = false;
            while (myRunThread.IsAlive)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
