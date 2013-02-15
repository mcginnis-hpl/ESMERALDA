using System;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{    
    public class ViewCondition : QueryField
    {
        public enum ConditionType
        {
            None = 0,
            Filter = 1,
            SortAscending = 2,
            SortDescending = 3,
            Exclude = 4,
            Formula = 5,
            Conversion = 6
        }
        public ConditionType Type;
        public QueryField SourceField;
        public string Condition;
        public Conversion CondConversion;

        public override Metric FieldMetric
        {
            get
            {
                return SourceField.FieldMetric;
            }
            set
            {
                return;
            }
        }

        public ViewCondition(QueryField inField, ConditionType inType, View inParentView)
            : base()
        {
            if(inField != null)
                Name = inField.Name;

            Parent = inParentView;
            Type = inType;
            SourceField = inField;
            Condition = string.Empty;
            CondConversion = null;
            if (inField != null)
            {
                SQLColumnName = inField.SQLColumnName;
            }
            else
            {
                SQLColumnName = string.Empty;
            }
            ID = Guid.NewGuid();
        }

        public string BuildClause()
        {
            string ret = string.Empty;
            if (Condition.IndexOf(" AND ") > 0 || Condition.IndexOf(" OR ") > 0)
            {
                string working = Condition;
                int and_index = working.IndexOf(" AND ");
                int or_index = working.IndexOf(" OR ");
                string clause = string.Empty;
                bool is_and = false;
                while (and_index > 0 || or_index > 0)
                {
                    if (and_index > 0 && or_index > 0)
                    {
                        if (and_index > or_index)
                        {
                            is_and = true;
                        }
                        else
                        {
                            is_and = false;
                        }
                    }
                    else if (and_index > 0)
                    {
                        is_and = true;
                    }
                    else
                    {
                        is_and = false;
                    }
                    if (is_and)
                    {
                        clause = working.Substring(0, and_index);
                        working = working.Substring(and_index + 5);
                        ret += (SourceField.FormattedColumnName + clause) + " AND ";
                    }
                    else
                    {
                        clause = working.Substring(0, or_index);
                        working = working.Substring(or_index + 4);
                        ret += (SourceField.FormattedColumnName + clause) + " OR ";
                    }
                    and_index = working.IndexOf(" AND ");
                    or_index = working.IndexOf(" OR ");
                }
                if (!string.IsNullOrEmpty(working))
                {
                    clause = working;
                    ret += (SourceField.FormattedColumnName + clause);
                }
            }
            else
            {
                ret = SourceField.FormattedColumnName + " " + Condition;
            }
            return ret;
        }

        public void Save(SqlConnection conn, Guid parentID)
        {
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteCondition";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@incondition_id", ID));
            query.Parameters.Add(new SqlParameter("@inview_id", parentID));
            query.Parameters.Add(new SqlParameter("@incondition_type", (int)Type));
            query.Parameters.Add(new SqlParameter("@incondition_text", Condition));
            query.Parameters.Add(new SqlParameter("@infield_id", SourceField.ID));
            if (CondConversion != null)
            {
                query.Parameters.Add(new SqlParameter("@incondition_conversion", CondConversion.ID));
            }
            query.Parameters.Add(new SqlParameter("@inname", SQLColumnName));
            query.Parameters.Add(new SqlParameter("@insql_name", SQLColumnName));
            query.ExecuteNonQuery();
            base.Save(conn);
        }

        public override Field.FieldType DBType
        {
            get
            {
                return SourceField.DBType;
            }
        }

        public string FormattedSourceName
        {
            get
            {
                View parent = (View)this.Parent;
                if (this.SourceField == null)
                    return string.Empty;
                return ("[" + parent.SourceData.ParentProject.database_name + "].[dbo].[" + parent.SourceData.SQLName + "].[" + this.SourceField.SQLColumnName + "]");
            }
        }

        public override string FormattedColumnName
        {
            get
            {                
                return ("[" + this.SQLColumnName + "]");
            }
        }
    }
}

