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
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulatePrograms(conn);
            conn.Close();
            this.versionNumber.InnerHtml = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!IsPostBack)
            {
                Guid redirectid = Guid.Empty;
                for (int i = 0; i < Request.Params.Count; i++)
                {
                    if (Request.Params.GetKey(i).ToUpper() == "DATASETID")
                    {
                        redirectid = new Guid(Request.Params[i]);
                    }
                }
                if (redirectid != Guid.Empty)
                {
                    string url = "setContentSource(\"ViewDataset.aspx?DATASETID=" + redirectid.ToString() + "\")";
                    AddStartupCall(url, "directlink");
                }
            }
        }

        protected void PopulatePrograms(SqlConnection conn)
        {
            string innerHTML = "<h3>Current Programs</h3><ul>";
            string cmd = "SELECT program_id, program_name FROM v_ESMERALDA_program_metadata ORDER BY program_name";
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                innerHTML = innerHTML + "<li><a href='javascript:setContentSource(\"EditProgram.aspx?PROGRAMID=" + reader["program_id"].ToString() + "\")'>" + reader["program_name"].ToString() + "</a></li>";
            }
            if (base.IsAuthenticated)
            {
                innerHTML = innerHTML + "<li><a href='javascript:setContentSource(\"EditProgram.aspx\")'>Add a new Program</a></li>";
            }
            reader.Close();
            innerHTML = innerHTML + "</ul>";
            this.left_side.InnerHtml = innerHTML;
        }
    }
}