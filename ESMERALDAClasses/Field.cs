namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class Field : QueryField
    {
        public int DataIndex = -1;        
        public string SourceColumnName = string.Empty;
        public Field Subfield = null;
        public Guid SubfieldID = Guid.Empty;
        public string SubfieldName = string.Empty;

        public static string ExtractColumnName(string inColName)
        {
            string ret = inColName;
            string[] start_brackets = new string[] { "(", "[" };
            string[] end_brackets = new string[] { ")", "]" };
            for (int i = 0; i < start_brackets.Length; i++)
            {
                if ((inColName.IndexOf(start_brackets[i]) > 0) && (inColName.IndexOf(end_brackets[i]) > inColName.IndexOf(start_brackets[i])))
                {
                    ret = inColName.Substring(0, inColName.IndexOf(start_brackets[i]) - 1);
                }
            }
            return ret;
        }

        public override string GetMetadata()
        {
            string ret = string.Empty;
            ret = "<field>";
            ret = ret + "<field_name>" + this.Name + "</field_name>";
            if (this.FieldMetric != null)
            {
                ret = ret + this.FieldMetric.GetMetadata();
            }
            if (this.Metadata != null)
            {
                ret = ret + this.Metadata.GetMetadata();
            }
            return (ret + "</field>");
        }

        public static Metric RecommendMetric(string inColName, List<Metric> metrics)
        {
            foreach (Metric m in metrics)
            {
                if (m.Name == inColName)
                {
                    return m;
                }
            }
            string[] start_brackets = new string[] { "(", "[" };
            string[] end_brackets = new string[] { ")", "]" };
            for (int i = 0; i < start_brackets.Length; i++)
            {
                if ((inColName.IndexOf(start_brackets[i]) > 0) && (inColName.IndexOf(end_brackets[i]) > inColName.IndexOf(start_brackets[i])))
                {
                    int start_dex = inColName.IndexOf(start_brackets[i]) + 1;
                    int len = inColName.IndexOf(end_brackets[i]) - start_dex;
                    string unit = inColName.Substring(start_dex, len);
                    foreach (Metric m in metrics)
                    {
                        if ((m.Name == unit) || (m.Abbrev == unit))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }

        public void Save(Guid datasetID, SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(this.SQLColumnName))
            {
                this.SQLColumnName = Utils.CreateDBName(this.Name);
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteField";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@indataset_id", datasetID));
            query.Parameters.Add(new SqlParameter("@infield_id", base.ID));
            query.Parameters.Add(new SqlParameter("@infield_name", this.Name));
            if (this.FieldMetric != null)
            {
                query.Parameters.Add(new SqlParameter("@inmetric_id", this.FieldMetric.ID));
            }
            query.Parameters.Add(new SqlParameter("@insource_column_name", this.SourceColumnName));
            query.Parameters.Add(new SqlParameter("@indb_type", ((int) this.DBType).ToString()));
            query.Parameters.Add(new SqlParameter("@insql_column_name", this.SQLColumnName));
            if (this.Subfield != null)
            {
                query.Parameters.Add(new SqlParameter("@insubfield_id", this.Subfield.ID));
            }
            query.ExecuteNonQuery();
            if (this.Metadata != null)
            {
                this.Metadata.Save(conn, base.ID);
            }
            base.Save(conn);
        }

        public override string FormattedColumnName
        {
            get
            {
                return ("[" + this.SQLColumnName + "]");
            }
        }

        public enum FieldType
        {
            DateTime = 4,
            Decimal = 2,
            Integer = 1,
            None = 0,
            Text = 3,
            Time = 6
        }
    }
}

