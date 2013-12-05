using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using System.Data.SqlClient;
using System.Data;

namespace ESMERALDA
{
    public partial class EditView : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = null;
            if (!base.IsPostBack)
            {
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                base.RemoveSessionValue("WorkingView");
                Guid viewid = Guid.Empty;
                Guid projectid = Guid.Empty;
                for (int i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                    {
                        viewid = new Guid(base.Request.Params[i]);
                    }
                    if (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID")
                    {
                        projectid = new Guid(base.Request.Params[i]);
                    }
                }
                if (viewid != Guid.Empty)
                {
                    working = new ESMERALDAClasses.View();
                    working.Load(conn, viewid, base.Conversions, base.Metrics);
                    working.AutopopulateConditions();
                }
                else
                {
                    working = new ESMERALDAClasses.View();
                    if (projectid != Guid.Empty)
                    {
                        ESMERALDAClasses.Container proj = new ESMERALDAClasses.Container();
                        proj.ID = projectid;
                        proj.Load(conn);
                        working.ParentEntity = proj;
                    }
                }
                base.SetSessionValue("WorkingView", working);
                this.PopulateData(conn, working);
                conn.Close();
            }
        }

        public class EntityHolder
        {
            public string Name;
            public string SQLName;
            public string SQLType;
            public Guid EntityID;
            public Guid ParentID;
            public EntityHolder Parent;
            public List<EntityHolder> Children;

            public EntityHolder()
            {
                Name = string.Empty;
                SQLName = string.Empty;
                SQLType = string.Empty;
                EntityID = Guid.Empty;
                Parent = null;
                ParentID = Guid.Empty;
                Children = new List<EntityHolder>();
            }
        }

        protected void PopulateTree(SqlConnection conn)
        {
            List<EntityHolder> entities = new List<EntityHolder>();
            string query = "SELECT DISTINCT entity_name, parent_id, container_id, database_name FROM v_ESMERALDA_container_metadata ORDER BY entity_name";
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                EntityHolder e = new EntityHolder();
                e.Name = reader["entity_name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("parent_id")))
                {
                    e.ParentID = new Guid(reader["parent_id"].ToString());
                }
                e.EntityID = new Guid(reader["container_id"].ToString());
                if (!reader.IsDBNull(reader.GetOrdinal("database_name")))
                    e.SQLName = reader["database_name"].ToString();
                e.SQLType = "container";
                entities.Add(e);
            }
            reader.Close();

            query = "SELECT DISTINCT entity_id, container_id, name, sql_name FROM v_ESMERALDA_query_metadata ORDER BY name";
            cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                EntityHolder e = new EntityHolder();
                e.Name = reader["name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("container_id")))
                {
                    e.ParentID = new Guid(reader["container_id"].ToString());
                }
                e.EntityID = new Guid(reader["entity_id"].ToString());
                if (!reader.IsDBNull(reader.GetOrdinal("sql_name")))
                    e.SQLName = reader["sql_name"].ToString();
                e.SQLType = "dataset";
                entities.Add(e);
            }
            reader.Close();

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].ParentID != Guid.Empty)
                {
                    for (int j = 0; j < entities.Count; j++)
                    {
                        if (entities[j].EntityID == entities[i].ParentID)
                        {
                            entities[i].Parent = entities[j];
                            entities[j].Children.Add(entities[i]);
                            break;
                        }
                    }
                }
            }
            treeSystemView.Nodes.Clear();
            foreach (EntityHolder h in entities)
            {
                if (h.ParentID == Guid.Empty)
                {
                    AddTreeNodes(h, null);
                }
            }
            treeSystemView.CollapseAll();
        }

        protected void AddTreeNodes(EntityHolder h, TreeNode parent)
        {
            TreeNode n = new TreeNode();
            string SQLName = string.Empty;
            EntityHolder curr = h;
            while (curr != null)
            {
                if (!string.IsNullOrEmpty(curr.SQLName))
                {
                    if (string.IsNullOrEmpty(SQLName))
                    {
                        SQLName = "[" + curr.SQLName + "]";
                    }
                    else
                    {
                        SQLName = "[" + curr.SQLName + "].dbo." + SQLName;
                        break;
                    }
                }
                curr = curr.Parent;
            }
            if (h.SQLType == "container")
            {
                n.Text = h.Name;
            }
            else
            {
                n.Text = "<a href='javascript:addField(\"" + SQLName + "\")'>" + h.Name + "</a>";
            }
            n.Value = h.EntityID.ToString() + "~" + SQLName;

            if (parent == null)
            {
                treeSystemView.Nodes.Add(n);
            }
            else
            {
                parent.ChildNodes.Add(n);
            }
            if (h.SQLType == "dataset")
            {
                n.PopulateOnDemand = true;
            }
            foreach (EntityHolder child in h.Children)
            {
                AddTreeNodes(child, n);
            }
        }



        protected void PopulateData(SqlConnection conn, ESMERALDAClasses.View working)
        {
            txtBriefDescription.Text = working.GetMetadataValue("purpose");
            txtDescription.Text = working.GetMetadataValue("abstract");
            txtViewName.Text = working.GetMetadataValue("title");
            txtViewSQLName.Text = working.SQLName;
            chkIsPublic.Checked = working.IsPublic;
            txtQueryText.Text = working.SQLQuery;

            PopulateTree(conn);
            BuildDisplayDataTable(conn, working);
            if (IsAuthenticated)
            {
                btnSaveView.Visible = true;
            }
            else
            {
                btnSaveView.Visible = false;
            }
        }

        protected void btnSubmitView_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtQueryText.Text))
            {
                ShowAlert("You must enter some query text.");
                return;
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");

            ESMERALDAClasses.View working = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            working.SQLQuery = txtQueryText.Text;
            string dbname = working.ParentContainer.database_name;
            SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);
            PopulatePreviewData(dataconn, working);
            dataconn.Close();
            PopulateData(conn, working);
            conn.Close();
            SetSessionValue("WorkingView", working);
        }

        protected void BuildDisplayDataTable(SqlConnection conn, ESMERALDAClasses.View working)
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
                
                tc = new TableCell
                {
                    Text = "<a id='metadata_" + i.ToString() + "' href='javascript:editAddMetadata(\"metadata_" + i.ToString() + "\", \"" + ((Field)working.Header[i]).SourceColumnName + "\")'>Edit Metadata</a>"
                };
                tr.Cells.Add(tc);
                this.tblFieldMetadata.Rows.Add(tr);
                string specstring = ((Field)working.Header[i]).SourceColumnName + "|" + working.Header[i].Name + "|" + ((int)working.Header[i].DBType).ToString() + "|";
                if (working.Header[i].FieldMetric != null)
                {
                    specstring = specstring + working.Header[i].FieldMetric.ID.ToString();
                }
                else
                {
                    specstring = specstring + Guid.Empty.ToString();
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

        protected void ParseSpecification(ESMERALDAClasses.View working)
        {
            if (string.IsNullOrEmpty(tableSpecification.Value))
                return;
            int i = 0;
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
                if (config_cols.Length >= 4)
                {
                    Field f = new Field
                    {
                        SourceColumnName = config_cols[0],
                        Name = config_cols[1],
                        DBType = (Field.FieldType)int.Parse(config_cols[2])
                    };
                    Guid metricid = new Guid(config_cols[3]);
                    foreach (Metric m in Metrics)
                    {
                        if (m.ID == metricid)
                        {
                            f.FieldMetric = m;
                            break;
                        }
                    }
                    f.IsTiered = false;
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
            working.Header.Clear();
            working.Header.AddRange(new_fields);
        }

        protected void PopulatePreviewData(SqlConnection conn, ESMERALDAClasses.View working)
        {
            TableHeaderRow thr;
            SqlDataReader reader = null;
            int i;
            TableHeaderCell thc;
            TableRow tr;
            TableCell tc;
            int numrows = 200;
            ParseSpecification(working);
            string cmd = working.GetQuery(numrows);
            this.tblPreviewData.Rows.Clear();
            List<Field> new_fields = new List<Field>();            
            if (!string.IsNullOrEmpty(cmd))
            {
                try
                {
                    reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                    while (reader.Read())
                    {
                        if (tblPreviewData.Rows.Count == 0)
                        {
                            thr = new TableHeaderRow();
                            for (i = 0; i < reader.FieldCount; i++)
                            {
                                Field f = null;
                                string field_name = reader.GetName(i);
                                for (int j = 0; j < working.Header.Count; j++)
                                {
                                    if (((Field)working.Header[j]).SourceColumnName == field_name)
                                    {
                                        f = (Field)working.Header[j];
                                        break;
                                    }
                                }
                                if (f == null)
                                {
                                    f = new Field();
                                    f.SourceColumnName = reader.GetName(i);
                                    f.Name = reader.GetName(i);
                                    f.SQLColumnName = reader.GetName(i);
                                    string db_type = reader.GetDataTypeName(i);
                                    Guid field_guid = Guid.Empty;
                                    if (db_type == "int" || db_type == "smallint" || db_type == "bigint")
                                    {
                                        f.DBType = Field.FieldType.Integer;
                                        field_guid = Metric.GenericInt;
                                    }
                                    else if (db_type.IndexOf("float") >= 0 || db_type == "real" || db_type.IndexOf("decimal") >= 0 || db_type.IndexOf("money") >= 0)
                                    {
                                        f.DBType = Field.FieldType.Decimal;
                                        field_guid = Metric.GenericDecimal;
                                    }
                                    else if (db_type == "date" || db_type == "datetime" || db_type == "time")
                                    {
                                        f.DBType = Field.FieldType.DateTime;
                                        field_guid = Metric.GenericDatetime;
                                    }
                                    else
                                    {
                                        f.DBType = Field.FieldType.Text;
                                        field_guid = Metric.GenericText;
                                    }                                    
                                    if (field_guid != Guid.Empty)
                                    {
                                        foreach (Metric m in Metrics)
                                        {
                                            if (m.ID == field_guid)
                                            {
                                                f.FieldMetric = m;
                                                break;
                                            }
                                        }
                                    }
                                }
                                thc = new TableHeaderCell
                                {
                                    Text = f.Name
                                };
                                new_fields.Add(f);
                                thr.Cells.Add(thc);
                            }
                            this.tblPreviewData.Rows.Add(thr);
                        }
                        tr = new TableRow();
                        for (i = 0; i < reader.FieldCount; i++)
                        {
                            tc = new TableCell();
                            if (reader.IsDBNull(i))
                            {
                                tc.Text = string.Empty;
                            }
                            else
                            {
                                tc.Text = reader[i].ToString();
                            }
                            tr.Cells.Add(tc);
                        }
                        this.tblPreviewData.Rows.Add(tr);
                        if (tblPreviewData.Rows.Count > numrows)
                        {
                            break;
                        }
                    }
                    working.Header.Clear();
                    if(new_fields.Count > 0)
                        working.Header.AddRange(new_fields);
                }
                catch (Exception ex)
                {
                    string msg = "An error occurred.\n" + ex.Message + "\n" + ex.StackTrace;
                    ShowAlert(msg);
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }
        }

        protected void btnSaveView_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (string.IsNullOrEmpty(txtViewName.Text) || string.IsNullOrWhiteSpace(txtBriefDescription.Text))
            {
                ShowAlert("The view must have a name and a brief description.");
            }
            else
            {
                if (working != null)
                {                    
                    working.SetMetadataValue("title", this.txtViewName.Text);
                    working.SetMetadataValue("abstract", this.txtDescription.Text);
                    working.SetMetadataValue("purpose", this.txtBriefDescription.Text);
                    working.SQLName = txtViewSQLName.Text;
                    working.IsPublic = chkIsPublic.Checked;
                    working.SQLQuery = txtQueryText.Text;
                    working.IsVisible = true;
                    working.Save(conn);
                }
            }
            string dbname = working.ParentContainer.database_name;
            SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);
            PopulatePreviewData(dataconn, working);
            PopulateData(conn, working);
            SetSessionValue("WorkingView", working);
            conn.Close();
            dataconn.Close();
        }
    }
}