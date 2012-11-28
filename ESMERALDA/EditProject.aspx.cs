using ESMERALDAClasses;
using SlimeeLibrary;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class EditProject : ESMERALDAPage
    {
        protected void btn_SaveMetadata_Click(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Project theProject = (Project)base.GetSessionValue("WorkingProject");
            theProject.project_name = this.txtMetadata_Name.Text;
            theProject.acronym = this.txtMetadata_Acronym.Text;
            theProject.description = this.txtMetadata_Description.Text;
            theProject.logo_url = this.txtMetadata_LogoURL.Text;
            theProject.small_logo_url = this.txtMetadata_SmallLogoURL.Text;
            theProject.project_url = this.txtMetadata_URL.Text;
            theProject.start_date = this.controlStartDate.SelectedDate;
            theProject.end_date = this.controlEndDate.SelectedDate;
            if (theProject.Owner == null)
            {
                theProject.Owner = base.CurrentUser;
            }
            theProject.Save(conn);
            if (!string.IsNullOrEmpty(this.comboParentProgram.SelectedValue))
            {
                Guid projectid = new Guid(this.comboParentProgram.SelectedValue);
                if ((theProject.parentProgram == null) || (theProject.parentProgram.ID != projectid))
                {
                    theProject.parentProgram = new Program();
                    theProject.parentProgram.Load(conn, projectid);
                }
            }
            this.PopulateFields(conn, theProject);
            base.SetSessionValue("WorkingProject", theProject);
            conn.Close();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                Guid projectid = Guid.Empty;
                Guid programid = Guid.Empty;
                for (int i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID")
                    {
                        projectid = new Guid(base.Request.Params[i]);
                    }
                    if (base.Request.Params.GetKey(i).ToUpper() == "PROGRAMID")
                    {
                        programid = new Guid(base.Request.Params[i]);
                    }
                }
                Project theProject = new Project();
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (projectid != Guid.Empty)
                {                    
                    theProject.Load(conn, projectid);
                }
                else
                {
                    if (programid != Guid.Empty)
                    {
                        theProject.parentProgram = new Program();
                        theProject.parentProgram.Load(conn, programid);
                    }
                }
                this.PopulateFields(conn, theProject);
                base.SetSessionValue("WorkingProject", theProject);
                conn.Close();
            }
        }

        protected void PopulateDatabaseList(SqlConnection conn, Project inProject)
        {
            string innerHTML = string.Empty;
            string cmd = string.Empty;
            cmd = "SELECT dataset_name, dataset_id, brief_description FROM v_dataset_metadata WHERE project_id='" + inProject.ID.ToString() + "' AND (IsPublic=1";
            if (IsAuthenticated && CurrentUser != null)
            {
                cmd += " OR CreatedBy='" + CurrentUser.ID + "'";
            }
            cmd += ")";
            cmd = cmd + " ORDER BY dataset_name";

            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Project Datasets</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='ViewDataset.aspx?DATASETID=" + reader["dataset_id"].ToString() + "'>" + reader["dataset_name"].ToString() + "</a></td><td>" + reader["brief_description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            this.currentDatasets.InnerHtml = innerHTML;
            reader.Close();
            if (base.IsAuthenticated)
            {
                string url = "<a href='EditDataSet.aspx?PROJECTID=" + inProject.ID.ToString() + "'>Add a dataset to this project.</a>";
                url += "<br/><a href='EditJoin.aspx?PROJECTID=" + inProject.ID.ToString() + "'>Add a join to this project.</a>";
                this.addDatasetControl.InnerHtml = url;
            }
            else
            {
                this.addDatasetControl.InnerHtml = string.Empty;
            }
        }

        protected void PopulateFields(SqlConnection conn, Project theProject)
        {
            this.txtMetadata_Name.Text = theProject.project_name;
            this.txtMetadata_Acronym.Text = theProject.acronym;
            this.txtMetadata_Description.Text = theProject.description;
            this.txtMetadata_LogoURL.Text = theProject.logo_url;
            this.txtMetadata_SmallLogoURL.Text = theProject.small_logo_url;
            this.txtMetadata_URL.Text = theProject.project_url;
            this.txtMetadata_DatabaseName.Text = theProject.override_database_name;
            if (theProject.start_date > DateTime.MinValue)
            {
                this.controlStartDate.SelectedDate = theProject.start_date;
            }
            if (theProject.end_date > DateTime.MinValue)
            {
                this.controlEndDate.SelectedDate = theProject.end_date;
            }
            this.PopulatePrograms(conn, theProject);
            if (theProject.ID != Guid.Empty)
            {
                this.PopulateDatabaseList(conn, theProject);
                this.PopulateViewList(conn, theProject);
            }
            if (!base.IsAuthenticated)
            {
                this.txtMetadata_Name.ReadOnly = true;
                this.txtMetadata_Acronym.ReadOnly = true;
                this.txtMetadata_Description.ReadOnly = true;
                this.txtMetadata_LogoURL.ReadOnly = true;
                this.txtMetadata_SmallLogoURL.ReadOnly = true;
                this.txtMetadata_URL.ReadOnly = true;
                this.btn_SaveMetadata.Visible = false;
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
                this.btn_SaveMetadata.Visible = true;
                this.controlStartDate.Enabled = true;
                this.controlEndDate.Enabled = true;
            }
        }

        protected void PopulatePrograms(SqlConnection conn, Project theProject)
        {
            this.comboParentProgram.Items.Clear();
            this.comboParentProgram.Items.Add(new ListItem("", ""));
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT program_id, program_name FROM program_metadata ORDER BY program_name" }.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("program_id")))
                {
                    ListItem li = new ListItem(reader["program_name"].ToString(), reader["program_id"].ToString());
                    this.comboParentProgram.Items.Add(li);
                    if ((theProject.parentProgram != null) && (li.Value == theProject.parentProgram.ID.ToString()))
                    {
                        this.comboParentProgram.SelectedIndex = this.comboParentProgram.Items.Count - 1;
                    }
                }
            }
            reader.Close();
        }

        protected void PopulateViewList(SqlConnection conn, Project inProject)
        {
            string innerHTML = string.Empty;
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT view_name, view_id, brief_description FROM v_Views_By_Project WHERE project_id='" + inProject.ID.ToString() + "' ORDER BY view_name" }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Project Views</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='ViewDataset.aspx?VIEWID=" + reader["view_id"].ToString() + "'>" + reader["view_name"].ToString() + "</a></td><td>" + reader["brief_description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            this.currentViews.InnerHtml = innerHTML;
            reader.Close();
        }
    }
}