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

namespace ESMERALDA
{
    public partial class EditDataSet : ESMERALDAPage
    {
        protected void btnCreateDataset_Click(object sender, EventArgs e)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Guid myId = (Guid) base.GetSessionValue("SessionID");
            Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
            this.SaveMetadata(working);
            DataTable data = (DataTable) base.GetSessionValue("TemporaryData");
            DataTable newtable = working.BuildDataTable(data, -1);
            SqlConnection working_connection = base.ConnectToDatabase(working.ParentProject.database_name);
            SqlTransaction tran = working_connection.BeginTransaction();
            SqlTableCreator creator = new SqlTableCreator(working_connection, tran);
            working.TableName = Utils.CreateDBName(working.Name);
            creator.DestinationTableName = working.TableName;
            creator.Create(SqlTableCreator.GetSchemaTable(newtable));
            tran.Commit();
            creator.WriteData(newtable);
            working_connection.Close();
            if (working.Owner == null)
            {
                working.Owner = base.CurrentUser;
            }
            working.Save(conn);
            this.PopulateFields(conn, working);
            conn.Close();
        }

        protected void btnDeleteExistingData_Click(object sender, EventArgs e)
        {
            Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
            Guid myId = (Guid) base.GetSessionValue("SessionID");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            base.RemoveSessionValue("TemporaryData");
            this.PopulateFields(conn, working);
            conn.Close();
        }

        protected void btnRefreshField_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Posting back.");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
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
            Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
            this.SaveMetadata(working);
            string data_config = this.tableSpecification.Value;
            char[] row_delim = new char[] { ';' };
            char[] col_delim = new char[] { '|' };
            List<Field> fields = new List<Field>();
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
                    Field f = new Field {
                        SourceColumnName = config_cols[0],
                        Name = config_cols[1],
                        DBType = (Field.FieldType) int.Parse(config_cols[2])
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
                    fields.Add(f);
                }
            }
            foreach (Field f in fields)
            {
                if (!string.IsNullOrEmpty(f.SubfieldName))
                {
                    foreach (Field f2 in fields)
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
            string metadata_string = this.fieldMetadata.Value;
            char[] row_delim2 = new char[] { '~' };
            char[] col_delim2 = new char[] { '|' };
            string[] meta_rows = metadata_string.Split(row_delim2);
            for (i = 0; i < meta_rows.Length; i++)
            {
                string[] tokens = meta_rows[i].Split(col_delim2);
                foreach (Field f in fields)
                {
                    if (f.SourceColumnName == tokens[0])
                    {
                        f.Metadata.observation_methodology = tokens[1];
                        f.Metadata.instrument = tokens[2];
                        f.Metadata.analysis_methodology = tokens[3];
                        f.Metadata.processing_methodology = tokens[4];
                        f.Metadata.citations = tokens[5];
                        f.Metadata.description = tokens[6];
                        break;
                    }
                }
            }
            working.Header = fields.ToArray<Field>();
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
            for (int i = 0; i < working.Header.Length; i++)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell {
                    ID = "header_sourcename" + i.ToString(),
                    Text = working.Header[i].SourceColumnName
                };
                tr.Cells.Add(tc);
                tc = new TableCell();
                TextBox txtName = new TextBox {
                    ID = "header_name" + i.ToString(),
                    Text = working.Header[i].Name
                };
                txtName.Attributes.Add("onchange", "updateHeaderField(1, " + i.ToString() + ", '" + txtName.ID + "')");
                tc.Controls.Add(txtName);
                tr.Cells.Add(tc);
                tc = new TableCell();
                DropDownList dl = new DropDownList {
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
                    if (dl.Items[k].Value == ((int) working.Header[i].DBType).ToString())
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
                            if ((working.Header[i].Subfield != null) && (working.Header[i].Subfield.SourceColumnName == f.SourceColumnName))
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
                CheckBox cb = new CheckBox {
                    Checked = true,
                    ID = "header_include" + i.ToString()
                };
                cb.Attributes.Add("onchange", "updateHeaderField(4, " + i.ToString() + ", '" + cb.ID + "')");
                tc.Controls.Add(cb);
                tr.Cells.Add(tc);
                tc = new TableCell {
                    Text = "<a id='metadata_" + i.ToString() + "' href='javascript:editAddMetadata(\"metadata_" + i.ToString() + "\", \"" + working.Header[i].SourceColumnName + "\")'>Edit Metadata</a>"
                };
                tr.Cells.Add(tc);
                this.tblDataField.Rows.Add(tr);
                string specstring = working.Header[i].SourceColumnName + "|" + working.Header[i].Name + "|" + ((int) working.Header[i].DBType).ToString() + "|";
                if (working.Header[i].FieldMetric != null)
                {
                    specstring = specstring + working.Header[i].FieldMetric.ID.ToString();
                }
                else
                {
                    specstring = specstring + Guid.Empty.ToString();
                }
                specstring = specstring + "|1";
                if (working.Header[i].Subfield == null)
                {
                    specstring = specstring + "|";
                }
                else
                {
                    specstring = specstring + "|" + working.Header[i].Subfield.SourceColumnName;
                }
                if (string.IsNullOrEmpty(specTable))
                {
                    specTable = specstring;
                }
                else
                {
                    specTable = specTable + ";" + specstring;
                }
                string metadata_string = working.Header[i].SourceColumnName + "|" + working.Header[i].Metadata.observation_methodology + "|" + working.Header[i].Metadata.instrument + "|" + working.Header[i].Metadata.analysis_methodology + "|" + working.Header[i].Metadata.processing_methodology + "|" + working.Header[i].Metadata.citations + "|" + working.Header[i].Metadata.description;
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
            Guid myId = (Guid) base.GetSessionValue("SessionID");
            DataTable data = (DataTable) base.GetSessionValue("TemporaryData");
            if (data != null)
            {
                DataTable parsed_data = working.BuildDataTable(data, 0x3e8);
                if (parsed_data.Rows.Count != 0)
                {
                    int i;
                    TableHeaderRow thr = new TableHeaderRow();
                    for (i = 0; i < parsed_data.Columns.Count; i++)
                    {
                        TableHeaderCell thc = new TableHeaderCell {
                            Text = parsed_data.Columns[i].ColumnName
                        };
                    }
                    this.previewTable.Rows.Add(thr);
                    List<string[]> newrows = new List<string[]>();
                    TableRow tr = null;
                    TableCell td = null;
                    for (i = 0; i < parsed_data.Rows.Count; i++)
                    {
                        if (i == 0x3e8)
                        {
                            break;
                        }
                        if ((i % 0x3e8) == 0)
                        {
                            Debug.WriteLine("Previewing: " + i.ToString());
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
                    Dataset newdataset = Dataset.Load(conn, setID, current_metrics);
                    if (newdataset != null)
                    {
                        Guid myId = (Guid) base.GetSessionValue("SessionID");
                        DataTable dt = newdataset.MoveExistingDataToTemp(conn, myId);
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
                if (!string.IsNullOrEmpty(this.newMetrics.Value))
                {
                    conn = base.ConnectToConfigString("RepositoryConnection");
                    this.SaveNewMetrics(this.newMetrics.Value, conn);
                }
                if (this.hiddenCommands.Value == "REFRESH")
                {
                    this.hiddenCommands.Value = string.Empty;
                    Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
                    if (working != null)
                    {
                        if (conn == null)
                        {
                            conn = base.ConnectToConfigString("RepositoryConnection");
                        }
                        this.PopulateFields(conn, working);
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
            if (working.IsDefined)
            {
                this.BuildPreview(conn, working);
            }
            this.SetDivVisibility(working);
            List<string> uploads = (List<string>) base.GetSessionValue("UploadedFiles");
            this.PopulateUploads(uploads);
            if (!string.IsNullOrEmpty(working.TableName))
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
            Guid projectid = Guid.Empty;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Request.Params[i]) && (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID"))
                {
                    projectid = new Guid(base.Request.Params[i]);
                }
            }
            this.txtMetadata_Acquisition.Text = working.AcquisitionDescription;
            this.txtMetadata_Description.Text = working.Description;
            this.txtMetadata_Name.Text = working.Name;
            this.txtMetadata_Processing.Text = working.ProcessingDescription;
            this.txtMetadata_ShortDescription.Text = working.BriefDescription;
            this.txtMetadata_URL.Text = working.URL;
            this.lblMetadata_DatasetID.Text = working.ID.ToString();
            string keywordstring = string.Empty;
            if (working.Keywords.Count > 0)
            {
                keywordstring = working.Keywords[0];
                for (i = 1; i < working.Keywords.Count; i++)
                {
                    keywordstring = keywordstring + ", " + working.Keywords[i];
                }
            }
            this.txtKeywords.Text = keywordstring;
            this.comboProject.Items.Clear();
            ListItem li = new ListItem(string.Empty, string.Empty);
            this.comboProject.Items.Add(li);
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT project_name, project_id FROM project_metadata ORDER BY project_name" }.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("project_name")))
                {
                    this.comboProject.Items.Add(new ListItem(reader["project_name"].ToString(), reader["project_id"].ToString()));
                }
            }
            if ((projectid == Guid.Empty) && (working.ParentProject != null))
            {
                projectid = working.ParentProject.ID;
            }
            if (projectid != Guid.Empty)
            {
                for (i = 0; i < this.comboProject.Items.Count; i++)
                {
                    if (this.comboProject.Items[i].Value == projectid.ToString())
                    {
                        this.comboProject.SelectedIndex = i;
                        break;
                    }
                }
            }
            reader.Close();
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
                        tmpval = metrics[i].Name + "|" + metrics[i].Abbrev + "|" + metrics[i].ID.ToString() + "|" + ((int) metrics[i].DataType).ToString() + "|0";
                        this.newMetrics.Value = this.newMetrics.Value + ";" + tmpval;
                    }
                }
            }
        }

        protected void PopulateUploads(List<string> files)
        {
            this.uploadedFiles.Rows.Clear();
            TableHeaderRow thr = new TableHeaderRow();
            TableHeaderCell thc = new TableHeaderCell {
                Text = "Uploaded Files"
            };
            thr.Cells.Add(thc);
            this.uploadedFiles.Rows.Add(thr);
            foreach (string s in files)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell {
                    Text = s
                };
                tr.Cells.Add(tc);
                this.uploadedFiles.Rows.Add(tr);
            }
        }

        protected void ProcessUpload(object sender, AsyncFileUploadEventArgs e)
        {
            int i;
            string[] header_fields;
            int j;
            List<string> missing_fields;
            List<string> extra_fields;
            string msg;
            DateTime starttime = DateTime.Now;
            DateTime functionstarttime = DateTime.Now;
            Debug.WriteLine("Loading file.");
            List<string> rows = new List<string>();
            Dataset working = (Dataset) base.GetSessionValue("WorkingDataSet");
            if (this.uploadFiles2.PostedFile != null)
            {
                Exception Ex;
                HttpPostedFile userPostedFile = this.uploadFiles2.PostedFile;
                string filename = userPostedFile.FileName;
                string mimetype = userPostedFile.ContentType;
                if (((filename.EndsWith(".csv") || (mimetype == "text/csv")) || ((mimetype == "text/comma-separated-values") || filename.EndsWith(".dat"))) || (mimetype == "ext/tab-separated-values"))
                {
                    try
                    {
                        if (userPostedFile.ContentLength > 0)
                        {
                            string curr = string.Empty;
                            StreamReader sr = new StreamReader(userPostedFile.InputStream);
                            while (!sr.EndOfStream)
                            {
                                curr = sr.ReadLine();
                                rows.Add(curr);
                            }
                            sr.Close();
                        }
                    }
                    catch (Exception exception1)
                    {
                        Ex = exception1;
                        this.metadata.InnerHtml = this.metadata.InnerHtml + "Error: <br>" + Ex.Message;
                    }
                }
                else if (filename.EndsWith(".xls") || (mimetype == "application/vnd.ms-excel"))
                {
                    try
                    {
                        if (userPostedFile.ContentLength > 0)
                        {
                            string tmpfilename = @"c:\tempfiles\" + Guid.NewGuid().ToString() + ".xls";
                            FileStream fs = File.OpenWrite(tmpfilename);
                            byte[] buffer = new byte[userPostedFile.ContentLength];
                            userPostedFile.InputStream.Read(buffer, 0, userPostedFile.ContentLength);
                            fs.Write(buffer, 0, userPostedFile.ContentLength);
                            fs.Flush();
                            fs.Close();
                            OleDbConnection connection = new OleDbConnection(string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", tmpfilename));
                            connection.Open();
                            string myTableName = (string) connection.GetSchema("Tables").Rows[0]["TABLE_NAME"];
                            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + myTableName + "]", connection);
                            DataSet ds = new DataSet();
                            adapter.Fill(ds, "anyNameHere");
                            connection.Close();
                            DataTable data = ds.Tables[0];
                            string data_string = Utils.ToCSV(data);
                            char[] delim = new char[] { '\n' };
                            string[] tokens = data_string.Split(delim);
                            rows.AddRange(tokens);
                        }
                    }
                    catch (Exception exception2)
                    {
                        Ex = exception2;
                        this.metadata.InnerHtml = this.metadata.InnerHtml + "Error: <br>" + Ex.Message;
                    }
                }
            }
            if (rows.Count == 0)
            {
                return;
            }
            TimeSpan debugtime = (TimeSpan) (DateTime.Now - starttime);
            starttime = DateTime.Now;
            Debug.WriteLine("Time to upload: " + ((int) debugtime.TotalMilliseconds).ToString() + "ms");
            int comma_num_fields = 0;
            char delim_char = ',';
            char[] delim_char_array = new char[] { ',' };
            int count = 0;
           foreach(string s in rows)
           {
                count = 1;
                foreach (char c in s)
                {
                    if (c == delim_char)
                    {
                        count++;
                    }
                }
                if (count > comma_num_fields)
                {
                    comma_num_fields = count;
                }
            }
            delim_char = '\t';
            delim_char_array[0] = '\t';
            int tab_num_fields = 0;
            foreach(string s in rows)
            {
                count = 1;
                foreach (char c in s)
                {
                    if (c == delim_char)
                    {
                        count++;
                    }
                }
                if (count > tab_num_fields)
                {
                    tab_num_fields = count;
                }
            }
            int num_fields = 0;
            if (tab_num_fields > comma_num_fields)
            {
                num_fields = tab_num_fields;
                delim_char = '\t';
                delim_char_array[0] = '\t';
            }
            else
            {
                num_fields = comma_num_fields;
                delim_char = ',';
                delim_char_array[0] = ',';
            }
            Guid projectid = Guid.Empty;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Request.Params[i]) && (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID"))
                {
                    projectid = new Guid(base.Request.Params[i]);
                }
            }
            if (working == null)
            {
                working = new Dataset();
            }
            List<Field> fields = new List<Field>();
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            Dictionary<string, int> colmap = new Dictionary<string, int>();
            bool error = false;
            missing_fields = new List<string>();
            extra_fields = new List<string>();

            if ((working.Header != null) && (working.Header.Length > 0))
            {                
                foreach (Field f in working.Header)
                {
                    missing_fields.Add(f.SourceColumnName);
                }
                for (i = 0; i < rows.Count; i++)
                {
                    string s = rows[i];
                    count = 1;
                    foreach (char c in s)
                    {
                        if (c == delim_char)
                        {
                            count++;
                        }
                    }
                    if (count == num_fields)
                    {
                        rows.RemoveRange(0, i + 1);
                        header_fields = s.Split(delim_char_array);
                        for (j = 0; j < header_fields.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(header_fields[j].Trim()))
                            {
                                string curr_head_col = header_fields[j].Trim();
                                if (!colmap.ContainsKey(curr_head_col))
                                {
                                    colmap.Add(curr_head_col, j);
                                    if (!missing_fields.Contains(curr_head_col))
                                    {
                                        extra_fields.Add(curr_head_col);
                                    }
                                    else
                                    {
                                        missing_fields.Remove(curr_head_col);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                for (i = 0; i < rows.Count; i++)
                {
                    string s = rows[i];
                    count = 1;
                    foreach (char c in s)
                    {
                        if (c == delim_char)
                        {
                            count++;
                        }
                    }
                    if (count == num_fields)
                    {
                        rows.RemoveRange(0, i + 1);
                        header_fields = null;
                        header_fields = s.Split(delim_char_array);
                        for (j = 0; j < header_fields.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(header_fields[j].Trim()))
                            {
                                Field f = new Field {
                                    SourceColumnName = header_fields[j].Trim()
                                };
                                if (!colmap.ContainsKey(f.SourceColumnName))
                                {
                                    colmap.Add(f.SourceColumnName, j);
                                    f.Name = Field.ExtractColumnName(f.SourceColumnName);
                                    f.FieldMetric = Field.RecommendMetric(f.SourceColumnName, base.Metrics);
                                    fields.Add(f);
                                }
                            }
                        }
                        break;
                    }
                }
                working.Header = fields.ToArray<Field>();
            }
            if (missing_fields.Count > 0)
            {
                msg = "The new data is missing the following columns; it can not be added to this dataset: " + missing_fields[0];
                for (i = 1; i < missing_fields.Count; i++)
                {
                    msg = msg + ", " + missing_fields[i];
                }
                base.ShowAlert(msg);
                error = true;
            }
            else if (extra_fields.Count > 0)
            {
                msg = "The new data has the following extra columns; these fields will not be added to the dataset: " + extra_fields[0];
                for (i = 1; i < extra_fields.Count; i++)
                {
                    msg = msg + ", " + extra_fields[i];
                }
                base.ShowAlert(msg);
            }
            if (projectid != Guid.Empty)
            {
                working.ParentProject = Project.Load(conn, projectid);
            }
            base.SetSessionValue("WorkingDataSet", working);
            Guid myId = (Guid) base.GetSessionValue("SessionID");
            if (!error)
            {
                DataTable saved_table = (DataTable) base.GetSessionValue("TemporaryData");
                saved_table = working.SaveTemporaryData(conn, rows, colmap, myId, saved_table, delim_char_array);
                base.SetSessionValue("TemporaryData", saved_table);
                debugtime = (TimeSpan) (DateTime.Now - starttime);
                starttime = DateTime.Now;
                Debug.WriteLine("Time to save temporary data: " + ((int) debugtime.TotalMilliseconds).ToString() + "ms");
                List<string> uploads = (List<string>) base.GetSessionValue("UploadedFiles");
                uploads.Add(this.uploadFiles2.PostedFile.FileName);
                base.SetSessionValue("UploadedFiles", uploads);
            }
            Debug.WriteLine("Loading file complete.");
            debugtime = (TimeSpan) (DateTime.Now - functionstarttime);
            Debug.WriteLine("Total upload time: " + ((int) debugtime.TotalMilliseconds).ToString() + "ms");
        }

        protected void SaveMetadata(Dataset working)
        {
            working.AcquisitionDescription = this.txtMetadata_Acquisition.Text;
            working.Description = this.txtMetadata_Description.Text;
            working.Name = this.txtMetadata_Name.Text;
            working.ProcessingDescription = this.txtMetadata_Processing.Text;
            working.BriefDescription = this.txtMetadata_ShortDescription.Text;
            working.URL = this.txtMetadata_URL.Text;
            char[] delim = new char[] { ',' };
            working.Keywords.Clear();
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
                            working.Keywords.Add(s2);
                        }
                    }
                }
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
                    new Metric { Name = tokens[0], Abbrev = tokens[1], ID = new Guid(tokens[2]), DataType = (Field.FieldType) int.Parse(tokens[3]) }.Save(conn);
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
                this.uploadPrompt.InnerHtml = "<h4>Upload a file</h4><br/>Select a file to upload data from:";
            }
            else
            {
                this.metadata.Style["visibility"] = "";
                this.datafields.Style["visibility"] = "";
                this.preview.Style["visibility"] = "";
                this.uploadPrompt.InnerHtml = "<h4>Upload additional data</h4><br/>Select a file to upload data from:";
            }
        }
    }
}