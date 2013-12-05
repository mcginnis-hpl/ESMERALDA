using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{
    public class Join : View
    {
        public QuerySet JoinedSourceData;
        public List<QueryField> JoinParameter1;
        public List<QueryField> JoinParameter2;
        public enum JoinType
        {
            Inner = 0,
            LeftOuter = 1,
            RightOuter = 2,
            FullOuter = 3
        }
        public JoinType ViewJoinType;

        public Join(QuerySet inSource1, QuerySet inSource2)
            : base(inSource1)
        {
            JoinedSourceData = inSource2;
            JoinParameter1 = new List<QueryField>();
            JoinParameter2 = new List<QueryField>();
            ViewJoinType = JoinType.Inner;
        }

        public Join()
            : base()
        {
            JoinedSourceData = null;
            JoinParameter1 = new List<QueryField>();
            JoinParameter2 = new List<QueryField>();
            ViewJoinType = JoinType.Inner;
        }

        public override void AutopopulateConditions()
        {
            foreach (QueryField f in SourceData.Header)
            {
                if (!f.IsSubfield)
                {
                    ViewCondition cond = new ViewCondition(f, ViewCondition.ConditionType.None, this);
                    Header.Add(cond);
                }
            }
            foreach (QueryField f in JoinedSourceData.Header)
            {
                if (!f.IsSubfield)
                {
                    ViewCondition cond = new ViewCondition(f, ViewCondition.ConditionType.None, this);
                    Header.Add(cond);
                }
            }
        }

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = "<data_view>";
            ret += "<view_name>" + GetMetadataValue("title") + "</view_name>";
            ret += "<brief_description>" + GetMetadataValue("purpose") + "</brief_description>";
            ret += "<description>" + GetMetadataValue("abstract") + "</description>";
            ret += "<created_on>" + Timestamp.ToShortDateString() + "</created_on>";
            for (int i = 0; i < JoinParameter1.Count; i++)
            {
                ret += "<joined_on>" + JoinParameter1[i].GetMetadata(format) + "</joined_on>";
                ret += "<joined_on>" + JoinParameter2[i].GetMetadata(format) + "</joined_on>";
            }
            if (Owner != null)
            {
                ret += "<createdby>" + Owner.GetMetadata(format) + "</createdby>";
            }
            if (SourceData != null)
            {
                if (SourceData.ParentContainer != null)
                {
                    ret += "<parent>" + SourceData.ParentContainer.GetMetadata(format) + "</parent>";
                }
                ret += "<dataset>" + SourceData.GetMetadata(format) + "</dataset>";
            }
            if (JoinedSourceData != null)
            {
                if (JoinedSourceData.ParentContainer != null)
                {
                    ret += "<parent>" + SourceData.ParentContainer.GetMetadata(format) + "</parent>";
                }
                ret += "<dataset>" + JoinedSourceData.GetMetadata(format) + "</dataset>";
            }
            ret += "</data_view>";
            return ret;
        }

        public override string GetQuery(int numrows)
        {
            string query1 = GetQuery(numrows, SourceData);
            string query2 = GetQuery(numrows, JoinedSourceData);
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
                    if (init)
                    {
                        ret += ",";
                    }
                    else
                    {
                        init = true;
                    }                    
                    ret += vc.BuildClause();
                }
            }
            if (!init)
            {
                return string.Empty;
            }
            ret += " FROM [" + SourceData.SQLName + "]";
            if (ViewJoinType == JoinType.FullOuter)
            {
                ret += " FULL OUTER JOIN";
            }
            else if (ViewJoinType == JoinType.Inner)
            {
                ret += " INNER JOIN";
            }
            else if (ViewJoinType == JoinType.LeftOuter)
            {
                ret += " LEFT OUTER JOIN";
            }
            else if (ViewJoinType == JoinType.RightOuter)
            {
                ret += " RIGHT OUTER JOIN";
            }
            ret += " [" + JoinedSourceData.SQLName + "]";
            ret += " ON " + JoinParameter1[0].FormattedColumnName + "=" + JoinParameter2[0].FormattedColumnName;
            for (int i = 1; i < JoinParameter1.Count; i++)
            {
                ret += ", " + JoinParameter1[i].FormattedColumnName + "=" + JoinParameter2[i].FormattedColumnName;
            }
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = null;
            string dbname = SourceData.ParentContainer.database_name;
            string query_string = GetQuery(-1);            
            query = new SqlCommand();
            if (ID == Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteJoin";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inview_id", ID));
            query.Parameters.Add(new SqlParameter("@injoinedsourcedata_id", JoinedSourceData.ID));            
            query.Parameters.Add(new SqlParameter("@injointype", (int)ViewJoinType));
            query.ExecuteNonQuery();
            string cmd = "DELETE FROM join_link_metadata WHERE view_id='" + ID.ToString() + "'";
            for (int i = 0; i < JoinParameter1.Count; i++)
            {
                cmd += "INSERT INTO join_link_metadata(view_id, joinparameter1_id, joinparameter2_id) VALUES ('" + ID.ToString() + "', '" + JoinParameter1[i].ID.ToString() + "', '" + JoinParameter2[i].ID.ToString() + "');";
            }
            query = new SqlCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();
            base.Save(conn);
        }

        public override void Load(SqlConnection conn, Guid viewID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            base.Load(conn, viewID, globalConversions, metrics);

            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadJoin";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", viewID));
            SqlDataReader reader = query.ExecuteReader();            
            Guid joinedsourceid = Guid.Empty;
            int joinedtype = 0;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                {
                    joinedsourceid = new Guid(reader["joinedsourcedata_id"].ToString());
                    joinedtype = int.Parse(reader["jointype"].ToString());
                }
            }
            reader.Close();

            JoinedSourceData = null;
            string source_type = Utils.GetEntityType(joinedsourceid, conn);
            if (source_type == "view")
            {
                JoinedSourceData = new View();
                JoinedSourceData.Load(conn, joinedsourceid, globalConversions, metrics);
            }
            else
            {
                JoinedSourceData = new Dataset();
                JoinedSourceData.Load(conn, joinedsourceid, globalConversions, metrics);
            }

            ViewJoinType = (JoinType)joinedtype;
            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadJoinLinks";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", viewID));
            reader = query.ExecuteReader();            
            Guid field1_id = Guid.Empty;
            Guid field2_id = Guid.Empty;
            while (reader.Read())
            {
                field1_id = new Guid(reader["joinparameter1_id"].ToString());
                field2_id = new Guid(reader["joinparameter2_id"].ToString());
                foreach (QueryField f in SourceData.Header)
                {
                    if (f.ID == field1_id)
                    {
                        JoinParameter1.Add(f);
                        break;
                    }
                    if (f.ID == field2_id)
                    {
                        JoinParameter2.Add(f);
                        break;
                    }
                }
                foreach (QueryField f in JoinedSourceData.Header)
                {
                    if (f.ID == field1_id)
                    {
                        JoinParameter1.Add(f);
                        break;
                    }
                    if (f.ID == field2_id)
                    {
                        JoinParameter2.Add(f);
                        break;
                    }
                }
            }
            reader.Close();
        }
    }
}
