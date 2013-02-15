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
            working.SetMetadataValue("title", this.txtMetadata_Name.Text);
            working.SetMetadataValue("acronym", this.txtMetadata_Acronym.Text);
            working.SetMetadataValue("description", this.txtMetadata_Description.Text);
            working.SetMetadataValue("logourl", this.txtMetadata_LogoURL.Text);
            working.SetMetadataValue("small_logo_url", this.txtMetadata_SmallLogoURL.Text);
            working.SetMetadataValue("url", this.txtMetadata_URL.Text);
            working.SetMetadataValue("startdate", this.controlStartDate.SelectedDate.ToString());
            working.SetMetadataValue("enddate", this.controlEndDate.SelectedDate.ToString());
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
                    theProgram = new Program();
                    theProgram.Load(conn, programid);
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
            this.txtMetadata_Name.Text = working.GetMetadataValue("title");
            this.txtMetadata_Acronym.Text = working.GetMetadataValue("acronym");
            this.txtMetadata_Description.Text = working.GetMetadataValue("description");
            this.txtMetadata_LogoURL.Text = working.GetMetadataValue("logourl");
            this.txtMetadata_SmallLogoURL.Text = working.GetMetadataValue("small_logo_url");
            this.txtMetadata_URL.Text = working.GetMetadataValue("url");
            this.lblMetadata_DatabaseName.Text = working.database_name;
            if (!string.IsNullOrEmpty(working.GetMetadataValue("startdate")))
            {
                this.controlStartDate.SelectedDate = DateTime.Parse(working.GetMetadataValue("startdate"));
            }
            if (!string.IsNullOrEmpty(working.GetMetadataValue("enddate")))
            {
                this.controlEndDate.SelectedDate = DateTime.Parse(working.GetMetadataValue("enddate"));
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
            string cmd = "SELECT project_name, project_id, project_description FROM v_ESMERALDA_project_metadata WHERE program_id='" + working.ID.ToString() + "' ORDER BY project_name";
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Program Projects</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='EditProject.aspx?PROJECTID=" + reader["project_id"].ToString() + "'>" + reader["project_name"].ToString() + "</a></td><td>" + reader["project_description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            this.projects.InnerHtml = innerHTML;
            reader.Close();
            if (base.IsAuthenticated && working != null)
            {
                Person p = CurrentUser;
                if (UserIsAdministrator || (p != null && p.Owner != null && working.Owner.ID == p.ID))
                {
                    this.addProjectControl.InnerHtml = "<a href='EditProject.aspx?PROGRAMID=" + working.ID.ToString() + "'>Add a project to this program.</a>";
                }
            }
        }
    }
}