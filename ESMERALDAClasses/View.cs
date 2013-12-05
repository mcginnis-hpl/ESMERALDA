using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ESMERALDAClasses
{
    public class QueryField : EsmeraldaEntity
    {
        public string SQLColumnName;
        public string Name;
        protected Field.FieldType m_DBType;
        protected Metric m_FieldMetric;
        public QuerySet Parent;
        public bool IsTiered = false;

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
                return ("[" + this.Parent.ParentContainer.database_name + "].[dbo].[" + this.Parent.SQLName + "].[" + this.SQLColumnName + "]");
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
        public Container ParentContainer
        {
            get
            {
                return (Container)ParentEntity;
            }
        }
        public List<QueryField> Header;
        public string SQLName;

        public QuerySet()
        {
            Header = new List<QueryField>();
            SQLName = string.Empty;
        }
        public QueryField GetFieldByName(string inName)
        {
            foreach (QueryField f in Header)
            {
                if (f.Name == inName)
                    return f;
            }
            return null;
        }
        public QueryField GetFieldBySQLName(string inName)
        {
            foreach (QueryField f in Header)
            {
                if (f.SQLColumnName == inName)
                    return f;
            }
            return null;
        }
        public string GetAdditionalMetadataTable()
        {
            string ret = "<table border='1' width='100%'>";
            string[] primary_meta = { "acqdesc", "abstract", "title", "procdesc", "purpose", "url" };
            foreach (string s in Metadata.Keys)
            {
                if (!primary_meta.Contains(s) && Metadata[s] != null)
                {
                    string val = Metadata[s][0];
                    for (int i = 1; i < Metadata[s].Count; i++)
                    {
                        val += ", " + Metadata[s][i];
                    }
                    if (!string.IsNullOrEmpty(val))
                    {
                        ret += "<tr><td>" + s + "</td><td>" + val + "</td></tr>";
                    }
                }
            }
            ret += "</table>";
            return ret;
        }
        public abstract void Load(SqlConnection conn, Guid inID, List<Conversion> globalConversions, List<Metric> metrics);
    }

    public class View : QuerySet
    {
        public QuerySet SourceData;
        public string SQLQuery;
        public bool IsVisible = false;

        public View(QuerySet inData)
            : base()
        {
            SourceData = inData;
            ID = Guid.NewGuid();
            SQLQuery = string.Empty;
            ParentEntity = inData.ParentContainer;
            IsVisible = false;
        }

        public View()
            : base()
        {
            SourceData = null;
            ID = Guid.NewGuid();
            SQLQuery = string.Empty;
            IsVisible = false;
        }

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<data_view>";
                string val = string.Empty;
                val = GetMetadataValue("title");
                if (!string.IsNullOrEmpty(val))
                    ret += "<view_name>" + val + "</view_name>";
                val = GetMetadataValue("purpose");
                if (!string.IsNullOrEmpty(val))
                    ret += "<brief_description>" + val + "</brief_description>";
                val = GetMetadataValue("description");
                if (!string.IsNullOrEmpty(val))
                    ret += "<description>" + val + "</description>";
                if (Timestamp != DateTime.MinValue)
                    ret += "<created_on>" + Timestamp.ToShortDateString() + "</created_on>";
                if (Owner != null)
                {
                    ret += "<createdby>" + Owner.GetMetadata(format) + "</createdby>";
                }
                if (ParentContainer != null)
                {
                    ret += "<parent>" + ParentContainer.GetMetadata(format) + "</parent>";
                }
                if (SourceData != null)
                {
                    ret += "<dataset>" + SourceData.GetMetadata(format) + "</dataset>";
                }
                ret += "</data_view>";
            }
            else if (format == MetadataFormat.BCODMO)
            {
                return SourceData.GetMetadata(format);
            }
            else if (format == MetadataFormat.FGDC)
            {
                return SourceData.GetMetadata(format);
            }
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

        public virtual string GetCountQuery(QuerySet source_set)
        {
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(SQLQuery))
            {
                string suffix = SQLQuery.Substring(SQLQuery.IndexOf(" FROM "));
                ret = "SELECT COUNT(*)" + suffix;
            }
            else
            {
                ret = "SELECT COUNT(*)";
                ret += " FROM [" + source_set.SQLName + "]";
                bool init = false;
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
            }
            return ret;
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
                    if (vc.Type == ViewCondition.ConditionType.None)
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
            string cmd = "IF OBJECT_ID ('dbo.[" + SQLName + "]', 'V') IS NOT NULL DROP VIEW dbo." + SQLName + " ;";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();

            query = new SqlCommand();
            query.CommandType = CommandType.Text;
            cmd = "CREATE VIEW [" + SQLName + "] AS " + GetQuery(-1) + ";";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = null;
            string dbname = ParentContainer.database_name;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            string query_string = GetQuery(-1);
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
            if (SourceData != null)
            {
                query.Parameters.Add(new SqlParameter("@indataset_id", SourceData.ID));
            }
            else
            {
                query.Parameters.Add(new SqlParameter("@indataset_id", Guid.Empty));
            }
            if (string.IsNullOrEmpty(SQLName))
            {
                SQLName = Utils.CreateUniqueTableName(GetMetadataValue("title"), dataconn);
            }
            query.Parameters.Add(new SqlParameter("@inview_sqlname", SQLName));
            query.Parameters.Add(new SqlParameter("@inquery", query_string));
            query.Parameters.Add(new SqlParameter("@inis_visible", IsVisible));
            if (ParentContainer != null)
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentContainer.ID));
            query.ExecuteNonQuery();

            if (Header.Count > 0)
            {
                foreach(QueryField v in Header)
                {
                    if (v.Owner == null)
                        v.Owner = Owner;
                    if (v.GetType() == typeof(ViewCondition))
                    {
                        ((ViewCondition)v).Save(conn, ID);
                    }
                    else
                    {
                        ((Field)v).Save(this, conn);
                    }
                }
            }
            base.Save(conn);
            if(IsVisible)
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
            Guid containerid = Guid.Empty;
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
                        containerid = new Guid(reader["project_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("is_visible")))
                        IsVisible = bool.Parse(reader["is_visible"].ToString());
                }
            }
            reader.Close();

            QuerySet source = null;
            if (sourceid != Guid.Empty)
            {
                string source_type = Utils.GetEntityType(sourceid, conn);
                if (source_type == "view")
                {
                    source = new View();
                    source.Load(conn, sourceid, globalConversions, metrics);
                }
                else
                {
                    source = new Dataset();
                    source.Load(conn, sourceid, globalConversions, metrics);
                }
            }
            ID = viewID;
            SQLName = view_sqlname;
            SQLQuery = view_query;
            if (containerid != Guid.Empty)
            {
                ParentEntity = new Container();
                ParentEntity.Load(conn, containerid);
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
                    con.Parent = this;
                    Header.Add(con);
                }
            }
            reader.Close();
            foreach (ViewCondition con in Header)
            {
                con.Load(conn);
            }
            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadFields";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inDatasetID", viewID));
            reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    Field newfield = new Field();
                    if (!reader.IsDBNull(reader.GetOrdinal("field_name")))
                        newfield.Name = reader["field_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("metric_id")))
                    {
                        Guid mid = new Guid(reader["metric_id"].ToString());
                        foreach (Metric m in metrics)
                        {
                            if (m.ID == mid)
                            {
                                newfield.FieldMetric = m;
                                break;
                            }
                        }
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("source_column_name")))
                        newfield.SourceColumnName = reader["source_column_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("db_type")))
                    {
                        newfield.DBType = (Field.FieldType)int.Parse(reader["db_type"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("field_id")))
                        newfield.ID = new Guid(reader["field_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("sql_column_name")))
                        newfield.SQLColumnName = reader["sql_column_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("subfield_id")))
                        newfield.SubfieldID = new Guid(reader["subfield_id"].ToString());
                    newfield.Parent = this;
                    Header.Add(newfield);
                }
            }
            reader.Close();
            foreach (Field f in Header)
            {
                f.Load(conn);
                if (f.SubfieldID != Guid.Empty)
                {
                    foreach (Field f2 in Header)
                    {
                        if (f2.ID == f.SubfieldID)
                        {
                            f.Subfield = f2;
                            f2.IsSubfield = true;
                            break;
                        }
                    }
                }
            }
            base.Load(conn);
        }

        public DataTable GetDataTable(SqlConnection dataconn)
        {
            DataTable t = new DataTable();
            SqlCommand cmd = new SqlCommand(GetQuery(-1), dataconn);
            SqlDataAdapter a = new SqlDataAdapter(cmd);
            a.Fill(t);
            return t;
        }

        public int GetRowCount(SqlConnection conn)
        {
            int ret = 0;
            string cmd = GetCountQuery(SourceData);
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    ret = int.Parse(reader[0].ToString());
            }
            reader.Close();
            return ret;
        }

        public void WriteDataToStream(string delimiter, StreamWriter outStream, SqlConnection conn)
        {
            int i;
            int numrows = -1;
            string newline = string.Empty;
            bool init = false;
            if (string.IsNullOrEmpty(SQLQuery))
            {
                i = 0;
                while (i < Header.Count)
                {
                    if (((ViewCondition)Header[i]).Type != ViewCondition.ConditionType.Exclude)
                    {
                        if (!string.IsNullOrEmpty(newline))
                        {
                            newline = newline + delimiter;
                        }
                        newline = newline + Utils.FormatValueForCSV(((ViewCondition)Header[i]).SourceField.Name, delimiter);
                        if (((ViewCondition)Header[i]).SourceField.FieldMetric != null)
                        {
                            newline += " (" + ((ViewCondition)Header[i]).SourceField.FieldMetric.Abbrev + ")";
                        }
                    }
                    i++;
                }
                outStream.WriteLine(newline);
            }
            string cmd = GetQuery(numrows);
            try
            {
                if (!string.IsNullOrEmpty(cmd))
                {
                    SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                    
                    if (string.IsNullOrEmpty(SQLQuery))
                    {
                        while (reader.Read())
                        {
                            newline = string.Empty;
                            init = false;
                            for (i = 0; i < Header.Count; i++)
                            {
                                if (((ViewCondition)Header[i]).Type != ViewCondition.ConditionType.Exclude)
                                {
                                    if (init)
                                    {
                                        newline = newline + delimiter;
                                    }
                                    else
                                    {
                                        init = true;
                                    }
                                    if (!reader.IsDBNull(reader.GetOrdinal(Header[i].SQLColumnName)))
                                    {
                                        if (((ViewCondition)Header[i]).CondConversion != null)
                                        {
                                            newline = newline + Utils.FormatValueForCSV(((ViewCondition)Header[i]).CondConversion.DestinationMetric.Format(reader[Header[i].SQLColumnName].ToString()), delimiter);
                                        }
                                        else
                                        {
                                            newline = newline + Utils.FormatValueForCSV(((ViewCondition)Header[i]).SourceField.FieldMetric.Format(reader[Header[i].SQLColumnName].ToString()), delimiter);
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(newline))
                            {
                                outStream.WriteLine(newline);
                                outStream.Flush();
                            }
                        }
                    }
                    else
                    {
                        string header_row = string.Empty;
                        while (reader.Read())
                        {
                            newline = string.Empty;
                            init = false;
                            if (string.IsNullOrEmpty(header_row))
                            {
                                i = 0;
                                while (i < reader.FieldCount)
                                {
                                    if (!string.IsNullOrEmpty(header_row))
                                    {
                                        header_row = header_row + delimiter;
                                    }
                                    header_row = header_row + Utils.FormatValueForCSV(reader.GetName(i), delimiter);
                                    i++;
                                }
                                outStream.WriteLine(header_row);
                            }
                            for (i = 0; i < reader.FieldCount; i++)
                            {
                                if (init)
                                {
                                    newline = newline + delimiter;
                                }
                                else
                                {
                                    init = true;
                                }
                                if (!reader.IsDBNull(i))
                                {
                                    newline = newline + Utils.FormatValueForCSV(reader[i].ToString(), delimiter);
                                }
                            }
                            if (!string.IsNullOrEmpty(newline))
                            {
                                outStream.WriteLine(newline);
                                outStream.Flush();
                            }
                        }
                    }
                    reader.Close();

                }
            }
            catch (TimeoutException ex)
            {

            }
        }
    }
}
