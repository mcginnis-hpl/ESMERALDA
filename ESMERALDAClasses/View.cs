using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{
    public class QueryField : EsmeraldaEntity
    {
        public string SQLColumnName;
        public string Name;
        protected Field.FieldType m_DBType;
        protected Metric m_FieldMetric;
        public QuerySet Parent;

        public virtual Metric FieldMetric
        {
            get
            {
                return m_FieldMetric;
            }
            set
            {
                m_FieldMetric = value;
            }
        }
        public virtual string FormattedColumnName
        {
            get
            {
                return ("[" + this.Parent.ParentProject.database_name + "].[dbo].[" + this.Parent.SQLName + "].[" + this.SQLColumnName + "]");
            }
        }
        public virtual Field.FieldType DBType
        {
            get
            {
                return m_DBType;
            }
            set
            {
                m_DBType = value;
            }
        }
        public bool IsSubfield;

        public virtual string GetMetadata()
        {
            return string.Empty;
        }

        public QueryField()
        {
            SQLColumnName = string.Empty;
            IsSubfield = false;
            Name = string.Empty;
            m_FieldMetric = null;
            m_DBType = Field.FieldType.None;
            Parent = null;
        }

    }

    public abstract class QuerySet : EsmeraldaEntity
    {
        public Project ParentProject;
        public List<QueryField> Header;
        public string SQLName;

        public QuerySet()
        {
            Header = new List<QueryField>();
            ParentProject = null;
            SQLName = string.Empty;
        }

        public virtual string GetMetadata()
        {
            return string.Empty;
        }

        public abstract void Load(SqlConnection conn, Guid inID, List<Conversion> globalConversions, List<Metric> metrics);
    }
   
    public class View : QuerySet
    {
        public QuerySet SourceData;
        public string SQLQuery;

        public View(QuerySet inData)
            : base()
        {
            SourceData = inData;
            ID = Guid.NewGuid();
            SQLQuery = string.Empty;
            ParentProject = inData.ParentProject;
        }

        public View()
            : base()
        {
            SourceData = null;
            ID = Guid.NewGuid();
            SQLQuery = string.Empty;
        }

        public override string GetMetadata()
        {
            string ret = "<data_view>";
            ret += "<view_name>" + GetMetadataValue("title") + "</view_name>";
            ret += "<brief_description>" + GetMetadataValue("purpose") + "</brief_description>";
            ret += "<description>" + GetMetadataValue("description") + "</description>";
            ret += "<created_on>" + Timestamp.ToShortDateString() + "</created_on>";
            if (Owner != null)
            {
                ret += "<createdby>" + Owner.GetMetadata() + "</createdby>";
            }
            if (ParentProject != null)
            {
                if (ParentProject.parentProgram != null)
                {
                    ret += "<parent_program>" + ParentProject.parentProgram.GetMetadata() + "</parent_program>";
                }
                ret += "<parent_project>" + ParentProject.GetMetadata() + "</parent_project>";
                ret += "<dataset>" + SourceData.GetMetadata() + "</dataset>";
            }
            ret += "</data_view>";
            return ret;
        }

        public virtual void AutopopulateConditions()
        {
            foreach (QueryField f in SourceData.Header)
            {
                if (!f.IsSubfield)
                {
                    ViewCondition cond = new ViewCondition(f, ViewCondition.ConditionType.None, this);
                    cond.Parent = this;
                    Header.Add(cond);
                }
            }
        }

        public virtual string GetQuery(int numrows, QuerySet source_set)
        {
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(SQLQuery))
                return SQLQuery;

            ret = "SELECT";
            if (numrows > 0)
            {
                ret += " TOP(" + numrows + ")";
            }
            bool init = false;
            for (int i = 0; i < Header.Count; i++)
            {
                ViewCondition vc = (ViewCondition)Header[i];
                if (vc.Type != ViewCondition.ConditionType.Exclude)
                {
                    if(vc.Type == ViewCondition.ConditionType.None)
                    {
                        if (string.IsNullOrEmpty(vc.FormattedSourceName) || string.IsNullOrEmpty(vc.FormattedColumnName))
                        {
                            continue;
                        }
                    }
                    if (init)
                    {
                        ret += ",";
                    }
                    else
                    {
                        init = true;
                    }
                    if (vc.Type == ViewCondition.ConditionType.Formula)
                    {
                        string formula_text = vc.Condition;
                        foreach (QueryField f in source_set.Header)
                        {
                            if (formula_text.IndexOf("[" + f.Name + "]") >= 0)
                            {
                                formula_text = formula_text.Replace("[" + f.Name + "]", f.FormattedColumnName);
                            }
                            else if (formula_text.IndexOf("[" + f.SQLColumnName + "]") >= 0)
                            {
                                formula_text = formula_text.Replace("[" + f.SQLColumnName + "]", f.FormattedColumnName);
                            }
                        }
                        ret += " (" + formula_text + ") AS " + vc.SQLColumnName;
                    }
                    else if (vc.Type == ViewCondition.ConditionType.Conversion && vc.DBType != Field.FieldType.DateTime)
                    {
                        ret += " Repository_Metadata.dbo." + vc.CondConversion.FormulaName + "(" + vc.FormattedSourceName + ") AS " + vc.FormattedColumnName;
                    }
                    else
                    {
                        ret += " " + vc.FormattedSourceName + " AS " + vc.FormattedColumnName;
                    }
                }
            }
            if (!init)
            {
                return string.Empty;
            }
            ret += " FROM [" + source_set.SQLName + "]";
            init = false;
            for (int i = 0; i < Header.Count; i++)
            {
                if (((ViewCondition)Header[i]).Type == ViewCondition.ConditionType.Filter)
                {
                    if (init)
                    {
                        ret += " AND";
                    }
                    else
                    {
                        ret += " WHERE";
                        init = true;
                    }
                    ret += " (" + ((ViewCondition)Header[i]).BuildClause() + ")";
                }
            }
            init = false;
            for (int i = 0; i < Header.Count; i++)
            {
                if (((ViewCondition)Header[i]).Type == ViewCondition.ConditionType.SortAscending)
                {
                    if (init)
                    {
                        ret += ",";
                    }
                    else
                    {
                        ret += " ORDER BY";
                        init = true;
                    }
                    ret += " " + ((ViewCondition)Header[i]).SourceField.FormattedColumnName + " ASC";
                }
                else if (((ViewCondition)Header[i]).Type == ViewCondition.ConditionType.SortDescending)
                {
                    if (init)
                    {
                        ret += ",";
                    }
                    else
                    {
                        ret += " ORDER BY";
                        init = true;
                    }
                    ret += " " + ((ViewCondition)Header[i]).SourceField.FormattedColumnName + " DESC";
                }
            }
            return ret;
        }

        public virtual string GetQuery(int numrows)
        {
            return GetQuery(numrows, SourceData);
        }

        protected void CreateSQLView(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.Text;
            string cmd = "IF OBJECT_ID ('dbo." + SQLName + "', 'V') IS NOT NULL DROP VIEW dbo." + SQLName + " ;";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();

            query = new SqlCommand();
            query.CommandType = CommandType.Text;
            cmd = "CREATE VIEW " + SQLName + " AS " + GetQuery(-1) + ";";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = null;
            string dbname = SourceData.ParentProject.database_name;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);            
            string query_string = GetQuery(-1);
            if (!string.IsNullOrEmpty(query_string))
            {
                SqlConnection test_conn = Utils.ConnectToDatabaseReadOnly(dbname);
                query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = query_string;
                query.CommandTimeout = 60;
                query.Connection = test_conn;
                try
                {
                    SqlDataReader r = query.ExecuteReader();
                    while (r.Read())
                        break;
                    r.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error in view query: " + ex.Message + "; " + ex.StackTrace);
                }
                finally
                {
                    test_conn.Close();
                }
            }
            query = new SqlCommand();
            if (ID == Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteView";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inview_id", ID));
            query.Parameters.Add(new SqlParameter("@indataset_id", SourceData.ID));
            if (string.IsNullOrEmpty(SQLName))
            {
                SQLName = Utils.CreateUniqueTableName(GetMetadataValue("title"), dataconn);
            }
            query.Parameters.Add(new SqlParameter("@inview_sqlname", SQLName));
            query.Parameters.Add(new SqlParameter("@query", SQLQuery));
            if(ParentProject != null)
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentProject.ID));
            query.ExecuteNonQuery();

            foreach (ViewCondition v in Header)
            {
                if (v.Owner == null)
                    v.Owner = Owner;
                v.Save(conn, ID);
            }
            base.Save(conn);                        
            CreateSQLView(dataconn);
            dataconn.Close();
        }

        public override void Load(SqlConnection conn, Guid viewID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadView";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", viewID));
            SqlDataReader reader = query.ExecuteReader();
            Guid enteredbyid = Guid.Empty;
            Guid projectid = Guid.Empty;
            Guid sourceid = Guid.Empty;
            string view_sqlname = string.Empty;
            string view_query = string.Empty;
            DateTime view_createdon = DateTime.MinValue;

            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    sourceid = new Guid(reader["dataset_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("view_sqlname")))
                        view_sqlname = reader["view_sqlname"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("query")))
                        view_query = reader["query"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                        projectid = new Guid(reader["project_id"].ToString());
                }
            }
            reader.Close();

            QuerySet source = null;
            string source_type = Utils.GetEntityType(sourceid, conn);
            if (source_type == "VIEW")
            {
                source = new View();
                source.Load(conn, sourceid, globalConversions, metrics);
            }
            else
            {
                source = new Dataset();
                source.Load(conn, sourceid, globalConversions, metrics);
            }

            ID = viewID;
            SQLName = view_sqlname;
            SQLQuery = view_query;
            if (projectid != Guid.Empty)
            {
                ParentProject = new Project();
                ParentProject.ID = projectid;
                ParentProject.Load(conn);
            }
            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadViewConditions";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inview_id", viewID));
            reader = query.ExecuteReader();
            Guid conversion_id = Guid.Empty;

            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                {
                    QueryField sourceField = null;
                    Guid fieldid = new Guid(reader["field_id"].ToString());
                    int field_type = int.Parse(reader["condition_type"].ToString());
                    foreach (QueryField f in source.Header)
                    {
                        if (f.ID == fieldid)
                        {
                            sourceField = f;
                            break;
                        }
                    }
                    if (sourceField == null)
                        continue;
                    ViewCondition con = new ViewCondition(sourceField, (ViewCondition.ConditionType)field_type, this);
                    con.ID = new Guid(reader["condition_id"].ToString());
                    con.Condition = reader["condition_text"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("condition_conversion")))
                    {
                        conversion_id = new Guid(reader["condition_conversion"].ToString());
                        for (int i = 0; i < globalConversions.Count; i++)
                        {
                            if (globalConversions[i].ID == conversion_id)
                            {
                                con.CondConversion = globalConversions[i];
                            }
                        }
                    }
                    con.SQLColumnName = reader["sql_name"].ToString();
                    con.Load(conn);
                    con.Parent = this;
                    Header.Add(con);
                }
            }
            reader.Close();

            base.Load(conn);
        }
    }
}
