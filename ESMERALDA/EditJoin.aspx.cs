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
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (!base.IsPostBack)
            {
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
                        ESMERALDAClasses.Project proj = new ESMERALDAClasses.Project();
                        proj.ID = projectid;
                        proj.Load(conn);
                        thejoin.ParentProject = proj;
                    }
                }
                base.SetSessionValue("WorkingJoin", thejoin);
                this.PopulateData(conn, thejoin);
            }             
            conn.Close();
        }

        protected void PopulateData(SqlConnection conn, ESMERALDAClasses.Join join)
        {
            txtBriefDescription.Text = join.GetMetadataValue("purpose");
            txtDescription.Text = join.GetMetadataValue("abstract");
            txtViewName.Text = join.GetMetadataValue("title");

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
                else if (join.ParentProject.ID == proj_guid)
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
                else if (join.ParentProject.ID == proj_guid)
                {
                    comboSource2Project.SelectedIndex = comboSource2Project.Items.Count - 1;
                }
            }
            reader.Close();

            Guid proj1_guid = Guid.Empty;
            Guid ds1_guid = Guid.Empty;
            if (join.SourceData == null || join.SourceData.ID == Guid.Empty)
            {
                proj1_guid = join.ParentProject.ID;
            }
            else
            {
                proj1_guid = join.SourceData.ParentProject.ID;
                ds1_guid = join.SourceData.ID;
            }
            Guid proj2_guid = Guid.Empty;
            Guid ds2_guid = Guid.Empty;
            if (join.SourceData == null || join.SourceData.ID == Guid.Empty)
            {
                proj2_guid = join.ParentProject.ID;
            }
            else
            {
                proj2_guid = join.JoinedSourceData.ParentProject.ID;
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
            comboSource1LinkingField.Items.Clear();
            comboSource2LinkingField.Items.Clear();

            li = new ListItem("", "");
            comboSource1LinkingField.Items.Add(li);

            li = new ListItem("", "");
            comboSource2LinkingField.Items.Add(li);

            if (ds1_guid != Guid.Empty)
            {
                Guid field1_guid = Guid.Empty;
                if (join.JoinParameter1 != null)
                    field1_guid = join.JoinParameter1.ID;
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT field_id, field_name FROM v_ESMERALDA_entitydata_fields WHERE query_id='" + ds1_guid.ToString() + "' ORDER BY field_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid field_id = Guid.Empty;
                string field_name = string.Empty;
                while (reader.Read())
                {
                    field_id = new Guid(reader["field_id"].ToString());
                    field_name = reader["field_name"].ToString();
                    comboSource1LinkingField.Items.Add(new ListItem(field_name, field_id.ToString()));
                    if (field1_guid != Guid.Empty && field1_guid == field_id)
                    {
                        comboSource1LinkingField.SelectedIndex = comboSource1LinkingField.Items.Count - 1;
                    }
                }
                reader.Close();
            }

            if (ds2_guid != Guid.Empty)
            {
                Guid field2_guid = Guid.Empty;
                if (join.JoinParameter2 != null)
                    field2_guid = join.JoinParameter2.ID;
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT field_id, field_name FROM v_ESMERALDA_entitydata_fields WHERE query_id='" + ds2_guid.ToString() + "' ORDER BY field_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                reader = query.ExecuteReader();
                Guid field_id = Guid.Empty;
                string field_name = string.Empty;
                while (reader.Read())
                {
                    field_id = new Guid(reader["field_id"].ToString());
                    field_name = reader["field_name"].ToString();
                    comboSource2LinkingField.Items.Add(new ListItem(field_name, field_id.ToString()));
                    if (field2_guid != Guid.Empty && field2_guid == field_id)
                    {
                        comboSource2LinkingField.SelectedIndex = comboSource2LinkingField.Items.Count - 1;
                    }
                }
                reader.Close();
            }
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

        protected void btnSubmitJoin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboSource1Project.SelectedValue) || string.IsNullOrEmpty(comboSource2Project.SelectedValue) ||
                string.IsNullOrEmpty(selectedDS1.Value) || string.IsNullOrEmpty(selectedDS2.Value) ||
                string.IsNullOrEmpty(selectedField1.Value) || string.IsNullOrEmpty(selectedField2.Value))
            {
                return;
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");

            ESMERALDAClasses.Join working = (ESMERALDAClasses.Join)base.GetSessionValue("WorkingJoin");
            Guid ds1_guid = new Guid(selectedDS1.Value);
            Guid field1_guid = new Guid(selectedField1.Value);

            Guid ds2_guid = new Guid(selectedDS2.Value);
            Guid field2_guid = new Guid(selectedField2.Value);

            string source_type = ESMERALDAClasses.Utils.GetEntityType(ds1_guid, conn);
            if (source_type == "VIEW")
            {
                working.SourceData = new ESMERALDAClasses.View();
                working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
            }
            else
            {
                working.SourceData = new ESMERALDAClasses.Dataset();
                working.SourceData.Load(conn, ds1_guid, Conversions, Metrics);
            }

            source_type = ESMERALDAClasses.Utils.GetEntityType(ds2_guid, conn);
            if (source_type == "VIEW")
            {
                working.JoinedSourceData = new ESMERALDAClasses.View();
                working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
            }
            else
            {
                working.JoinedSourceData = new ESMERALDAClasses.Dataset();
                working.JoinedSourceData.Load(conn, ds2_guid, Conversions, Metrics);
            }
            working.AutopopulateConditions();
            working.ViewJoinType = (ESMERALDAClasses.Join.JoinType)int.Parse(comboJoinType.SelectedValue);
            foreach (ESMERALDAClasses.QueryField f in working.SourceData.Header)
            {
                if (f.ID == field1_guid)
                {
                    working.JoinParameter1 = f;
                }
            }
            foreach (ESMERALDAClasses.QueryField f in working.JoinedSourceData.Header)
            {
                if (f.ID == field2_guid)
                {
                    working.JoinParameter2 = f;
                }
            }
            string dbname = working.SourceData.ParentProject.database_name;
            string tablename = working.SourceData.SQLName;
            SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);

            PopulatePreviewData(dataconn, working);
            PopulateData(conn, working);
            SetSessionValue("WorkingJoin", working);
            conn.Close();
            dataconn.Close();
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
                    working.Save(conn);
                }
            }
            string dbname = working.SourceData.ParentProject.database_name;
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