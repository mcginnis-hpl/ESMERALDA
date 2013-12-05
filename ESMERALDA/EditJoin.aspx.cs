using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace ESMERALDA
{
    public partial class EditJoin : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ESMERALDAClasses.Join thejoin = null;            
            if (!base.IsPostBack)
            {
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                base.RemoveSessionValue("WorkingJoin");
                Guid joinid = Guid.Empty;
                Guid projectid = Guid.Empty;
                for (int i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "JOINID")
                    {
                        joinid = new Guid(base.Request.Params[i]);
                    }
                    if (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID")
                    {
                        projectid = new Guid(base.Request.Params[i]);
                    }
                }
                if (joinid != Guid.Empty)
                {
                    thejoin = new ESMERALDAClasses.Join();
                    thejoin.Load(conn, joinid, base.Conversions, base.Metrics);
                    thejoin.AutopopulateConditions();
                }
                else
                {
                    thejoin = new ESMERALDAClasses.Join();
                    if (projectid != Guid.Empty)
                    {
                        ESMERALDAClasses.Container proj = new ESMERALDAClasses.Container();
                        proj.ID = projectid;
                        proj.Load(conn);
                        thejoin.ParentEntity = proj;
                    }
                }
                base.SetSessionValue("WorkingJoin", thejoin);
                this.PopulateData(conn, thejoin);
                conn.Close();
            }
            if (!string.IsNullOrEmpty(removeField.Value))
            {
                RemoveRow(removeField.Value);
            }
        }

        protected void RemoveRow(string inID)
        {
            removeField.Value = string.Empty;
            ESMERALDAClasses.Join working = (ESMERALDAClasses.Join)GetSessionValue("WorkingJoin");
            if (inID == "ALL")
            {
                working.JoinParameter1.Clear();
                working.JoinParameter2.Clear();
            }
            else
            {
                Guid rowid = new Guid(inID);                                
                int i = 0;
                while (i < working.JoinParameter1.Count)
                {
                    if (working.JoinParameter1[i].ID == rowid)
                    {
                        working.JoinParameter1.RemoveAt(i);
                        working.JoinParameter2.RemoveAt(i);
                        break;
                    }
                    i += 1;
                }                
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            PopulateData(conn, working);
            conn.Close();
            SetSessionValue("WorkingJoin", working);
        }

        protected void PopulateFields(SqlConnection conn, ESMERALDAClasses.Join join, Guid ds1_id, Guid ds2_id)
        {
            int i = 0;
            while(i < tblJoinInfo.Rows.Count)
            {
                if (string.IsNullOrEmpty(tblJoinInfo.Rows[i].ID))
                {
                    tblJoinInfo.Rows.RemoveAt(i);
                }
                else
                {
                    i += 1;
                }
            }

            comboSource1LinkingField.Items.Clear();
            comboSource2LinkingField.Items.Clear();

            ListItem li = new ListItem("", "");
            comboSource1LinkingField.Items.Add(li);

            li = new ListItem("", "");
            comboSource2LinkingField.Items.Add(li);
            SqlCommand query = null;
            SqlDataReader reader = null;

            List<ListItem> fields_1 = new List<ListItem>();
            List<ListItem> fields_2 = new List<ListItem>();

            if (ds1_id != Guid.Empty)
            {
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT field_id, field_name FROM v_ESMERALDA_entitydata_fields WHERE query_id='" + ds1_id.ToString() + "' ORDER BY field_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid field_id = Guid.Empty;
                string field_name = string.Empty;
                while (reader.Read())
                {
                    field_id = new Guid(reader["field_id"].ToString());
                    field_name = reader["field_name"].ToString();
                    fields_1.Add(new ListItem(field_name, field_id.ToString()));
                }
                reader.Close();
            }

            if (ds2_id != null)
            {
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT field_id, field_name FROM v_ESMERALDA_entitydata_fields WHERE query_id='" + ds2_id.ToString() + "' ORDER BY field_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid field_id = Guid.Empty;
                string field_name = string.Empty;
                while (reader.Read())
                {
                    field_id = new Guid(reader["field_id"].ToString());
                    field_name = reader["field_name"].ToString();
                    fields_2.Add(new ListItem(field_name, field_id.ToString()));
                }
                reader.Close();
            }
            foreach (ListItem new_li in fields_1)
            {
                comboSource1LinkingField.Items.Add(new ListItem(new_li.Text, new_li.Value));
            }
            foreach (ListItem new_li in fields_2)
            {
                comboSource2LinkingField.Items.Add(new ListItem(new_li.Text, new_li.Value));
            }
            if (join.JoinParameter1.Count > 0)
            {
                for (i = 0; i < join.JoinParameter1.Count; i++)
                {
                    TableRow tr = new TableRow();
                    TableCell td = new TableCell();
                    td.Text = "Source 1 Linking Field";
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = join.JoinParameter1[i].Name;
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = "Source 2 Linking Field";
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = join.JoinParameter2[i].Name;
                    tr.Cells.Add(td);

                    td = new TableCell();
                    td.Text = "<a href='javascript.removeRow(\"" + join.JoinParameter1[i].ID.ToString() + "\")'>Remove</a>";
                    tr.Cells.Add(td);

                    for (int j = 0; j < tblJoinInfo.Rows.Count; j++)
                    {
                        if (tblJoinInfo.Rows[j].ID == "newJoinRow")
                        {
                            tblJoinInfo.Rows.AddAt(j, tr);
                            break;
                        }
                    }
                }
            }
        }

        protected void PopulateData(SqlConnection conn, ESMERALDAClasses.Join join)
        {
            txtBriefDescription.Text = join.GetMetadataValue("purpose");
            txtDescription.Text = join.GetMetadataValue("abstract");
            txtViewName.Text = join.GetMetadataValue("title");
            lblViewSQLName.Text = join.SQLName;
            chkIsPublic.Checked = join.IsPublic;

            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = "SELECT project_id, project_name, program_name FROM v_ESMERALDA_entityinfo_project ORDER BY program_name, project_name";
            query.CommandTimeout = 60;
            query.Connection = conn;
            SqlDataReader reader = query.ExecuteReader();
            Guid proj_guid = Guid.Empty;
            string proj_name = string.Empty;
            string prog_name = string.Empty;
            comboSource1Project.Items.Clear();
            comboSource2Project.Items.Clear();
            while (reader.Read())
            {
                proj_guid = new Guid(reader["project_id"].ToString());
                prog_name = reader["program_name"].ToString();
                proj_name = reader["project_name"].ToString();
                comboSource1Project.Items.Add(new ListItem(prog_name + ": " + proj_name, proj_guid.ToString()));
                if (join.SourceData != null && join.SourceData.ID != Guid.Empty)
                {
                    if (join.SourceData.ID == proj_guid)
                    {
                        comboSource1Project.SelectedIndex = comboSource1Project.Items.Count - 1;
                    }
                }
                else if (join.ParentContainer.ID == proj_guid)
                {
                    comboSource1Project.SelectedIndex = comboSource1Project.Items.Count - 1;
                }
                comboSource2Project.Items.Add(new ListItem(prog_name + ": " + proj_name, proj_guid.ToString()));
                if (join.JoinedSourceData != null && join.JoinedSourceData.ID != Guid.Empty)
                {
                    if (join.JoinedSourceData.ID == proj_guid)
                    {
                        comboSource2Project.SelectedIndex = comboSource2Project.Items.Count - 1;
                    }
                }
                else if (join.ParentContainer.ID == proj_guid)
                {
                    comboSource2Project.SelectedIndex = comboSource2Project.Items.Count - 1;
                }
            }
            reader.Close();

            Guid proj1_guid = Guid.Empty;
            Guid ds1_guid = Guid.Empty;
            if (join.SourceData == null || join.SourceData.ID == Guid.Empty)
            {
                proj1_guid = join.ParentContainer.ID;
            }
            else
            {
                proj1_guid = join.SourceData.ParentContainer.ID;
                ds1_guid = join.SourceData.ID;
            }
            Guid proj2_guid = Guid.Empty;
            Guid ds2_guid = Guid.Empty;
            if (join.SourceData == null || join.SourceData.ID == Guid.Empty)
            {
                proj2_guid = join.ParentContainer.ID;
            }
            else
            {
                proj2_guid = join.JoinedSourceData.ParentContainer.ID;
                ds2_guid = join.JoinedSourceData.ID;
            }
            comboSource1Dataset.Items.Clear();
            comboSource2Dataset.Items.Clear();
            ListItem li = new ListItem("", "");
            comboSource1Dataset.Items.Add(li);

            li = new ListItem("", "");
            comboSource2Dataset.Items.Add(li);

            if (proj1_guid != Guid.Empty)
            {
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT query_id, query_name FROM v_ESMERALDA_entitydata_queries WHERE parent_id='" + proj1_guid.ToString() + "' ORDER BY query_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid query_id = Guid.Empty;
                string query_name = string.Empty;
                while (reader.Read())
                {
                    query_id = new Guid(reader["query_id"].ToString());
                    query_name = reader["query_name"].ToString();
                    comboSource1Dataset.Items.Add(new ListItem(query_name, query_id.ToString()));
                    if (ds1_guid != Guid.Empty && ds1_guid == query_id)
                    {
                        comboSource1Dataset.SelectedIndex = comboSource1Dataset.Items.Count - 1;
                    }
                    if (proj2_guid == proj1_guid)
                    {
                        comboSource2Dataset.Items.Add(new ListItem(query_name, query_id.ToString()));
                        if (ds2_guid != Guid.Empty && ds2_guid == query_id)
                        {
                            comboSource2Dataset.SelectedIndex = comboSource2Dataset.Items.Count - 1;
                        }
                    }
                }
                reader.Close();
            }
            if (proj2_guid != Guid.Empty && proj2_guid != proj1_guid)
            {
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT query_id, query_name FROM v_ESMERALDA_entitydata_queries WHERE parent_id='" + proj2_guid.ToString() + "' ORDER BY query_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid query_id = Guid.Empty;
                string query_name = string.Empty;
                while (reader.Read())
                {
                    query_id = new Guid(reader["query_id"].ToString());
                    query_name = reader["query_name"].ToString();
                    comboSource2Dataset.Items.Add(new ListItem(query_name, query_id.ToString()));
                    if (ds2_guid != Guid.Empty && query_id == ds2_guid)
                    {
                        comboSource2Dataset.SelectedIndex = comboSource2Dataset.Items.Count - 1;
                    }
                }
                reader.Close();
            }
            PopulateFields(conn, join, ds1_guid, ds2_guid);
            for (int i = 0; i < comboJoinType.Items.Count; i++)
            {
                if (comboJoinType.Items[i].Value == ((int)join.ViewJoinType).ToString())
                {
                    comboJoinType.SelectedIndex = i;
                    break;
                }
            }
            if (IsAuthenticated)
            {
                btnSaveJoin.Visible = true;
            }
            else
            {
                btnSaveJoin.Visible = false;
            }
        }
        
        protected void btnAddLink_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.Join working = (ESMERALDAClasses.Join)base.GetSessionValue("WorkingJoin");
            Guid field1_id = new Guid(selectedField1.Value);
            Guid field2_id = new Guid(selectedField2.Value);
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            
            Guid ds1_guid = new Guid(selectedDS1.Value);
            if (working.SourceData == null || working.SourceData.ID != ds1_guid)
            {
                string source_type = ESMERALDAClasses.Utils.GetEntityType(ds1_guid, conn);
                if (source_type == "view")
                {
                    working.SourceData = new ESMERALDAClasses.View();
                    working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
                }
                else
                {
                    working.SourceData = new ESMERALDAClasses.Dataset();
                    working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
                }

            }
            Guid ds2_guid = new Guid(selectedDS2.Value);
            if (working.JoinedSourceData == null || working.JoinedSourceData.ID != ds2_guid)
            {
                string source_type = ESMERALDAClasses.Utils.GetEntityType(ds1_guid, conn);
                if (source_type == "view")
                {
                    working.JoinedSourceData = new ESMERALDAClasses.View();
                    working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
                }
                else
                {
                    working.JoinedSourceData = new ESMERALDAClasses.Dataset();
                    working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
                }
            }
            foreach (ESMERALDAClasses.QueryField f in working.SourceData.Header)
            {
                if (f.ID == field1_id)
                {
                    working.JoinParameter1.Add(f);
                    break;
                }
            }
            foreach (ESMERALDAClasses.QueryField f in working.JoinedSourceData.Header)
            {
                if (f.ID == field2_id)
                {
                    working.JoinParameter2.Add(f);
                    break;
                }
            }
            
            base.SetSessionValue("WorkingJoin", working);
            this.PopulateData(conn, working);
            conn.Close();
            selectedField1.Value = string.Empty;
            selectedField2.Value = string.Empty;
        }

        protected void btnSubmitJoin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboSource1Project.SelectedValue) || string.IsNullOrEmpty(comboSource2Project.SelectedValue) ||
                string.IsNullOrEmpty(selectedDS1.Value) || string.IsNullOrEmpty(selectedDS2.Value))
            {
                return;
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");

            ESMERALDAClasses.Join working = (ESMERALDAClasses.Join)base.GetSessionValue("WorkingJoin");
            Guid ds1_guid = new Guid(selectedDS1.Value);            
            Guid ds2_guid = new Guid(selectedDS2.Value);

            if (working.SourceData == null || working.SourceData.ID != ds2_guid)
            {
                string source_type = ESMERALDAClasses.Utils.GetEntityType(ds1_guid, conn);
                if (source_type == "view")
                {
                    working.SourceData = new ESMERALDAClasses.View();
                    working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
                }
                else
                {
                    working.SourceData = new ESMERALDAClasses.Dataset();
                    working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
                }
            }
            if (working.JoinedSourceData == null || working.JoinedSourceData.ID != ds2_guid)
            {
                string source_type = ESMERALDAClasses.Utils.GetEntityType(ds2_guid, conn);
                if (source_type == "view")
                {
                    working.JoinedSourceData = new ESMERALDAClasses.View();
                    working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
                }
                else
                {
                    working.JoinedSourceData = new ESMERALDAClasses.Dataset();
                    working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
                }
            }
            if (!string.IsNullOrEmpty(selectedField1.Value) && !string.IsNullOrEmpty(selectedField2.Value))
            {
                Guid field1_id = new Guid(selectedField1.Value);
                Guid field2_id = new Guid(selectedField2.Value);
                foreach (ESMERALDAClasses.QueryField f in working.SourceData.Header)
                {
                    if (f.ID == field1_id)
                    {
                        working.JoinParameter1.Add(f);
                        break;
                    }
                }
                foreach (ESMERALDAClasses.QueryField f in working.JoinedSourceData.Header)
                {
                    if (f.ID == field2_id)
                    {
                        working.JoinParameter2.Add(f);
                        break;
                    }
                }
            }
            if (working.JoinParameter1.Count > 0)
            {
                working.AutopopulateConditions();
                if (!string.IsNullOrEmpty(comboJoinType.SelectedValue))
                {
                    working.ViewJoinType = (ESMERALDAClasses.Join.JoinType)int.Parse(comboJoinType.SelectedValue);
                }
                else
                {
                    working.ViewJoinType = ESMERALDAClasses.Join.JoinType.FullOuter;
                }
                string dbname = working.SourceData.ParentContainer.database_name;
                string tablename = working.SourceData.SQLName;
                SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);

                PopulatePreviewData(dataconn, working);                
                dataconn.Close();
            }
            PopulateData(conn, working);
            conn.Close();
            SetSessionValue("WorkingJoin", working);            
        }

        protected void PopulatePreviewData(SqlConnection conn, ESMERALDAClasses.Join working)
        {
            TableHeaderRow thr;
            SqlDataReader reader;
            int i;
            TableHeaderCell thc;
            TableRow tr;
            TableCell tc;
            int numrows = 200;
            string cmd = working.GetQuery(numrows);
            this.tblPreviewData.Rows.Clear();
            thr = new TableHeaderRow();
            i = 0;
            while (i < working.Header.Count)
            {
                thc = new TableHeaderCell
                {
                    Text = working.Header[i].SQLColumnName
                };
                thr.Cells.Add(thc);
                i++;
            }
            this.tblPreviewData.Rows.Add(thr);
            if (!string.IsNullOrEmpty(cmd))
            {
                reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                while (reader.Read())
                {
                    tr = new TableRow();
                    for (i = 0; i < working.Header.Count; i++)
                    {
                        tc = new TableCell();
                        if (!reader.IsDBNull(reader.GetOrdinal(working.Header[i].SQLColumnName)))
                        {
                            tc.Text = working.Header[i].FieldMetric.Format(reader[working.Header[i].SQLColumnName].ToString());                            
                        }
                        tr.Cells.Add(tc);
                    }
                    this.tblPreviewData.Rows.Add(tr);
                }
                reader.Close();
            }
        }

        protected void btnSaveJoin_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.Join working = (ESMERALDAClasses.Join)base.GetSessionValue("WorkingJoin");
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (string.IsNullOrEmpty(txtViewName.Text) || string.IsNullOrWhiteSpace(txtBriefDescription.Text))
            {
                ShowAlert("The join must have a name and a brief description.");
            }
            else
            {
                if (working != null)
                {
                    working.SetMetadataValue("title", this.txtViewName.Text);
                    working.SetMetadataValue("abstract", this.txtDescription.Text);
                    working.SetMetadataValue("purpose", this.txtBriefDescription.Text);
                    working.IsPublic = chkIsPublic.Checked;
                    working.Save(conn);
                }
            }
            string dbname = working.SourceData.ParentContainer.database_name;
            string tablename = working.SourceData.SQLName;
            SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);

            PopulatePreviewData(dataconn, working);
            PopulateData(conn, working);
            SetSessionValue("WorkingJoin", working);
            conn.Close();
            dataconn.Close();
        }
    }
}