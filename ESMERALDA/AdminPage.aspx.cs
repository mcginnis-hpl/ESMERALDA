using ESMERALDAClasses;
using SlimeeLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class AdminPage : ESMERALDAPage
    {
        protected void btn_SavProject_Click(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Project theProject = (Project)base.GetSessionValue("WorkingProject");
            if (theProject == null)
            {
                theProject = new Project();
            }
            theProject.project_name = this.txtProject_Name.Text;
            theProject.acronym = this.txtProject_Acronym.Text;
            theProject.description = this.txtProject_Description.Text;
            theProject.logo_url = this.txtProject_LogoURL.Text;
            theProject.override_database_name = this.txtProject_DatabaseName.Text;
            theProject.small_logo_url = this.txtProject_SmallLogoURL.Text;
            theProject.project_url = this.txtProject_URL.Text;
            theProject.start_date = this.controlStartDate.SelectedDate;
            theProject.end_date = this.controlEndDate.SelectedDate;
            if (theProject.Owner == null)
            {
                theProject.Owner = base.CurrentUser;
            }
            Program p = (Program)base.GetSessionValue("WorkingProgram");
            theProject.parentProgram = p;
            theProject.Save(conn);
            base.SetSessionValue("WorkingProject", theProject);
            this.PopulateFields(conn, -1);
            conn.Close();
        }

        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            if (working == null)
            {
                working = new Dataset();
            }
            int edit_row = -1;
            if (!string.IsNullOrEmpty(this.fieldCommands.Value))
            {
                char[] delim = new char[] { ':' };
                edit_row = int.Parse(this.fieldCommands.Value.Split(delim)[1]);
                this.fieldCommands.Value = string.Empty;
            }
            Field f = new Field
            {
                Name = this.txtRowName.Text
            };
            this.txtRowName.Text = string.Empty;
            f.DBType = (Field.FieldType)int.Parse(this.comboRowType.SelectedValue);
            this.comboRowType.SelectedIndex = 0;
            Guid fid = new Guid(this.selectedMetric.Value);
            foreach (Metric m in base.Metrics)
            {
                if (m.ID == fid)
                {
                    f.FieldMetric = m;
                    break;
                }
            }
            this.comboRowMetric.SelectedIndex = 0;
            f.SourceColumnName = this.txtRowSourceColumn.Text;
            this.txtRowSourceColumn.Text = string.Empty;
            f.SQLColumnName = this.txtRowSQLColumn.Text;
            this.txtRowSQLColumn.Text = string.Empty;
            f.Metadata.description = this.txtRowCitations.Text;
            this.txtRowCitations.Text = string.Empty;
            f.Metadata.analysis_methodology = this.txtRowAnalysis.Text;
            this.txtRowAnalysis.Text = string.Empty;
            f.Metadata.instrument = this.txtRowInstrument.Text;
            this.txtRowInstrument.Text = string.Empty;
            f.Metadata.observation_methodology = this.txtRowObservation.Text;
            this.txtRowObservation.Text = string.Empty;
            f.Metadata.processing_methodology = this.txtRowProcessing.Text;
            this.txtRowProcessing.Text = string.Empty;
            if (edit_row >= 0)
            {
                working.Header[edit_row] = f;
            }
            else
            {
                working.Header.Add(f);
            }
            base.SetSessionValue("WorkingDataset", working);
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateFields(conn, -1);
            conn.Close();
        }

        protected void btnSaveDataset_Click(object sender, EventArgs e)
        {
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            Project theProject = (Project)base.GetSessionValue("WorkingProject");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            working.AcquisitionDescription = this.txtDataset_Acquisition.Text;
            working.Description = this.txtDataset_Description.Text;
            working.Name = this.txtDataset_Name.Text;
            working.ProcessingDescription = this.txtDataset_Processing.Text;
            working.BriefDescription = this.txtDataset_ShortDescription.Text;
            working.URL = this.txtDataset_URL.Text;
            working.SQLName = this.txtDataset_TableName.Text;
            working.ParentProject = theProject;
            char[] delim = new char[] { ',' };
            working.Keywords.Clear();
            if (!string.IsNullOrEmpty(this.txtDataset_Keywords.Text))
            {
                string[] tokens = this.txtDataset_Keywords.Text.Split(delim);
                if (tokens.Length > 0)
                {
                    foreach (string s in tokens)
                    {
                        string s2 = s.Trim();
                        if (!string.IsNullOrEmpty(s2))
                        {
                            working.Keywords.Add(s2);
                        }
                    }
                }
            }
            working.Save(conn);
            working.UpdateBounds(conn);
            base.SetSessionValue("WorkingDataSet", working);
            this.PopulateFields(conn, -1);
            conn.Close();
        }

        protected void btnSaveProgram_Click(object sender, EventArgs e)
        {
            Program working = (Program)base.GetSessionValue("WorkingProgram");
            if (working == null)
            {
                working = new Program();
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            working.program_name = this.txtProgram_Name.Text;
            working.acronym = this.txtProgram_Acronym.Text;
            working.description = this.txtProgram_Description.Text;
            working.logo_url = this.txtProgram_LogoURL.Text;
            working.small_logo_url = this.txtProgram_SmallLogoURL.Text;
            working.program_url = this.txtProgram_URL.Text;
            working.start_date = this.controlStartDate.SelectedDate;
            working.end_date = this.controlEndDate.SelectedDate;
            working.database_name = this.txtProgram_DatabaseName.Text;
            if (working.Owner == null)
            {
                working.Owner = base.CurrentUser;
            }
            working.Save(conn);
            base.SetSessionValue("WorkingProgram", working);
            this.PopulateFields(conn, -1);
            conn.Close();
        }

        protected void BuildDisplayDataTable(SqlConnection conn, Dataset working, int edit_index)
        {
            int j = 0;
            TableRow editrow = null;
            while (j < this.tblSpecification.Rows.Count)
            {
                if (this.tblSpecification.Rows[j].ID == "EditRow")
                {
                    editrow = this.tblSpecification.Rows[j];
                }
                if (this.tblSpecification.Rows[j].ID == "headerRow")
                {
                    j++;
                }
                else
                {
                    this.tblSpecification.Rows.RemoveAt(j);
                }
            }
            for (int i = 0; i < working.Header.Count; i++)
            {
                if (i == edit_index)
                {
                    this.txtRowName.Text = working.Header[i].Name;
                    for (int k = 0; k < this.comboRowType.Items.Count; k++)
                    {
                        if (this.comboRowType.Items[k].Value == ((int)working.Header[i].DBType).ToString())
                        {
                            this.comboRowType.SelectedIndex = k;
                            break;
                        }
                    }
                    base.ClientScript.RegisterStartupScript(base.GetType(), "MetricInit", "<script language='JavaScript'>populateMetrics('" + working.Header[i].FieldMetric.ID.ToString() + "');</script>");
                    this.txtRowSourceColumn.Text = ((Field)working.Header[i]).SourceColumnName;
                    this.txtRowSQLColumn.Text = working.Header[i].SQLColumnName;
                    this.txtRowInstrument.Text = ((Field)working.Header[i]).Metadata.instrument;
                    this.txtRowObservation.Text = ((Field)working.Header[i]).Metadata.observation_methodology;
                    this.txtRowAnalysis.Text = ((Field)working.Header[i]).Metadata.analysis_methodology;
                    this.txtRowProcessing.Text = ((Field)working.Header[i]).Metadata.processing_methodology;
                    this.txtRowCitations.Text = ((Field)working.Header[i]).Metadata.description;
                    this.tblSpecification.Rows.Add(editrow);
                    continue;
                }
                TableRow tr = new TableRow();
                TableCell tc = new TableCell
                {
                    Text = working.Header[i].Name
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = working.Header[i].DBType.ToString()
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = working.Header[i].FieldMetric.Name
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).SourceColumnName
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = working.Header[i].SQLColumnName
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).Metadata.instrument
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).Metadata.observation_methodology
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).Metadata.analysis_methodology
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).Metadata.processing_methodology
                };
                tr.Cells.Add(tc);
                tc = new TableCell
                {
                    Text = ((Field)working.Header[i]).Metadata.description
                };
                tr.Cells.Add(tc);
                tc = new TableCell();
                string links = "<a href='javascript:RemoveRow(" + i.ToString() + ")'>Delete</a>&nbsp;&nbsp;<a href='javascript:EditRow(" + i.ToString() + ")'>Edit</a>";
                tc.Text = links;
                tr.Cells.Add(tc);
                this.tblSpecification.Rows.Add(tr);
            }
            if (edit_index < 0)
            {
                this.tblSpecification.Rows.Add(editrow);
            }
        }

        protected void DeleteRow(int i)
        {
            Dataset ds = (Dataset)base.GetSessionValue("WorkingDataSet");
            ds.Header.RemoveAt(i);
            base.SetSessionValue("WorkingDataSet", ds);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                base.RemoveSessionValue("WorkingProgram");
                base.RemoveSessionValue("WorkingProject");
                base.RemoveSessionValue("WorkingDataSet");
            }
            int edit_row = -1;
            if (!string.IsNullOrEmpty(this.fieldCommands.Value))
            {
                char[] delim = new char[] { ':' };
                string[] tokens = this.fieldCommands.Value.Split(delim);
                if (tokens[0] == "DELETE")
                {
                    this.DeleteRow(int.Parse(tokens[1]));
                }
                else if (tokens[0] == "EDIT")
                {
                    edit_row = int.Parse(tokens[1]);
                }
            }
            this.PopulateMetrics(base.Metrics);
            if (edit_row >= 0)
            {
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                this.PopulateFields(conn, edit_row);
                conn.Close();
            }
        }

        protected void PopulateDatasetFields(SqlConnection conn, Dataset working, int edit_row)
        {
            this.PopulateDatasetMetadataFields(conn, working);
            this.BuildDisplayDataTable(conn, working, edit_row);
        }

        protected void PopulateDatasetMetadataFields(SqlConnection conn, Dataset working)
        {
            DateTime starttime = DateTime.Now;
            this.txtDataset_Acquisition.Text = working.AcquisitionDescription;
            this.txtDataset_Description.Text = working.Description;
            this.txtDataset_Name.Text = working.Name;
            this.txtDataset_Processing.Text = working.ProcessingDescription;
            this.txtDataset_ShortDescription.Text = working.BriefDescription;
            this.txtDataset_URL.Text = working.URL;
            this.txtDataset_ID.Text = working.ID.ToString();
            string keywordstring = string.Empty;
            if (working.Keywords.Count > 0)
            {
                keywordstring = working.Keywords[0];
                for (int i = 1; i < working.Keywords.Count; i++)
                {
                    keywordstring = keywordstring + ", " + working.Keywords[i];
                }
            }
            this.txtDataset_Keywords.Text = keywordstring;
        }

        protected void PopulateFields(SqlConnection conn, int edit_row)
        {
            Program p = (Program)base.GetSessionValue("WorkingProgram");
            if (p != null)
            {
                this.PopulateProgramFields(conn, p);
            }
            Project proj = (Project)base.GetSessionValue("WorkingProject");
            if (proj != null)
            {
                this.PopulateProjectFields(conn, proj);
            }
            Dataset ds = (Dataset)base.GetSessionValue("WorkingDataSet");
            if (ds != null)
            {
                this.PopulateDatasetFields(conn, ds, edit_row);
            }
        }

        protected void PopulateMetrics(List<Metric> metrics)
        {
            if (metrics.Count == 0)
            {
                this.newMetrics.Value = "";
            }
            else
            {
                string tmpval = "Generic Text||e903e4f4-3139-4179-a03f-559649f633d4|3|0";
                tmpval = (tmpval + ";UTC|UTC|cb35010b-1b49-40e9-bee6-f5cc9400d175|4|0") + ";Generic Integer||bbb6dfd7-ac14-4566-8737-5f4cf9eb0e6b|1|0" + ";Generic Decimal||930310cd-c8fc-4738-841b-ed422516adf0|2|0";
                List<Guid> generics = new List<Guid> {
                    new Guid("e903e4f4-3139-4179-a03f-559649f633d4"),
                    new Guid("cb35010b-1b49-40e9-bee6-f5cc9400d175"),
                    new Guid("bbb6dfd7-ac14-4566-8737-5f4cf9eb0e6b"),
                    new Guid("930310cd-c8fc-4738-841b-ed422516adf0")
                };
                this.newMetrics.Value = tmpval;
                for (int i = 0; i < metrics.Count; i++)
                {
                    if (!generics.Contains(metrics[i].ID))
                    {
                        tmpval = metrics[i].Name + "|" + metrics[i].Abbrev + "|" + metrics[i].ID.ToString() + "|" + ((int)metrics[i].DataType).ToString() + "|0";
                        this.newMetrics.Value = this.newMetrics.Value + ";" + tmpval;
                    }
                }
            }
        }

        protected void PopulateProgramFields(SqlConnection conn, Program working)
        {
            this.txtProgram_Name.Text = working.program_name;
            this.txtProgram_Acronym.Text = working.acronym;
            this.txtProgram_Description.Text = working.description;
            this.txtProgram_LogoURL.Text = working.logo_url;
            this.txtProgram_SmallLogoURL.Text = working.small_logo_url;
            this.txtProgram_URL.Text = working.program_url;
            this.txtProgram_DatabaseName.Text = working.database_name;
            if (working.start_date > DateTime.MinValue)
            {
                this.controlStartDate.SelectedDate = working.start_date;
            }
            if (working.end_date > DateTime.MinValue)
            {
                this.controlEndDate.SelectedDate = working.end_date;
            }
            this.txtProgram_ID.Text = working.ID.ToString();
        }

        protected void PopulateProjectFields(SqlConnection conn, Project theProject)
        {
            this.txtProject_Name.Text = theProject.project_name;
            this.txtProject_Acronym.Text = theProject.acronym;
            this.txtProject_Description.Text = theProject.description;
            this.txtProject_LogoURL.Text = theProject.logo_url;
            this.txtProject_DatabaseName.Text = theProject.override_database_name;
            this.txtProject_SmallLogoURL.Text = theProject.small_logo_url;
            this.txtProject_URL.Text = theProject.project_url;
            if (theProject.start_date > DateTime.MinValue)
            {
                this.projectStartDate.SelectedDate = theProject.start_date;
            }
            if (theProject.end_date > DateTime.MinValue)
            {
                this.projectEndDate.SelectedDate = theProject.end_date;
            }
            this.txtProject_ID.Text = theProject.ID.ToString();
        }

        protected void txtDataset_ID_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtDataset_ID.Text))
            {
                Guid setID = new Guid(this.txtDataset_ID.Text);
                if (setID != Guid.Empty)
                {
                    SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                    Dataset newdataset = new Dataset();
                    newdataset.Load(conn, setID, base.Conversions, base.Metrics);
                    if (newdataset != null)
                    {
                        base.SetSessionValue("WorkingDataSet", newdataset);
                        this.PopulateFields(conn, -1);
                        conn.Close();
                    }
                }
            }
        }

        protected void txtProgram_ID_TextChanged(object sender, EventArgs e)
        {
            Guid programid = new Guid(this.txtProgram_ID.Text);
            Program theProgram = new Program(); ;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (programid != Guid.Empty)
            {
                theProgram.Load(conn, programid);
            }
            base.SetSessionValue("WorkingProgram", theProgram);
            this.PopulateFields(conn, -1);
            conn.Close();
        }

        protected void txtProject_ID_TextChanged(object sender, EventArgs e)
        {
            Guid projectid = new Guid(this.txtProject_ID.Text);
            Project theProject = new Project();
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (projectid != Guid.Empty)
            {
                theProject.Load(conn, projectid);
            }
            this.PopulateProjectFields(conn, theProject);
            base.SetSessionValue("WorkingProject", theProject);
            this.PopulateFields(conn, -1);
            conn.Close();
        }
    }
}