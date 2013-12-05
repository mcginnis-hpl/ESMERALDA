using AjaxControlToolkit;
using ESMERALDAClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using LumenWorks.Framework.IO.Csv;

namespace ESMERALDA
{
    public partial class EditDataSet : ESMERALDAPage
    {
        protected void btnCreateDataset_Click(object sender, EventArgs e)
        {
            
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Guid myId = (Guid)base.GetSessionValue("SessionID");
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            this.SaveMetadata(conn, working);
            if (working.ID == Guid.Empty || working.IsDirty)
            {
                DataTable data = (DataTable)base.GetSessionValue("TemporaryData");
                int chunk_offset = 0;
                int chunk_size = 1000;
                SqlConnection working_connection = base.ConnectToDatabase(working.ParentContainer.database_name);
                bool created = false;
                if (string.IsNullOrEmpty(working.SQLName))
                {
                    working.SQLName = Utils.CreateUniqueTableName(working.GetMetadataValue("title"), working_connection);
                }
                while (chunk_offset < data.Rows.Count)
                {
                    DataTable newtable = working.BuildDataTable(data, chunk_offset, chunk_size);
                    if (!created)
                    {
                        SqlTransaction tran = working_connection.BeginTransaction();
                        SqlTableCreator table_creator = new SqlTableCreator(working_connection, tran);
                        table_creator.DestinationTableName = working.SQLName;

                        table_creator.Create(SqlTableCreator.GetSchemaTable(newtable));
                        created = true;
                        tran.Commit();
                    }
                    SqlTableCreator creator = new SqlTableCreator(working_connection);
                    creator.DestinationTableName = working.SQLName;
                    creator.WriteData(newtable);
                    chunk_offset += chunk_size;
                }                
                working_connection.Close();
            }
            if (working.Owner == null)
            {
                working.Owner = base.CurrentUser;
            }
            working.Save(conn);
            working.UpdateBounds(conn);
            this.PopulateFields(conn, working);
            conn.Close();
        }

        protected void btnDeleteExistingData_Click(object sender, EventArgs e)
        {
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            Guid myId = (Guid)base.GetSessionValue("SessionID");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            base.RemoveSessionValue("TemporaryData");
            this.PopulateFields(conn, working);
            conn.Close();
        }

        protected void Refresh()
        {
            Debug.WriteLine("Posting back.");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            if (working != null)
            {
                this.PopulateFields(conn, working);
            }
            conn.Close();
            Debug.WriteLine("Done posting back.");
        }

        protected void btnSaveTableConfig_Click(object sender, EventArgs e)
        {
            int i;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            List<Metric> current_metrics = Metric.LoadExistingMetrics(conn);
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            this.SaveMetadata(conn, working);
            string data_config = this.tableSpecification.Value;
            char[] row_delim = new char[] { ';' };
            char[] col_delim = new char[] { '|' };            
            List<Field> new_fields = new List<Field>();

            string[] config_rows = data_config.Split(row_delim);
            for (i = 0; i < config_rows.Length; i++)
            {
                if (string.IsNullOrEmpty(config_rows[i]))
                {
                    continue;
                }
                string[] config_cols = config_rows[i].Split(col_delim);
                if ((config_cols.Length >= 5) && (int.Parse(config_cols[4]) != 0))
                {
                    Field f = new Field
                    {
                        SourceColumnName = config_cols[0],
                        Name = config_cols[1],
                        DBType = (Field.FieldType)int.Parse(config_cols[2])
                    };
                    Guid metricid = new Guid(config_cols[3]);
                    foreach (Metric m in current_metrics)
                    {
                        if (m.ID == metricid)
                        {
                            f.FieldMetric = m;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(config_cols[5]))
                    {
                        f.SubfieldName = config_cols[5];
                    }
                    if (config_cols[6] == "1")
                    {
                        f.IsTiered = true;
                    }
                    else
                    {
                        f.IsTiered = false;
                    }
                    f.Parent = working;
                    new_fields.Add(f);                    
                }
            }
            foreach (Field f in new_fields)
            {
                if (!string.IsNullOrEmpty(f.SubfieldName))
                {
                    foreach (Field f2 in new_fields)
                    {
                        if (f2.SourceColumnName == f.SubfieldName)
                        {
                            f.Subfield = f2;
                            f2.IsSubfield = true;
                            break;
                        }
                    }
                }
            }
            if (working.Header.Count == 0 || working.ID == Guid.Empty || working.IsDirty)
            {
                working.Header.Clear();
                working.Header.AddRange(new_fields);
            }
            else
            {
                List<Field> found_fields = new List<Field>();
                i = 0;
                bool is_dirty = false;
                while(i < new_fields.Count)
                {
                    Field f = new_fields[i];
                    bool found = false;
                    foreach (Field qf in working.Header)
                    {
                        if (qf.SourceColumnName == f.SourceColumnName)
                        {
                            found = true;
                            if (qf.DBType != f.DBType || (qf.Subfield == null && f.Subfield != null) || (qf.Subfield != null && f.Subfield == null) || (f.Subfield != null && qf.Subfield != null && qf.Subfield.ID != f.Subfield.ID))
                            {
                                is_dirty = true;
                            }
                            else
                            {
                                f.SQLColumnName = qf.SQLColumnName;
                            }
                            found_fields.Add(f);
                            new_fields.RemoveAt(i);
                            break;
                        }
                    }
                    if (!found)
                    {
                        i += 1;
                    }
                }
                if (new_fields.Count > 0)
                {
                    is_dirty = true;
                    found_fields.AddRange(new_fields);
                }
                if (is_dirty && !working.IsDirty)
                {
                    working.IsDirty = true;
                    Guid myId = (Guid)base.GetSessionValue("SessionID");
                    DataTable dt = working.MoveExistingDataToTemp(conn, myId, true);
                    base.SetSessionValue("TemporaryData", dt);
                }
                working.Header.Clear();
                working.Header.AddRange(found_fields);
            }
            string metadata_string = this.fieldMetadata.Value;
            char[] row_delim2 = new char[] { '~' };
            char[] col_delim2 = new char[] { '|' };
            string[] meta_rows = metadata_string.Split(row_delim2);
            for (i = 0; i < meta_rows.Length; i++)
            {
                string[] tokens = meta_rows[i].Split(col_delim2);
                foreach (Field f in working.Header)
                {
                    if (f.SourceColumnName == tokens[0])
                    {
                        f.SetMetadataValue("observation_methodology", tokens[1]);
                        f.SetMetadataValue("instrument", tokens[2]);
                        f.SetMetadataValue("analysis_methodology", tokens[3]);
                        f.SetMetadataValue("processing_methodology", tokens[4]);
                        f.SetMetadataValue("citations", tokens[5]);
                        f.SetMetadataValue("description", tokens[6]);
                        break;
                    }
                }
            }
            this.PopulateFields(conn, working);
            conn.Close();
            base.SetSessionValue("WorkingDataSet", working);
        }

        protected void BuildDisplayDataTable(SqlConnection conn, Dataset working)
        {
            DateTime starttime = DateTime.Now;
            List<Metric> metrics = Metric.LoadExistingMetrics(conn);
            string specTable = string.Empty;
            string metadata_table = string.Empty;
            int j = 0;
            for (int i = 0; i < working.Header.Count; i++)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell
                {
                    ID = "header_sourcename" + i.ToString(),
                    Text = ((Field)working.Header[i]).SourceColumnName
                };
                tr.Cells.Add(tc);
                tc = new TableCell();
                TextBox txtName = new TextBox
                {
                    ID = "header_name" + i.ToString(),
                    Text = working.Header[i].Name
                };
                txtName.Attributes.Add("onchange", "updateHeaderField(1, " + i.ToString() + ", '" + txtName.ID + "')");
                tc.Controls.Add(txtName);
                tr.Cells.Add(tc);
                tc = new TableCell();
                DropDownList dl = new DropDownList
                {
                    ID = "header_unit" + i.ToString()
                };
                dl.Items.Add(new ListItem("", "0"));
                dl.Items.Add(new ListItem("Integer", "1"));
                dl.Items.Add(new ListItem("Decimal", "2"));
                dl.Items.Add(new ListItem("Text", "3"));
                dl.Items.Add(new ListItem("Datetime", "4"));
                dl.Items.Add(new ListItem("Time", "6"));
                dl.Attributes.Add("onchange", "updateHeaderField(2, " + i.ToString() + ", '" + dl.ID + "')");
                tc.Controls.Add(dl);
                for (int k = 0; k < dl.Items.Count; k++)
                {
                    if (dl.Items[k].Value == ((int)working.Header[i].DBType).ToString())
                    {
                        dl.SelectedIndex = k;
                        break;
                    }
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                dl = new DropDownList();
                dl.Items.Add(new ListItem("", ""));
                dl.Items.Add(new ListItem("New Metric", "NEW"));
                foreach (Metric m in metrics)
                {
                    if (m.DataType == working.Header[i].DBType)
                    {
                        string tag = m.Name + "(" + m.Abbrev + ")";
                        dl.Items.Add(new ListItem(tag, m.ID.ToString()));
                    }
                }
                dl.ID = "header_metric" + i.ToString();
                dl.Attributes.Add("onchange", "updateHeaderField(3, " + i.ToString() + ", '" + dl.ID + "')");
                dl.SelectedIndex = 0;
                if (working.Header[i].FieldMetric != null)
                {
                    for (j = 0; j < dl.Items.Count; j++)
                    {
                        if (dl.Items[j].Value == working.Header[i].FieldMetric.ID.ToString())
                        {
                            dl.SelectedIndex = j;
                            break;
                        }
                    }
                }
                tc.Controls.Add(dl);
                tr.Cells.Add(tc);
                tc = new TableCell();
                dl = new DropDownList();
                dl.Items.Add(new ListItem("", ""));
                if (working.Header[i].DBType == Field.FieldType.DateTime)
                {
                    foreach (Field f in working.Header)
                    {
                        if (f.DBType == Field.FieldType.Time)
                        {
                            dl.Items.Add(new ListItem(f.Name, f.SourceColumnName));
                            if (((Field)working.Header[i]).Subfield != null && ((Field)working.Header[i]).Subfield.SourceColumnName == f.SourceColumnName)
                            {
                                dl.SelectedIndex = dl.Items.Count - 1;
                                break;
                            }
                        }
                    }
                    dl.Style["display"] = "";
                }
                else
                {
                    dl.Style["display"] = "none";
                }
                dl.ID = "header_subfield" + i.ToString();
                dl.Attributes.Add("onchange", "updateHeaderField(5, " + i.ToString() + ", '" + dl.ID + "')");
                tc.Controls.Add(dl);
                tr.Cells.Add(tc);


                tc = new TableCell();
                CheckBox cb = new CheckBox
                {
                    Checked = true,
                    ID = "header_include" + i.ToString()
                };
                cb.Attributes.Add("onclick", "updateHeaderField(4, " + i.ToString() + ", '" + cb.ID + "')");
                tc.Controls.Add(cb);
                tr.Cells.Add(tc);

                tc = new TableCell();
                cb = new CheckBox
                {
                    Checked = working.Header[i].IsTiered,
                    ID = "header_tiered" + i.ToString()
                };
                cb.Attributes.Add("onchange", "updateHeaderField(6, " + i.ToString() + ", '" + cb.ID + "')");
                tc.Controls.Add(cb);
                tr.Cells.Add(tc);

                tc = new TableCell
                {
                    Text = "<a id='metadata_" + i.ToString() + "' href='javascript:editAddMetadata(\"metadata_" + i.ToString() + "\", \"" + ((Field)working.Header[i]).SourceColumnName + "\")'>Edit Metadata</a>"
                };
                tr.Cells.Add(tc);
                this.tblDataField.Rows.Add(tr);
                string specstring = ((Field)working.Header[i]).SourceColumnName + "|" + working.Header[i].Name + "|" + ((int)working.Header[i].DBType).ToString() + "|";
                if (working.Header[i].FieldMetric != null)
                {
                    specstring = specstring + working.Header[i].FieldMetric.ID.ToString();
                }
                else
                {
                    specstring = specstring + Guid.Empty.ToString();
                }
                specstring = specstring + "|1";
                if (((Field)working.Header[i]).Subfield == null)
                {
                    specstring = specstring + "|";
                }
                else
                {
                    specstring = specstring + "|" + ((Field)working.Header[i]).Subfield.SourceColumnName;
                }
                if (working.Header[i].IsTiered)
                {
                    specstring += "|1";
                }
                else
                {
                    specstring += "|0";
                }

                if (string.IsNullOrEmpty(specTable))
                {
                    specTable = specstring;
                }
                else
                {
                    specTable = specTable + ";" + specstring;
                }
                string metadata_string = ((Field)working.Header[i]).SourceColumnName + "|" + ((Field)working.Header[i]).GetMetadataValue("observation_methodology") + "|" + ((Field)working.Header[i]).GetMetadataValue("instrument") + "|" + ((Field)working.Header[i]).GetMetadataValue("analysis_methodology") + "|" + ((Field)working.Header[i]).GetMetadataValue("processing_methodology") + "|" + ((Field)working.Header[i]).GetMetadataValue("citations") + "|" + ((Field)working.Header[i]).GetMetadataValue("description");
                if (string.IsNullOrEmpty(metadata_table))
                {
                    metadata_table = metadata_string;
                }
                else
                {
                    metadata_table = metadata_table + "~" + metadata_string;
                }
            }
            this.tableSpecification.Value = specTable;
            this.fieldMetadata.Value = metadata_table;
            this.PopulateMetrics(metrics);
        }

        protected void BuildPreview(SqlConnection conn, Dataset working)
        {
            this.previewTable.Rows.Clear();
            DateTime starttime = DateTime.Now;
            Guid myId = (Guid)base.GetSessionValue("SessionID");
            DataTable data = (DataTable)base.GetSessionValue("TemporaryData");
            if (data != null)
            {
                DataTable parsed_data = working.BuildDataTable(data, -1, 0x3e8);
                if (parsed_data.Rows.Count != 0)
                {
                    int i;
                    TableHeaderRow thr = new TableHeaderRow();
                    for (i = 0; i < parsed_data.Columns.Count; i++)
                    {
                        TableHeaderCell thc = new TableHeaderCell
                        {
                            Text = parsed_data.Columns[i].ColumnName
                        };
                        thr.Cells.Add(thc);
                    }
                    this.previewTable.Rows.Add(thr);
                    List<string[]> newrows = new List<string[]>();
                    TableRow tr = null;
                    TableCell td = null;
                    for (i = 0; i < parsed_data.Rows.Count; i++)
                    {
                        if (i == 200)
                        {
                            break;
                        }
                        tr = new TableRow();
                        for (int j = 0; j < parsed_data.Columns.Count; j++)
                        {
                            td = new TableCell();
                            if (parsed_data.Rows[i][j] == DBNull.Value)
                            {
                                td.Text = string.Empty;
                            }
                            else
                            {
                                td.Text = parsed_data.Rows[i][j].ToString();
                            }
                            tr.Cells.Add(td);
                        }
                        this.previewTable.Rows.Add(tr);
                    }
                    this.saveControl.Style["display"] = "";
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsAuthenticated)
            {
                string url = "Default.aspx";
                Response.Redirect(url, false);
                return;
            }
            if (UserIsAdministrator)
            {
                pasteDataSpecification.Visible = true;
            }
            else
            {
                pasteDataSpecification.Visible = false;
            }
            int i;
            SqlConnection conn;
            this.saveControl.Style["display"] = "none";
            if (!base.IsPostBack)
            {
                base.RepositoryInit();
                if (base.GetSessionValue("SessionID") == null)
                {
                    base.SetSessionValue("SessionID", Guid.NewGuid());
                }
                this.SetDivVisibility(null);
                base.SetSessionValue("UploadedFiles", new List<string>());
                base.RemoveSessionValue("WorkingDataSet");
                base.RemoveSessionValue("TemporaryData");
                Guid setID = Guid.Empty;
                for (i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "DATASETID")
                    {
                        setID = new Guid(base.Request.Params[i]);
                    }
                }
                if (setID != Guid.Empty)
                {
                    conn = base.ConnectToConfigString("RepositoryConnection");
                    List<Metric> current_metrics = base.Metrics;
                    Dataset newdataset = new Dataset();
                    newdataset.Load(conn, setID, null, current_metrics);
                    if (newdataset != null)
                    {
                        Guid myId = (Guid)base.GetSessionValue("SessionID");
                        DataTable dt = newdataset.MoveExistingDataToTemp(conn, myId, false);
                        base.SetSessionValue("TemporaryData", dt);
                        this.PopulateFields(conn, newdataset);
                        conn.Close();
                        base.SetSessionValue("WorkingDataSet", newdataset);
                    }
                }
                
            }
            else
            {
                Debug.WriteLine("Posting back.");
                conn = null;
                Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
                if (working != null)
                    PopulateAttachments(working);
                if (!string.IsNullOrEmpty(this.newMetrics.Value))
                {
                    conn = base.ConnectToConfigString("RepositoryConnection");
                    this.SaveNewMetrics(this.newMetrics.Value, conn);
                }
                if (this.hiddenCommands.Value == "REFRESH")
                {
                    this.hiddenCommands.Value = string.Empty;                    
                    if (working != null)
                    {
                        if (conn == null)
                        {
                            conn = base.ConnectToConfigString("RepositoryConnection");
                        }
                        this.PopulateFields(conn, working);
                    }
                    List<string> sheets = (List<string>)GetSessionValue("SheetsToPick");
                    if (sheets != null)
                    {
                        comboSpreadsheetSheets.Items.Clear();
                        foreach (string s in sheets)
                        {
                            comboSpreadsheetSheets.Items.Add(new ListItem(s, s));
                        }
                        RemoveSessionValue("SheetsToPick");
                        spreadsheetSheetPicker.Style["display"] = "inline";
                        // AddStartupCall("showSheetSelector();", "showPicker");
                    }
                }
                if (conn != null)
                {
                    conn.Close();
                }
                Debug.WriteLine("Done posting back.");                
            }
        }

        protected void PopulateFields(SqlConnection conn, Dataset working)
        {
            this.PopulateMetadataField(conn, working);
            this.BuildDisplayDataTable(conn, working);
            this.PopulateAttachments(working);
            if (working.IsDefined)
            {
                this.BuildPreview(conn, working);
            }
            this.SetDivVisibility(working);
            List<string> uploads = (List<string>)base.GetSessionValue("UploadedFiles");
            this.PopulateUploads(uploads);
            if (!string.IsNullOrEmpty(working.SQLName))
            {
                this.btnDeleteExistingData.Visible = true;
            }
            else
            {
                this.btnDeleteExistingData.Visible = false;
            }
        }

        protected void PopulateMetadataField(SqlConnection conn, Dataset working)
        {
            int i;
            DateTime starttime = DateTime.Now;
            Guid containerid = Guid.Empty;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Request.Params[i]) && (base.Request.Params.GetKey(i).ToUpper() == "CONTAINERID"))
                {
                    containerid = new Guid(base.Request.Params[i]);
                }
            }
            this.txtMetadata_Acquisition.Text = working.GetMetadataValue("acqdesc");
            this.txtMetadata_Description.Text = working.GetMetadataValue("abstract");
            this.txtMetadata_Name.Text = working.GetMetadataValue("title");
            this.txtMetadata_Processing.Text = working.GetMetadataValue("procdesc");
            this.txtMetadata_ShortDescription.Text = working.GetMetadataValue("purpose");
            this.txtMetadata_URL.Text = working.GetMetadataValue("url");
            string directlink = "<a href=\"http://hpldata.hpl.umces.edu/Default.aspx?ENTITYID=" + working.ID.ToString() + "\">" + working.ID.ToString() + "</a>";
            this.lblMetadata_DatasetID.Text = directlink;
            string keywordstring = string.Empty;
            List<string> keywords = working.GetMetadataValueArray("keyword");
            if (keywords.Count > 0)
            {
                keywordstring = keywords[0];
                for (i = 1; i < keywords.Count; i++)
                {
                    keywordstring = keywordstring + ", " + keywords[i];
                }
            }
            this.txtKeywords.Text = keywordstring;
            chkIsPublic.Checked = working.IsPublic;            
            chooser.PopulateChooser(conn, working);
            metadata_picker.PopulateMetadata(working);
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
                tmpval = (tmpval + ";UTC|UTC|cb35010b-1b49-40e9-bee6-f5cc9400d175|4|0" + ";Year Day||88f9145a-fb19-455d-9c9d-0432603a332e|4|0") + ";Generic Integer||bbb6dfd7-ac14-4566-8737-5f4cf9eb0e6b|1|0" + ";Generic Decimal||930310cd-c8fc-4738-841b-ed422516adf0|2|0";
                List<Guid> generics = new List<Guid> {
                    new Guid("e903e4f4-3139-4179-a03f-559649f633d4"),
                    new Guid("88f9145a-fb19-455d-9c9d-0432603a332e"),
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

        protected void PopulateUploads(List<string> files)
        {
            this.uploadedFiles.Rows.Clear();
            TableHeaderRow thr = new TableHeaderRow();
            TableHeaderCell thc = new TableHeaderCell
            {
                Text = "Uploaded Files"
            };
            thr.Cells.Add(thc);
            this.uploadedFiles.Rows.Add(thr);
            foreach (string s in files)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell
                {
                    Text = s
                };
                tr.Cells.Add(tc);
                this.uploadedFiles.Rows.Add(tr);
            }
        }

        protected void ShowError(string msg)
        {
            lblError.Text = msg;
            
        }

        protected void ProcessUpload(object sender, AsyncFileUploadEventArgs e)
        {
            DateTime starttime = DateTime.Now;
            DateTime functionstarttime = DateTime.Now;
            Debug.WriteLine("Loading file.");
            int header_row = -1;
            if (!string.IsNullOrEmpty(txtHeaderRow_Upload.Text))
            {
                try
                {
                    header_row = int.Parse(txtHeaderRow_Upload.Text);
                }
                catch (FormatException)
                {
                }
            }
            if (this.uploadFiles2.PostedFile != null)
            {
                HttpPostedFile userPostedFile = this.uploadFiles2.PostedFile;
                string filename = userPostedFile.FileName;
                string mimetype = userPostedFile.ContentType;
                if ((filename.EndsWith(".csv") || (mimetype == "text/csv")) || ((mimetype == "text/comma-separated-values") || filename.EndsWith(".dat")) || (mimetype == "ext/tab-separated-values") || filename.EndsWith(".txt"))
                {
                    try
                    {
                        if (userPostedFile.ContentLength > 0)
                        {
                            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
                            DataTable saved_table = (DataTable)base.GetSessionValue("TemporaryData");
                            string curr = string.Empty;
                            StreamReader sr = new StreamReader(userPostedFile.InputStream, System.Text.Encoding.Default, true);
                            FinishProcessingUpload(sr, ref working, ref saved_table);
                            sr.Close();
                            base.SetSessionValue("TemporaryData", saved_table);
                        }
                    }
                    catch (Exception exception1)
                    {
                        ShowError(exception1.Message + " " + exception1.StackTrace);
                    }
                }
                else if (filename.EndsWith(".xls") || (mimetype == "application/vnd.ms-excel") || mimetype == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || filename.EndsWith(".xlsx"))
                {
                    try
                    {
                        if (userPostedFile.ContentLength > 0)
                        {
                            string extension = filename.Substring(filename.LastIndexOf('.'));
                            string save_dir = GetApplicationSetting("incomingpath");
                            string tmpfilename = save_dir + Guid.NewGuid().ToString() + extension;
                            SetSessionValue("LastTempFile", tmpfilename);
                            FileStream fs = File.OpenWrite(tmpfilename);
                            byte[] buffer = new byte[userPostedFile.ContentLength];
                            userPostedFile.InputStream.Read(buffer, 0, userPostedFile.ContentLength);
                            fs.Write(buffer, 0, userPostedFile.ContentLength);
                            fs.Flush();
                            fs.Close();
                            OleDbConnection connection = null;
                            if (extension == ".xls")
                            {
                                connection = new OleDbConnection(string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", tmpfilename));
                            }
                            else if (extension == ".xlsx")
                            {
                                connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + tmpfilename + ";Extended Properties='Excel 12.0 xml;HDR=YES;'");
                            }
                            connection.Open();
                            List<string> sheets = new List<string>();
                            foreach (DataRow r in connection.GetSchema("Tables").Rows)
                            {
                                if (!string.IsNullOrEmpty((string)r["TABLE_NAME"]))
                                    sheets.Add((string)r["TABLE_NAME"]);
                            }
                            if (sheets.Count > 1)
                            {
                                SetSessionValue("SheetsToPick", sheets);
                                SetSessionValue("postedfile", userPostedFile.FileName);
                                connection.Close();
                            }
                            else
                            {
                                string myTableName = (string)connection.GetSchema("Tables").Rows[0]["TABLE_NAME"];
                                OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + myTableName + "]", connection);
                                DataSet ds = new DataSet();
                                adapter.Fill(ds, "anyNameHere");
                                connection.Close();
                                DataTable data = ds.Tables[0];
                                string data_string = Utils.ToCSV(data);
                                StringReader sr = new StringReader(data_string);
                                Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
                                DataTable saved_table = (DataTable)base.GetSessionValue("TemporaryData");
                                FinishProcessingUpload(sr, ref working, ref saved_table);
                                sr.Close();
                            }
                        }
                    }
                    catch (Exception exception1)
                    {
                        ShowError(exception1.Message + " " + exception1.StackTrace);
                    }
                }
            }            
        }

        protected void FinishProcessingUpload(TextReader in_reader, ref Dataset working, ref DataTable saved_table)
        {
            int i;
            string[] header_fields;
            int j;
            List<string> missing_fields;
            List<string> extra_fields;
            string msg;
            
            DateTime functionstarttime = DateTime.Now;
            DateTime starttime = DateTime.Now;
            TimeSpan debugtime = (TimeSpan)(DateTime.Now - starttime);
            starttime = DateTime.Now;
            Debug.WriteLine("Time to upload: " + ((int)debugtime.TotalMilliseconds).ToString() + "ms");

            CsvReader csv = new CsvReader(in_reader, true, true);            
            csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
            Guid containerid = Guid.Empty;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Request.Params[i]) && (base.Request.Params.GetKey(i).ToUpper() == "CONTAINERID"))
                {
                    containerid = new Guid(base.Request.Params[i]);
                }
            }
            if (working == null)
            {
                working = new Dataset();
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            missing_fields = new List<string>();
            extra_fields = new List<string>();
            if ((working.Header != null) && (working.Header.Count > 0))
            {
                foreach (Field f in working.Header)
                {
                    missing_fields.Add(f.SourceColumnName);
                }
                string[] file_headers = csv.GetFieldHeaders();
                for (i = 0; i < file_headers.Length; i++)
                {
                    if (!missing_fields.Contains(file_headers[i]))
                    {
                        extra_fields.Add(file_headers[i]);
                    }
                    else
                    {
                        missing_fields.Remove(file_headers[i]);
                    }                
                }
            }
            if (working.Header == null || working.Header.Count == 0)
            {
                working.Header = new List<QueryField>();
                header_fields = csv.GetFieldHeaders();
                for (j = 0; j < header_fields.Length; j++)
                {
                    if (!string.IsNullOrEmpty(header_fields[j].Trim()))
                    {
                        Field f = new Field
                        {
                            SourceColumnName = header_fields[j].Trim()
                        };
                        f.Name = Field.ExtractColumnName(f.SourceColumnName);
                        f.FieldMetric = Field.RecommendMetric(f.SourceColumnName, base.Metrics, null);
                        if (f.FieldMetric != null)
                            f.DBType = f.FieldMetric.DataType;
                        f.Parent = working;
                        working.Header.Add(f);
                    }
                }                
            }
            else
            {
                if (missing_fields.Count > 0)
                {
                    if (saved_table == null || saved_table.Rows.Count == 0)
                    {
                        foreach (string t in missing_fields)
                        {
                            int k = 0;
                            while (k < working.Header.Count)
                            {
                                if(((Field)working.Header[k]).SourceColumnName == t)
                                {
                                    working.Header.RemoveAt(k);
                                    break;
                                }
                                k += 1;
                            }
                        }
                    }
                    else
                    {
                        msg = "The new data is missing the following columns; these columns will be null in the new rows: " + missing_fields[0];
                        for (i = 1; i < missing_fields.Count; i++)
                        {
                            msg = msg + ", " + missing_fields[i];
                        }
                        base.ShowAlert(msg);
                    }
                }
                if (extra_fields.Count > 0)
                {
                    msg = "The new data has the following extra columns; these fields will be null in any existing rows: " + extra_fields[0];
                    for (i = 1; i < extra_fields.Count; i++)
                    {
                        msg = msg + ", " + extra_fields[i];
                    }
                    foreach (string s in extra_fields)
                    {
                        Field f = new Field
                        {
                            SourceColumnName = s.Trim()
                        };
                        f.Name = Field.ExtractColumnName(f.SourceColumnName);
                        f.FieldMetric = Field.RecommendMetric(f.SourceColumnName, base.Metrics, null);
                        if (f.FieldMetric != null)
                            f.DBType = f.FieldMetric.DataType;
                        f.Parent = working;
                        working.Header.Add(f);
                    }
                    base.ShowAlert(msg);
                }
            }
            if (containerid != Guid.Empty)
            {
                working.ParentEntity = new Container();
                working.ParentEntity.Load(conn, containerid);
            }
            working.IsDirty = true;
            base.SetSessionValue("WorkingDataSet", working);

            saved_table = working.SaveTemporaryData(csv, saved_table);
            foreach (Field f in working.Header)
            {
                if (f.FieldMetric == null)
                {
                    f.FieldMetric = Field.RecommendMetric(f.SourceColumnName, base.Metrics, saved_table);
                    if(f.FieldMetric != null)
                        f.DBType = f.FieldMetric.DataType;
                }
            }
            debugtime = (TimeSpan)(DateTime.Now - starttime);
            starttime = DateTime.Now;
            Debug.WriteLine("Time to save temporary data: " + ((int)debugtime.TotalMilliseconds).ToString() + "ms");
            List<string> uploads = (List<string>)base.GetSessionValue("UploadedFiles");
            if (this.uploadFiles2.PostedFile != null)
            {
                uploads.Add(this.uploadFiles2.PostedFile.FileName);
            }
            else if (GetSessionValue("postedfile") != null)
            {
                uploads.Add((string)GetSessionValue("postedfile"));
                RemoveSessionValue("postedfile");
            }
            base.SetSessionValue("UploadedFiles", uploads);

            Debug.WriteLine("Loading file complete.");
            debugtime = (TimeSpan)(DateTime.Now - functionstarttime);
            Debug.WriteLine("Total upload time: " + ((int)debugtime.TotalMilliseconds).ToString() + "ms");
        }

        protected void SaveMetadata(SqlConnection conn, Dataset working)
        {
            working.SetMetadataValue("acqdesc", this.txtMetadata_Acquisition.Text);
            working.SetMetadataValue("abstract", this.txtMetadata_Description.Text);
            working.SetMetadataValue("title", this.txtMetadata_Name.Text);
            working.SetMetadataValue("procdesc", this.txtMetadata_Processing.Text);
            working.SetMetadataValue("purpose", this.txtMetadata_ShortDescription.Text);
            working.IsPublic = chkIsPublic.Checked;
            working.SetMetadataValue("url", this.txtMetadata_URL.Text);
            char[] delim = new char[] { ',' };
            working.ClearMetadataValue("keyword");
            if (!string.IsNullOrEmpty(this.txtKeywords.Text))
            {
                string[] tokens = this.txtKeywords.Text.Split(delim);
                if (tokens.Length > 0)
                {
                    foreach (string s in tokens)
                    {
                        string s2 = s.Trim();
                        if (!string.IsNullOrEmpty(s2))
                        {
                            working.AddMetadataValue("keyword", s2);
                        }
                    }
                }
            }
            List<Guid> personids = new List<Guid>();
            List<string> rels = new List<string>();
            chooser.GetSelectedItems(personids, rels);
            // metadata_picker.GetSelectedItems(working);
            working.Relationships.Clear();
            for(int i=0; i < personids.Count; i++)
            {
                PersonRelationship pr = new PersonRelationship();
                pr.person = new Person();
                pr.person.Load(conn, personids[i]);
                pr.relationship = rels[i];
                working.Relationships.Add(pr);
            }
        }

        protected void SaveNewMetrics(string inValue, SqlConnection conn)
        {
            char[] delim1 = new char[] { ';' };
            char[] delim2 = new char[] { '|' };
            string[] lines = inValue.Split(delim1);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] tokens = lines[i].Split(delim2);
                if ((tokens.Length >= 3) && !(tokens[4] == "0"))
                {
                    new Metric { Name = tokens[0], Abbrev = tokens[1], ID = new Guid(tokens[2]), DataType = (Field.FieldType)int.Parse(tokens[3]) }.Save(conn);
                }
            }
            base.LoadCommonStructures();
            this.newMetrics.Value = string.Empty;
        }

        protected void SetDivVisibility(Dataset working)
        {
            if (working == null)
            {
                this.metadata.Style["visibility"] = "hidden";
                this.datafields.Style["visibility"] = "hidden";
                this.preview.Style["visibility"] = "hidden";                
            }
            else
            {
                this.metadata.Style["visibility"] = "";
                this.datafields.Style["visibility"] = "";
                this.preview.Style["visibility"] = "";
            }
        }

        protected void btnSelectSheet_Click(object sender, EventArgs e)
        {
            int header_row = -1;
            if (!string.IsNullOrEmpty(txtHeaderRow.Text))
            {
                header_row = int.Parse(txtHeaderRow.Text);
            }
            RemoveStartupCall("showPicker");
            string myTableName = (string)comboSpreadsheetSheets.SelectedValue;
            string tmpfilename = (string)GetSessionValue("LastTempFile");
            string extension = tmpfilename.Substring(tmpfilename.LastIndexOf('.'));
            OleDbConnection connection = null;
            if (extension == ".xls")
            {
                connection = new OleDbConnection(string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", tmpfilename));
            }
            else if (extension == ".xlsx")
            {
                connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + tmpfilename + ";Extended Properties='Excel 12.0 xml;HDR=NO;IMEX=1;'");
            }
            connection.Open();
            List<string> rows = new List<string>();
            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + myTableName + "]", connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "anyNameHere");            
            DataTable data = ds.Tables[0];
            string data_string = Utils.ToCSV(data);
            spreadsheetSheetPicker.Style["display"] = "none";
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            DataTable saved_table = (DataTable)base.GetSessionValue("TemporaryData");
            StringReader sr = new StringReader(data_string);
            FinishProcessingUpload(sr, ref working, ref saved_table);
            sr.Close();
            connection.Close();
            SetSessionValue("TemporaryData", saved_table);
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateFields(conn, working);
            conn.Close();            
        }

        // Process the "upload" button, which adds an attachment to the request.
        protected void doUpload(object sender, EventArgs e)
        {
            SqlConnection conn = ConnectToConfigString("RepositoryConnection");
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
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
                SetSessionValue("WorkingDataSet", working);
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
        protected void PopulateAttachments(Dataset working)
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
                filedownloadlink.InnerHtml = linkurl;
            }
            else
            {
                filedownloadlink.InnerHtml = string.Empty;
            }
        }

        // Remove an attachment.
        protected void RemoveAttachment(Guid attachmentid)
        {
            SqlConnection conn = ConnectToConfigString("RepositoryConnection");
            try
            {
                Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
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

        protected void txtpasteDataSpecification_TextChanged(object sender, EventArgs e)
        {
            string spec = txtpasteDataSpecification.Text;
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(spec));
            StreamReader sr = new StreamReader(ms);
            CsvReader csv = new CsvReader(sr, true, true);
            csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
            int fieldCount = csv.FieldCount;
            string[] headers = csv.GetFieldHeaders();
            Dataset working = (Dataset)base.GetSessionValue("WorkingDataSet");
            while (csv.ReadNextRecord())
            {
                Field f = null;
                for (int i = 0; i < fieldCount; i++)
                {
                    string val = csv[i];
                    if (headers[i].ToUpper() == "FIELD NAME")
                    {
                        foreach (Field f2 in working.Header)
                        {
                            if (f2.SourceColumnName == val)
                            {
                                f = f2;
                                break;
                            }
                        }
                    }
                    if (f != null)
                        break;
                }
                if (f != null)
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        string val = csv[i];
                        if (string.IsNullOrEmpty(val))
                            continue;
                        if (headers[i].ToUpper() == "LABEL")
                        {
                            f.Name = val;
                        }
                        else if (headers[i].ToUpper() == "TYPE")
                        {
                            f.DBType = (Field.FieldType)int.Parse(val);
                        }
                        else if (headers[i].ToUpper() == "UNITS")
                        {
                            foreach (Metric m in Metrics)
                            {
                                if (m.Name == val && m.DataType == f.DBType)
                                {
                                    f.FieldMetric = m;
                                    break;
                                }
                            }
                        }
                        else if (headers[i].ToUpper() == "TIERED")
                        {
                            if (val.ToUpper() == "TRUE")
                            {
                                f.IsTiered = true;
                            }
                            else
                            {
                                f.IsTiered = false;
                            }
                        }
                        else if (headers[i].ToUpper() == "FIELD INCLUDED")
                        {
                            if (val.ToUpper() == "FALSE")
                            {
                                for (int j = 0; j < working.Header.Count; j++)
                                {
                                    if (working.Header[j] == f)
                                    {
                                        working.Header.RemoveAt(j);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (headers[i].ToUpper() == "INSTRUMENT")
                        {
                            f.SetMetadataValue("instrument", val);
                        }
                        else if (headers[i].ToUpper() == "OBSERVATION METHODOLOGY")
                        {
                            f.SetMetadataValue("observation_methodology", val);
                        }
                        else if (headers[i].ToUpper() == "ANALYSIS METHODOLOGY")
                        {
                            f.SetMetadataValue("analysis_methodology", val);
                        }
                        else if (headers[i].ToUpper() == "PROCESSING METHODOLOGY")
                        {
                            f.SetMetadataValue("processing_methodology", val);
                        }
                        else if (headers[i].ToUpper() == "CITATIONS")
                        {
                            f.SetMetadataValue("citations", val);
                        }
                        else if (headers[i].ToUpper() == "DESCRIPTION")
                        {
                            f.SetMetadataValue("description", val);
                        }
                    }
                }
            }
            sr.Close();
            ms.Close();
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            this.PopulateFields(conn, working);
            conn.Close();
            SetSessionValue("WorkingDataSet", working);
        }
    }
}