namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class Field : QueryField
    {
        public int DataIndex = -1;
        public string SourceColumnName
        {
            get
            {
                return m_sourcecolumnname;
            }
            set
            {
                m_sourcecolumnname = value.Replace("\"", "_").Replace("'", "_");
            }
        }
        protected string m_sourcecolumnname = string.Empty;
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
            return ret.Replace("\"", "").Replace("'", "").Replace("_", " ");
        }

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<field>";
                ret = ret + "<field_name>" + this.Name + "</field_name>";
                if (this.FieldMetric != null)
                {
                    ret = ret + this.FieldMetric.GetMetadata(format);
                }
                foreach (string s in Metadata.Keys)
                {
                    List<string> meta = Metadata[s];
                    for (int i = 0; i < meta.Count; i++)
                    {
                        ret += "<" + s + ">" + meta[i] + "</" + s + ">";
                    }
                }
                ret += "</field>";
            }
            else if (format == MetadataFormat.XML)
            {
                ret = this.Name;
                string val = GetMetadataValue("description");
                if (!string.IsNullOrEmpty(val))
                {
                    ret += " - " + val;
                }
                if (FieldMetric != null)
                {
                    ret += " (" + FieldMetric.Name + ")";
                }
            }
            else if (format == MetadataFormat.FGDC)
            {
                ret = "<attr>";
                ret += "<attrlabl>" + this.Name + "</attrlabl>";
                string val = GetMetadataValue("description");
                if (!string.IsNullOrEmpty(val))
                {
                    ret += "<attrdef>" + val + "</attrdef>";
                }
                if (FieldMetric != null && !string.IsNullOrEmpty(FieldMetric.Abbrev))
                {
                    ret += "<attrdomv>";
                    ret += "<udom><attrunit>" + FieldMetric.Abbrev + " (" + FieldMetric.Name + ")</attrunit></udom>";
                    ret += "</attrdomv>";
                }
                ret += "</attr>";
            }
            return ret;
        }

        public static Metric RecommendMetric(string inColName, List<Metric> metrics, DataTable inTable)
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
            if (inTable == null)
                return null;
            string[] types = new string[3];
            for (int i = 0; i < 3; i++)
            {
                Random r = new Random();
                int index = r.Next(inTable.Rows.Count);
                if (inTable.Rows[index][inColName] == DBNull.Value)
                {
                    continue;
                }
                try
                {
                    int test = int.Parse((string)inTable.Rows[index][inColName]);
                    types[i] = "Generic Integer";
                    continue;
                }
                catch (FormatException)
                {
                }
                try
                {
                    float test2 = float.Parse((string)inTable.Rows[index][inColName]);
                    types[i] = "Generic Decimal";
                    continue;
                }
                catch (FormatException)
                {
                }
                try
                {
                    DateTime test3 = DateTime.Parse((string)inTable.Rows[index][inColName]);
                    types[i] = "UTC";
                    continue;
                }
                catch (FormatException)
                {
                }
                types[i] = "Generic Text";
            }
            Metric ret = null;
            if (!string.IsNullOrEmpty(types[0]) && types[0] == types[1] && types[0] == types[2])
            {
                foreach (Metric m in metrics)
                {
                    if (m.Name == types[0])
                    {
                        ret = m;
                        break;
                    }
                }
            }
            else
            {
                foreach (Metric m in metrics)
                {
                    if (m.Name == "Generic Text")
                    {
                        ret = m;
                        break;
                    }
                }
            }
            return ret;
        }

        public void Save(QuerySet parent, SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(this.SQLColumnName))
            {
                int count = 0;
                bool found = true;
                string original_new_name = Utils.CreateDBName(this.Name);
                string new_name = original_new_name;
                while (found)
                {
                    found = false;
                    foreach (Field d in parent.Header)
                    {
                        if (d.SQLColumnName == new_name)
                        {
                            found = true;                            
                            break;
                        }
                    }
                    if (found)
                    {
                        count += 1;
                        new_name = original_new_name + "_" + count.ToString();
                    }
                }
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteField";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@indataset_id", parent.ID));
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
            base.Save(conn);
        }

        public static string GetFieldTypeName(FieldType inType)
        {
            switch (inType)
            {
                case FieldType.DateTime:
                    return "DateTime";
                case FieldType.Decimal:
                    return "Decimal";
                case FieldType.Integer:
                    return "Integer";
                case FieldType.None:
                    return "None";
                case FieldType.Text:
                    return "Text";
                case FieldType.Time:
                    return "Time";
            }
            return string.Empty;
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

