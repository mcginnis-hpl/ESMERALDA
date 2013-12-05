using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.UI.HtmlControls;

namespace ESMERALDA
{
    public partial class Default : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = GetAppString("appstring_shortname") + " - " + GetAppString("appstring_fullname");
            pagestring_fullname.InnerHtml = GetAppString("appstring_fullname");
            pagestring_shortname.InnerHtml = GetAppString("appstring_shortname");
            SqlConnection conn = null;
            try
            {
                conn = base.ConnectToConfigString("RepositoryConnection");
                this.PopulateContainers(conn);
                this.versionNumber.InnerHtml = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (!IsPostBack)
                {
                    Guid redirectid = Guid.Empty;
                    for (int i = 0; i < Request.Params.Count; i++)
                    {
                        if (Request.Params.GetKey(i).ToUpper() == "ENTITYID")
                        {
                            redirectid = new Guid(Request.Params[i]);
                        }
                    }                    
                    if (redirectid != Guid.Empty)
                    {
                        string type = ESMERALDAClasses.Utils.GetEntityType(redirectid, conn);
                        string url = string.Empty;
                        if (type == "dataset")
                        {
                            url = "setContentSource(\"ViewDataset.aspx?DATASETID=" + redirectid.ToString() + "\")";                            
                        }
                        else if (type == "container")
                        {
                            url = "setContentSource(\"EditContainer.aspx?CONTAINERID=" + redirectid.ToString() + "\")";                            
                        }
                        else if (type == "view")
                        {
                            url = "setContentSource(\"ViewDataset.aspx?VIEWID=" + redirectid.ToString() + "\")";                            
                        }
                        if (!string.IsNullOrEmpty(url))
                        {
                            AddStartupCall(url, "directlink");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message + ": " + ex.StackTrace);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        protected void PopulateContainers(SqlConnection conn)
        {
            string innerHTML = "<h3>Current Folders</h3><ul>";
            string cmd = "SELECT container_id, entity_name FROM v_ESMERALDA_container_metadata WHERE parent_id IS NULL";
            if (!UserIsAdministrator)
            {
                cmd += " AND (IsPublic=1";
                if(IsAuthenticated && CurrentUser != null)
                {
                    cmd += " OR personid='" + CurrentUser.ID.ToString() + "'";
                }
                cmd += ")";
            }
            cmd += " GROUP BY container_id, entity_name ORDER BY entity_name";
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                innerHTML = innerHTML + "<li><a href='javascript:setContentSource(\"EditContainer.aspx?CONTAINERID=" + reader["container_id"].ToString() + "\")'>" + reader["entity_name"].ToString() + "</a></li>";
            }
            if (base.IsAuthenticated && CurrentUser != null)
            {
                innerHTML = innerHTML + "<li><a href='javascript:setContentSource(\"EditContainer.aspx\")'>Add a new top-level folder</a></li>";
            }
            reader.Close();
            innerHTML = innerHTML + "</ul>";
            this.left_side.InnerHtml = innerHTML;
        }       
    }
}