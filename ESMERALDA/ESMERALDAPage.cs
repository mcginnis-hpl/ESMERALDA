using ESMERALDAClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace ESMERALDA
{
    public class ESMERALDAPage : Page
    {
        public SqlConnection Connect(string connectionString)
        {
            SqlConnection ret = new SqlConnection(connectionString);
            ret.Open();
            return ret;
        }

        public SqlConnection ConnectToConfigString(string key)
        {
            string connstring = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            return this.Connect(connstring);
        }

        public SqlConnection ConnectToDatabase(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Client; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        public SqlConnection ConnectToDatabaseReadOnly(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Reader; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        public static string CreatePasswordHash(string pwd, string salt)
        {
            return (FormsAuthentication.HashPasswordForStoringInConfigFile(pwd + salt, "SHA1") + salt);
        }

        public static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public object GetSessionValue(string key)
        {
            try
            {
                return this.Session[this.Session.SessionID + "-" + key];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Guid GetUserID(SqlConnection conn)
        {
            Guid ret = Guid.Empty;
            if (!string.IsNullOrEmpty(this.Username))
            {
                SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT personid FROM person_metadata WHERE email='" + this.Username + "'" }.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        ret = new Guid(reader[0].ToString());
                    }
                }
                reader.Close();
            }
            return ret;
        }

        public void LoadCommonStructures()
        {
            SqlConnection conn = this.ConnectToConfigString("RepositoryConnection");
            List<Metric> allmetrics = Metric.LoadExistingMetrics(conn);
            List<Conversion> allconversion = Conversion.LoadAll(conn, allmetrics);
            this.SetSessionValue("Metrics", allmetrics);
            this.SetSessionValue("Conversions", allconversion);
            conn.Close();
        }

        public void Logout()
        {
            this.Session.Clear();
            this.Session.Abandon();
            FormsAuthentication.SignOut();
            base.Response.Cache.SetExpires(DateTime.Now);
            base.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            base.Response.Cache.SetNoStore();
        }

        public void RemoveSessionValue(string key)
        {
            this.Session.Remove(this.Session.SessionID + "-" + key);
        }

        protected void RepositoryInit()
        {
        }

        public void SetSessionValue(string key, object inobj)
        {
            this.Session[this.Session.SessionID + "-" + key] = inobj;
        }

        public void ShowAlert(string msg)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), "MessagePopUp", "<script language='JavaScript'>alert('" + msg + "');</script>");
        }

        public List<Conversion> Conversions
        {
            get
            {
                if (this.GetSessionValue("Conversions") == null)
                {
                    this.LoadCommonStructures();
                }
                return (List<Conversion>)this.GetSessionValue("Conversions");
            }
        }

        public Person CurrentUser
        {
            get
            {
                if (!IsAuthenticated)
                    return null;
                Person ret = (Person)this.GetSessionValue("CurrentUser");
                if (ret == null)
                {
                    if (!string.IsNullOrEmpty(Username))
                    {
                        Person p = new Person();
                        SqlConnection conn = ConnectToConfigString("RepositoryConnection");
                        p.LoadByUsername(conn, Username);
                        conn.Close();
                        if (p != null)
                        {
                            ret = p;
                            SetSessionValue("CurrentUser", p);
                        }
                    }
                }
                return ret;
            }
            set
            {
                this.SetSessionValue("CurrentUser", value);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return base.Request.IsAuthenticated;
            }
        }

        public List<Metric> Metrics
        {
            get
            {
                if (this.GetSessionValue("Metrics") == null)
                {
                    this.LoadCommonStructures();
                }
                return (List<Metric>)this.GetSessionValue("Metrics");
            }
        }

        public bool UserIsAdministrator
        {
            get
            {
                return (this.Username == "smcginnis@umces.edu");
            }
        }

        public string Username
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }
    }
}