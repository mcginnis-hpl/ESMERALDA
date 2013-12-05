using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class Browse : ESMERALDAPage
    {
        protected HtmlForm form1;
        protected Table tblPrograms;

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = GetAppString("appstring_shortname") + " - Browse";
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateContainers(conn);
            conn.Close();
        }

        protected void PopulateContainers(SqlConnection conn)
        {
            string cmd = "SELECT container_id, entity_name, entity_description FROM v_ESMERALDA_container_metadata WHERE parent_id IS NULL";
            if (!UserIsAdministrator)
            {
                cmd += " AND (IsPublic=1";
                if (IsAuthenticated && CurrentUser != null)
                {
                    cmd += " OR personid='" + CurrentUser.ID.ToString() + "'";
                }
                cmd += ")";
            }
            cmd += " GROUP BY container_id, entity_name, entity_description ORDER BY entity_name";
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell
                {
                    Text = "<a href='EditContainer.aspx?CONTAINERID=" + reader["container_id"].ToString() + "'>" + reader["entity_name"].ToString() + "</a>",
                    Width = Unit.Percentage(30)
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = reader["entity_description"].ToString()
                };
                tr.Cells.Add(tc);
                this.tblPrograms.Rows.Add(tr);
            }
            reader.Close();
        }
    }
}