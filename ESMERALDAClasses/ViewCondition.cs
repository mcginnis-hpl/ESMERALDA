namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class ViewCondition : EsmeraldaEntity
    {
        public Conversion CondConversion;
        public string Condition;
        protected View parentView;
        public Field SourceField;
        public string SQLName;
        public ConditionType Type;

        public ViewCondition(Field inField, ConditionType inType, View inParentView)
        {
            this.parentView = inParentView;
            this.Type = inType;
            this.SourceField = inField;
            this.Condition = string.Empty;
            this.CondConversion = null;
            if (inField != null)
            {
                this.SQLName = inField.SQLColumnName;
            }
            else
            {
                this.SQLName = string.Empty;
            }
            base.ID = Guid.NewGuid();
        }

        public string BuildClause()
        {
            string ret = string.Empty;
            if ((this.Condition.IndexOf(" AND ") > 0) || (this.Condition.IndexOf(" OR ") > 0))
            {
                string working = this.Condition;
                int and_index = working.IndexOf(" AND ");
                int or_index = working.IndexOf(" OR ");
                string clause = string.Empty;
                bool is_and = false;
                while ((and_index > 0) || (or_index > 0))
                {
                    if ((and_index > 0) && (or_index > 0))
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
                        ret = ret + this.SourceField.FormattedColumnName + clause + " AND ";
                    }
                    else
                    {
                        clause = working.Substring(0, or_index);
                        working = working.Substring(or_index + 4);
                        ret = ret + this.SourceField.FormattedColumnName + clause + " OR ";
                    }
                    and_index = working.IndexOf(" AND ");
                    or_index = working.IndexOf(" OR ");
                }
                return (ret + working);
            }
            return (this.SourceField.FormattedColumnName + " " + this.Condition);
        }

        public void Save(SqlConnection conn, Guid parentID)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_WriteCondition",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@incondition_id", base.ID));
            query.Parameters.Add(new SqlParameter("@inview_id", parentID));
            query.Parameters.Add(new SqlParameter("@incondition_type", (int) this.Type));
            query.Parameters.Add(new SqlParameter("@incondition_text", this.Condition));
            query.Parameters.Add(new SqlParameter("@infield_id", this.SourceField.ID));
            if (this.CondConversion != null)
            {
                query.Parameters.Add(new SqlParameter("@incondition_conversion", this.CondConversion.ID));
            }
            query.Parameters.Add(new SqlParameter("@inname", this.SQLName));
            query.Parameters.Add(new SqlParameter("@insql_name", this.SQLName));
            query.ExecuteNonQuery();
            base.Save(conn);
        }

        public enum ConditionType
        {
            None,
            Filter,
            SortAscending,
            SortDescending,
            Exclude,
            Formula,
            Conversion
        }
    }
}

