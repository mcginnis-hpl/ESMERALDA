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
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulatePrograms(conn);
            conn.Close();
        }

        protected void PopulatePrograms(SqlConnection conn)
        {
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT program_id, program_name, description FROM program_metadata ORDER BY program_name" }.ExecuteReader();
            while (reader.Read())
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell
                {
                    Text = "<a href='EditProgram.aspx?PROGRAMID=" + reader["program_id"].ToString() + "'>" + reader["program_name"].ToString() + "</a>"
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = reader["description"].ToString()
                };
                tr.Cells.Add(tc);
                this.tblPrograms.Rows.Add(tr);
            }
            reader.Close();
        }
    }
}