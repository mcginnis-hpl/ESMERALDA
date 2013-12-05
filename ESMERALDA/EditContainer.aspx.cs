using ESMERALDAClasses;
using SlimeeLibrary;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace ESMERALDA
{
    public partial class EditContainer : ESMERALDAPage
    {
        protected void btn_SaveMetadata_Click(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Container theContainer = (Container)base.GetSessionValue("WorkingContainer");
            theContainer.SetMetadataValue("title", this.txtMetadata_Name.Text);
            theContainer.SetMetadataValue("acronym", this.txtMetadata_Acronym.Text);
            theContainer.SetMetadataValue("description", this.txtMetadata_Description.Text);
            theContainer.SetMetadataValue("logourl", this.txtMetadata_LogoURL.Text);
            theContainer.SetMetadataValue("small_logo_url", this.txtMetadata_SmallLogoURL.Text);
            theContainer.SetMetadataValue("url", this.txtMetadata_URL.Text);
            theContainer.SetMetadataValue("startdate", this.controlStartDate.SelectedDate.ToString());
            theContainer.SetMetadataValue("enddate", this.controlEndDate.SelectedDate.ToString());
            if (chkIsSeparateDatabase.Checked)
            {
                theContainer.database_name = txtMetadata_DatabaseName.Text;
                if (string.IsNullOrEmpty(theContainer.database_name))
                {
                    theContainer.CreateDatabase(conn);
                }
            }
            theContainer.IsPublic = chkIsPublic.Checked;
            if (theContainer.Owner == null)
            {
                theContainer.Owner = base.CurrentUser;
            }
            List<Guid> personids = new List<Guid>();
            List<string> rels = new List<string>();

            chooser.GetSelectedItems(personids, rels);
            theContainer.Relationships.Clear();
            for (int i = 0; i < personids.Count; i++)
            {
                PersonRelationship pr = new PersonRelationship();
                pr.person = new Person();
                pr.person.Load(conn, personids[i]);
                pr.relationship = rels[i];
                theContainer.Relationships.Add(pr);
            }
            theContainer.Save(conn);
            this.PopulateFields(conn, theContainer);
            base.SetSessionValue("WorkingContainer", theContainer);
            conn.Close();
        }

        protected void PopulateBreadcrumb(ESMERALDAClasses.Container working)
        {
            if (working == null)
                return;
            string total_link = string.Empty;
            total_link = working.GetMetadataValue("title");
            string prog_link = string.Empty;
            Container c = working.parentContainer;
            while (c != null)
            {
                prog_link = "<a href='EditContainer.aspx?CONTAINERID=" + c.ID.ToString() + "'>" + c.GetMetadataValue("title") + "</a>";
                total_link = prog_link + ": " + total_link;
                c = c.parentContainer;
            }
            breadcrumb.InnerHtml = total_link;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool do_refresh = false;
            if (hiddenCommands != null)
            {
                if (hiddenCommands.Value == "REFRESH")
                {
                    do_refresh = true;
                    hiddenCommands.Value = string.Empty;
                }
            }
            if (!base.IsPostBack)
            {
                RemoveSessionValue("WorkingContainer");
            }
            if (Request.Browser.Type.Contains("Firefox"))
            {
                attachments.Visible = false;
            }
            if (!base.IsPostBack || do_refresh)
            {
                Guid containerid = Guid.Empty;
                Guid parentid = Guid.Empty;
                Container theContainer = null;
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (base.IsPostBack)
                {
                    theContainer = (Container)GetSessionValue("WorkingContainer");
                }
                else
                {
                    for (int i = 0; i < base.Request.Params.Count; i++)
                    {
                        if (base.Request.Params.GetKey(i).ToUpper() == "CONTAINERID")
                        {
                            containerid = new Guid(base.Request.Params[i]);
                        }
                        if (base.Request.Params.GetKey(i).ToUpper() == "PARENTID")
                        {
                            parentid = new Guid(base.Request.Params[i]);
                        }
                    }
                }
                if (theContainer == null)
                {
                    theContainer = new Container();                    
                    if (containerid != Guid.Empty)
                    {
                        theContainer.Load(conn, containerid);
                    }
                    else
                    {
                        if (parentid != Guid.Empty)
                        {
                            theContainer.ParentEntity = new Container();
                            theContainer.ParentEntity.Load(conn, parentid);
                        }
                    }
                }
                this.PopulateFields(conn, theContainer);
                base.SetSessionValue("WorkingContainer", theContainer);
                PopulateBreadcrumb(theContainer);
                conn.Close();
            }
        }

        protected void PopulateDatabaseList(SqlConnection conn, Container inContainer)
        {
            DateTime debug_start = DateTime.Now;
            string innerHTML = string.Empty;
            string cmd = string.Empty;
            cmd = "SELECT dataset_name, dataset_id, dataset_purpose, contributors FROM v_ESMERALDA_dataset_metadata WHERE project_id='" + inContainer.ID.ToString() + "'";
            if (!UserIsAdministrator)
            {
                cmd += " AND (IsPublic=1";
                if (IsAuthenticated && CurrentUser != null)
                {
                    cmd += " OR personid='" + CurrentUser.ID.ToString() + "'";
                }
                cmd += ")";
            }
            cmd = cmd + " GROUP BY dataset_name, dataset_id, dataset_purpose, contributors ORDER BY dataset_name";

            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Datasets</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='ViewDataset.aspx?DATASETID=" + reader["dataset_id"].ToString() + "'>" + reader["dataset_name"].ToString() + "</a></td><td>" + reader["dataset_purpose"].ToString() + "</td><td>" + reader["contributors"].ToString() + "</td></tr>";
            }
            System.Diagnostics.Debug.WriteLine("Time to populate dataset: " + (DateTime.Now - debug_start).TotalSeconds.ToString());
            debug_start = DateTime.Now;
            innerHTML = innerHTML + "</table>";
            this.currentDatasets.InnerHtml = innerHTML;
            reader.Close();

            if (base.IsAuthenticated)
            {
                string url = "<table border='0'><tr>";
                url += "<td><a class='squarebutton' href='EditContainer.aspx?PARENTID=" + inContainer.ID.ToString() + "'><span>Add a subfolder to this project.</span></a></td>";
                url += "<td><a class='squarebutton' href='EditDataSet.aspx?CONTAINERID=" + inContainer.ID.ToString() + "'><span>Add a dataset to this project.</span></a></td>";                
                url += "<td><a class='squarebutton' href='EditView.aspx?PROJECTID=" + inContainer.ID.ToString() + "'><span>Add a View to this project.</span></a></td>";
                url += "</tr></table>";
                this.addDatasetControl.InnerHtml = url;
            }
            else
            {
                string url = "<a class='squarebutton' href='EditJoin.aspx?PROJECTID=" + inContainer.ID.ToString() + "'><span>Add a join to this project.</span></a>";
                this.addDatasetControl.InnerHtml = url;
            }

            // LOAD CONTAINERS
            innerHTML = string.Empty;

            cmd = "SELECT container_id, parent_id, entity_name, entity_description FROM v_ESMERALDA_container_metadata WHERE parent_id='" + inContainer.ID.ToString() + "'";
            if (!UserIsAdministrator)
            {
                cmd += " AND (IsPublic=1";
                if (IsAuthenticated && CurrentUser != null)
                {
                    cmd += " OR personid='" + CurrentUser.ID.ToString() + "'";
                }
                cmd += ")";
            }
            cmd = cmd + " GROUP BY container_id, parent_id, entity_name, entity_description ORDER BY entity_name";

            reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Sub-Folders</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='EditContainer.aspx?CONTAINERID=" + reader["container_id"].ToString() + "'>" + reader["entity_name"].ToString() + "</a></td><td>" + reader["entity_description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            this.subContainers.InnerHtml = innerHTML;
            reader.Close();
            System.Diagnostics.Debug.WriteLine("Time to populate subfolders: " + (DateTime.Now - debug_start).TotalSeconds.ToString());
            debug_start = DateTime.Now;
            // POPULATE VIEWS
            innerHTML = string.Empty;
            cmd = "SELECT view_name, view_id, view_description FROM v_ESMERALDA_view_metadata WHERE project_id='" + inContainer.ID.ToString() + "'";
            if (!UserIsAdministrator)
            {
                cmd += " AND (IsPublic=1";
                if (IsAuthenticated && CurrentUser != null)
                {
                    cmd += " OR personid='" + CurrentUser.ID.ToString() + "'";
                }
                cmd += ")";
            }
            cmd += " GROUP BY view_name, view_id, view_description ORDER BY view_name";
            reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                if (string.IsNullOrEmpty(innerHTML))
                {
                    innerHTML = "<h3>Views</h3><br/><table border='0'>";
                }
                innerHTML = innerHTML + "<tr><td><a href='ViewDataset.aspx?VIEWID=" + reader["view_id"].ToString() + "'>" + reader["view_name"].ToString() + "</a></td><td>" + reader["view_description"].ToString() + "</td></tr>";
            }
            innerHTML = innerHTML + "</table>";
            System.Diagnostics.Debug.WriteLine("Time to populate views: " + (DateTime.Now - debug_start).TotalSeconds.ToString());
            debug_start = DateTime.Now;
            this.currentViews.InnerHtml = innerHTML;
            reader.Close();
        }

        protected void PopulateFields(SqlConnection conn, Container theContainer)
        {
            this.txtMetadata_Name.Text = theContainer.GetMetadataValue("title");
            this.txtMetadata_Acronym.Text = theContainer.GetMetadataValue("acronym");
            this.txtMetadata_Description.Text = theContainer.GetMetadataValue("description");
            this.txtMetadata_LogoURL.Text = theContainer.GetMetadataValue("logourl");
            this.txtMetadata_SmallLogoURL.Text = theContainer.GetMetadataValue("small_logo_url");
            this.txtMetadata_URL.Text = theContainer.GetMetadataValue("url");
            bool can_edit = false;
            if (CurrentUser != null && CurrentUser.IsAdministrator)
            {
                can_edit = true;
            }
            else if (CurrentUser == null)
            {
                can_edit = false;
            }
            else
            {
                foreach (PersonRelationship p in theContainer.Relationships)
                {
                    if (p.person.ID == CurrentUser.ID)
                    {
                        can_edit = true;
                        break;
                    }
                }
            }
            this.txtMetadata_DatabaseName.Text = theContainer.override_database_name;
            if (can_edit)
                txtMetadata_DatabaseName.ReadOnly = false;
            else
                txtMetadata_DatabaseName.ReadOnly = true;

            this.lblMetadata_projectid.Text = theContainer.ID.ToString();
            chkIsPublic.Checked = theContainer.IsPublic;
            chkIsSeparateDatabase.Checked = theContainer.IsSeparateDatabase;
            if (!string.IsNullOrEmpty(theContainer.GetMetadataValue("startdate")))
            {
                this.controlStartDate.SelectedDate = DateTime.Parse(theContainer.GetMetadataValue("startdate"));
            }
            if (!string.IsNullOrEmpty(theContainer.GetMetadataValue("enddate")))
            {
                this.controlEndDate.SelectedDate = DateTime.Parse(theContainer.GetMetadataValue("enddate"));
            }
            if (theContainer.ID != Guid.Empty)
            {
                this.PopulateDatabaseList(conn, theContainer);
            }
            chooser.PopulateChooser(conn, theContainer);
            PopulateAttachments(theContainer);
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
                this.chkIsSeparateDatabase.Enabled = false;
                chooser.ReadOnly = true;
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
                this.chkIsSeparateDatabase.Enabled = true;
                chooser.ReadOnly = false;
            }
        }

        // Process the "upload" button, which adds an attachment to the request.
        protected void doUpload(object sender, EventArgs e)
        {
            SqlConnection conn = ConnectToConfigString("RepositoryConnection");
            Container working = (Container)base.GetSessionValue("WorkingContainer");
            try
            {
                // Create a new attached file.
                AttachedFile af = new AttachedFile();
                af.ID = Guid.NewGuid();
                string filepath = GetApplicationSetting("filesavepath") + af.ID.ToString();
                af.Filename = uploadAttachment.FileName;
                af.Path = filepath;
                // Save the attachment.
                uploadAttachment.SaveAs(filepath);
                // working.attachments.Clear();
                working.attachments.Add(af);
                // PopulateData(conn, working, false);
                SetSessionValue("WorkingContainer", working);
                this.PopulateFields(conn, working);
                // PopulateData(conn, working, true);
            }
            catch (Exception ex)
            {
                ShowAlert("Could not upload file: " + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Populate the list of attachments for this current request
        /// </summary>
        /// <param name="working">The current request.</param>
        protected void PopulateAttachments(Container working)
        {
            if (working.attachments.Count > 0)
            {
                string linkurl = "<table border='0'>";
                foreach (AttachedFile f in working.attachments)
                {
                    string download_link = "<a href='DownloadAttachedFile.aspx?ATTACHMENTID=" + f.ID.ToString() + "' target='_blank'>Download " + f.Filename + "</a>";
                    string remove_link = "<a href='javascript:removeAttachment(\"" + f.ID.ToString() + "\")'>Remove " + f.Filename + "</a>";
                    bool can_remove = false;
                    if (IsAuthenticated && CurrentUser != null)
                    {
                        if (CurrentUser.IsAdministrator)
                        {
                            can_remove = true;
                        }
                        else
                        {
                            foreach (PersonRelationship p in working.Relationships)
                            {
                                if (p.person.ID == CurrentUser.ID)
                                {
                                    can_remove = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!can_remove)
                    {
                        remove_link = string.Empty;
                    }
                    // It's just a table; add a download link in a table cell for each row.
                    linkurl += "<tr><td>" + download_link + "</td><td>" + remove_link + "</td></tr>";
                }
                linkurl += "</table>";
                if (filedownloadlink != null)
                {
                    filedownloadlink.InnerHtml = linkurl;
                }
            }
            else
            {
                if (filedownloadlink != null)
                {
                    filedownloadlink.InnerHtml = string.Empty;
                }
            }
        }

        // Remove an attachment.
        protected void RemoveAttachment(Guid attachmentid)
        {
            SqlConnection conn = ConnectToConfigString("RepositoryConnection");
            try
            {
                Container working = (Container)base.GetSessionValue("WorkingContainer");
                for (int i = 0; i < working.attachments.Count; i++)
                {
                    AttachedFile af = working.attachments[i];
                    if (af.ID == attachmentid)
                    {
                        af.DeleteLocalCopy();
                        working.attachments.RemoveAt(i);
                        break;
                    }
                }
                PopulateFields(conn, working);
                if (working.ID != Guid.Empty)
                {
                    working.Save(conn);
                }
            }
            catch (Exception ex)
            {
                ShowAlert("An error occurred: " + ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
    }
}