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

        public string GetValueKeyCrossPage(string key)
        {
            try
            {
                if (ViewState["UniqueID"] == null)
                    return null;
                return (string)ViewState["UniqueID"] + "-" + key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetSessionValueCrossPage(string key)
        {
            try
            {
                return this.Session[key];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetSessionValue(string key)
        {
            try
            {
                if (ViewState["UniqueID"] == null)
                    return null;
                return this.Session[ViewState["UniqueID"] + "-" + key];
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
                SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT personid FROM v_ESMERALDA_person_with_username WHERE email='" + this.Username + "'" }.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        ret = new Guid(reader[0].ToString());
                        break;
                    }
                }
                reader.Close();
            }
            return ret;
        }

        public void LoadApplicationStrings()
        {
            SqlConnection conn = this.ConnectToConfigString("RepositoryConnection");
            Dictionary<string, string> appstrings = new Dictionary<string, string>();
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadApplicationStrings"
            };
            SqlDataReader reader = cmd.ExecuteReader();
            string tag = string.Empty;
            string val = string.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("tag")))
                {
                    tag = reader["tag"].ToString();
                    val = reader["value"].ToString();
                    if(!appstrings.ContainsKey(tag))
                        appstrings.Add(tag, val);
                }
            }
            reader.Close();
            conn.Close();
            SetSessionValue("AppStrings", appstrings);
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
            if (ViewState["UniqueID"] == null)
                return;
            this.Session.Remove(ViewState["UniqueID"] + "-" + key);
        }

        protected void RepositoryInit()
        {
        }

        public void SetSessionValue(string key, object inobj)
        {
            if (ViewState["UniqueID"] == null)
                ViewState["UniqueID"] = Guid.NewGuid().ToString();
            this.Session[ViewState["UniqueID"] + "-" + key] = inobj;
        }

        public void SetSessionValueCrossPage(string key, object inobj)
        {
            this.Session[key] = inobj;
        }

        public void ShowAlert(string msg)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), "MessagePopUp", "<script language='JavaScript'>alert('" + msg + "');</script>");
        }

        public void AddStartupCall(string call, string name)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), name, "<script language='JavaScript'>" + call + "</script>");
        }

        public void RemoveStartupCall(string name)
        {
            base.ClientScript.RegisterStartupScript(base.GetType(), name, string.Empty, true);
        }

        public Dictionary<string,string> AppStrings
        {
            get
            {
                if (this.GetSessionValue("AppStrings") == null)
                {
                    this.LoadApplicationStrings();
                }
                return (Dictionary<string, string>)this.GetSessionValue("AppStrings");
            }
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
                if (CurrentUser == null)
                    return false;
                return CurrentUser.IsAdministrator;
            }
        }

        public string Username
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        // Return a value from web.config
        public string GetApplicationSetting(string inkey)
        {
            return ConfigurationManager.AppSettings[inkey];
        }

        public string GetAppString(string inkey)
        {
            if (!AppStrings.ContainsKey(inkey))
                return string.Empty;
            return AppStrings[inkey];
        }
    }
}