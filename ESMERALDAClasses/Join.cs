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
        public QueryField JoinParameter1;
        public QueryField JoinParameter2;
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
            JoinParameter1 = null;
            JoinParameter2 = null;
            ViewJoinType = JoinType.Inner;
        }

        public Join()
            : base()
        {
            JoinedSourceData = null;
            Description = string.Empty;
            BriefDescription = string.Empty;
            JoinParameter1 = null;
            JoinParameter2 = null;
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

        public override string GetMetadata()
        {
            string ret = "<data_view>";
            ret += "<view_name>" + Name + "</view_name>";
            ret += "<brief_description>" + BriefDescription + "</brief_description>";
            ret += "<description>" + Description + "</description>";
            ret += "<created_on>" + Timestamp.ToShortDateString() + "</created_on>";
            ret += "<joined_on>" + JoinParameter1.GetMetadata() + "</joined_on>";
            ret += "<joined_on>" + JoinParameter2.GetMetadata() + "</joined_on>";
            if (Owner != null)
            {
                ret += "<createdby>" + Owner.GetMetadata() + "</createdby>";
            }
            if (SourceData != null)
            {
                if (SourceData.ParentProject != null)
                {
                    if (SourceData.ParentProject.parentProgram != null)
                    {
                        ret += "<parent_program>" + SourceData.ParentProject.parentProgram.GetMetadata() + "</parent_program>";
                    }
                    ret += "<parent_project>" + SourceData.ParentProject.GetMetadata() + "</parent_project>";
                }
                ret += "<dataset>" + SourceData.GetMetadata() + "</dataset>";
            }
            if (JoinedSourceData != null)
            {
                if (JoinedSourceData.ParentProject != null)
                {
                    if (JoinedSourceData.ParentProject.parentProgram != null)
                    {
                        ret += "<parent_program>" + JoinedSourceData.ParentProject.parentProgram.GetMetadata() + "</parent_program>";
                    }
                    ret += "<parent_project>" + JoinedSourceData.ParentProject.GetMetadata() + "</parent_project>";
                }
                ret += "<dataset>" + JoinedSourceData.GetMetadata() + "</dataset>";
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
            ret += " ON " + JoinParameter1.FormattedColumnName + "=" + JoinParameter2.FormattedColumnName;
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = null;
            string dbname = SourceData.ParentProject.database_name;
            string query_string = GetQuery(-1);            
            query = new SqlCommand();
            if (ID == Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteJoin";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inview_id", ID));
            query.Parameters.Add(new SqlParameter("@injoineddataset_id", JoinedSourceData.ID));
            query.Parameters.Add(new SqlParameter("@injoinparameter1_id", JoinParameter1.ID));
            query.Parameters.Add(new SqlParameter("@injoinparameter2_id", JoinParameter2.ID));
            query.Parameters.Add(new SqlParameter("@injointype", (int)ViewJoinType));
            query.ExecuteNonQuery();           
            base.Save(conn);
        }

        public override void Load(SqlConnection conn, Guid viewID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            base.Load(conn, viewID, globalConversions, metrics);

            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_LoadJoin";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", viewID));
            SqlDataReader reader = query.ExecuteReader();
            Guid joinedfield1 = Guid.Empty;
            Guid joinedfield2 = Guid.Empty;
            Guid joinedsourceid = Guid.Empty;
            int joinedtype = 0;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                {
                    joinedsourceid = new Guid(reader["joinedsourcedata_id"].ToString());
                    joinedfield1 = new Guid(reader["joinparameter1_id"].ToString());
                    joinedfield2 = new Guid(reader["joinparameter2_id"].ToString());
                    joinedtype = int.Parse(reader["jointype"].ToString());
                }
            }
            reader.Close();

            JoinedSourceData = null;
            string source_type = Utils.GetEntityType(joinedsourceid, conn);
            if (source_type == "VIEW")
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
            foreach (QueryField f in SourceData.Header)
            {
                if (f.ID == joinedfield1)
                {
                    JoinParameter1 = f;
                    break;
                }
                if (f.ID == joinedfield2)
                {
                    JoinParameter2 = f;
                    break;
                }
            }
            foreach (QueryField f in JoinedSourceData.Header)
            {
                if (f.ID == joinedfield1)
                {
                    JoinParameter1 = f;
                    break;
                }
                if (f.ID == joinedfield2)
                {
                    JoinParameter2 = f;
                    break;
                }
            }
        }
    }
}
