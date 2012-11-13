using ESMERALDAClasses;
using SlimeeLibrary;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class EditProgram : ESMERALDAPage
    {
protected void btnSave_Click(object sender, EventArgs e)
        {
            Program working = (Program) base.GetSessionValue("WorkingProgram");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            working.program_name = this.txtMetadata_Name.Text;
            working.acronym = this.txtMetadata_Acronym.Text;
            working.description = this.txtMetadata_Description.Text;
            working.logo_url = this.txtMetadata_LogoURL.Text;
            working.small_logo_url = this.txtMetadata_SmallLogoURL.Text;
            working.program_url = this.txtMetadata_URL.Text;
            working.start_date = this.controlStartDate.SelectedDate;
            working.end_date = this.controlEndDate.SelectedDate;
            if (working.Owner == null)
            {
                working.Owner = base.CurrentUser;
            }
            working.Save(conn);
            this.lblProgramID.Text = working.ID.ToString();
            this.PopulateFields(conn, working);
            conn.Close();
            base.SetSessionValue("WorkingProgram", working);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                Guid programid = Guid.Empty;
                for (int i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "PROGRAMID")
                    {
                        programid = new Guid(base.Request.Params[i]);
                    }
                }
                Program theProgram = null;
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (programid != Guid.Empty)
                {
                    theProgram = Program.Load(conn, programid);
                }
                else
                {
                    theProgram = new Program();
                }
                this.PopulateFields(conn, theProgram);
                base.SetSessionValue("WorkingProgram", theProgram);
                conn.Close();
            }
        }

        protected void PopulateFields(SqlConnection conn, Program working)
        {
            this.txtMetadata_Name.Text = working.program_name;
            this.txtMetadata_Acronym.Text = working.acronym;
            this.txtMetadata_Description.Text = working.description;
            this.txtMetadata_LogoURL.Text = working.logo_url;
            this.txtMetadata_SmallLogoURL.Text = working.small_logo_url;
            this.txtMetadata_URL.Text = working.program_url;
            this.lblMetadata_DatabaseName.Text = working.database_name;
            if (working.start_date > DateTime.MinValue)
            {
                this.controlStartDate.SelectedDate = working.start_date;
            }
            if (working.end_date > DateTime.MinValue)
            {
                this.controlEndDate.SelectedDate = working.end_date;
            }
            if (working.ID != Guid.Empty)
            {
                this.PopulateProjectList(conn, working);
            }
            this.lblProgramID.Text = working.ID.ToString();
            if (!base.IsAuthenticated)
            {
                this.txtMetadata_Name.ReadOnly = true;
                this.txtMetadata_Acronym.ReadOnly = true;
                this.txtMetadata_Description.ReadOnly = true;
                this.txtMetadata_LogoURL.ReadOnly = true;
                this.txtMetadata_SmallLogoURL.ReadOnly = true;
                this.txtMetadata_URL.ReadOnly = true;
                this.btnSave.Visible = false;
                this.controlStartDate.Enabled = false;
                this.controlEndDate.Enabled = false;
            }
            else
            {
                this.txtMetadata_Name.ReadOnly = false;
                this.txtMetadata_Acronym.ReadOnly = false;
                this.txtMetadata_Description.ReadOnly = false;
                this.txtMetadata_LogoURL.ReadOnly = false;
                this.txtMetadata_SmallLogoURL.ReadOnly = false;
                this.txtMetadata_URL.ReadOnly = false;
                this.btnSave.Visible = true;
                this.controlStartDate.Enabled = true;
                this.controlEndDate.Enabled = true;
            }
        }

        protected void PopulateProjectList(SqlConnection conn, Program working)
        {
            string innerHTML = string.Empty;
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT project_name, project_id, description FROM v_ProjectListByProgram WHERE program_id='" + working.ID.ToString() + "' ORDER BY project_name" }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Program Projects</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='EditProject.aspx?PROJECTID=" + reader["project_id"].ToString() + "'>" + reader["project_name"].ToString() + "</a></td><td>" + reader["description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            this.projects.InnerHtml = innerHTML;
            reader.Close();
            if (base.IsAuthenticated)
            {
                this.addProjectControl.InnerHtml = "<a href='EditProject.aspx?PROGRAMID=" + working.ID.ToString() + "'>Add a project to this program.</a>";
            }
        }
    }
}