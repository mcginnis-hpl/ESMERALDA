namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class Metric : EsmeraldaEntity
    {
        public string Abbrev = string.Empty;
        public Field.FieldType DataType = Field.FieldType.None;
        public static Guid JulianDayID = new Guid("01d8b4cb-e853-492e-9f4a-45c331923c0f");
        public static Guid GenericDatetime = new Guid("CB35010B-1B49-40E9-BEE6-F5CC9400D175");
        public static Guid GenericInt = new Guid("BBB6DFD7-AC14-4566-8737-5F4CF9EB0E6B");
        public static Guid GenericDecimal = new Guid("930310CD-C8FC-4738-841B-ED422516ADF0");
        public static Guid GenericText = new Guid("E903E4F4-3139-4179-A03F-559649F633D4");

        protected TimeZoneInfo myTimeZone = null;
        public string Name = string.Empty;
        public static List<ViewCondition> emptyConditions = new List<ViewCondition>();

        public string Format(string value)
        {
            if (this.DataType == Field.FieldType.DateTime)
            {
                return Utils.ConvertDateFromUTC(value, this.CurrentTimeZone).ToString();
            }
            if (this.DataType == Field.FieldType.Time)
            {
                DateTime tmp;
                if (DateTime.TryParse(value, out tmp))
                {
                    return tmp.TimeOfDay.ToString();
                }
                return value;
            }
            return value;
        }

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = (((((string.Empty + "<metric>") + "<metric_name>" + this.Name + "</metric_name>") + "<metric_abbreviation>" + this.Abbrev + "</metric_abbreviation>") + "<data_type>" + Utils.MapFieldType(this.DataType) + "</data_type>") + "</metric>");
            }
            return ret;            
        }

        public static List<Metric> LoadExistingMetrics(SqlConnection conn)
        {
            List<Metric> ret = new List<Metric>();
            if (conn.State != ConnectionState.Open)
            {
                throw new Exception("SQL Server connection is not currently open.");
            }
            SqlDataReader reader = new SqlCommand { CommandType = CommandType.StoredProcedure, CommandTimeout = 0x1770, Connection = conn, CommandText = "sp_ESMERALDA_GetAllMetrics" }.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("metric_id")))
                {
                    Metric new_metric = new Metric();
                    if (!reader.IsDBNull(reader.GetOrdinal("metric_abbrev")))
                    {
                        new_metric.Abbrev = reader["metric_abbrev"].ToString();
                    }
                    new_metric.ID = new Guid(reader["metric_id"].ToString());
                    new_metric.Name = reader["metric_name"].ToString();
                    new_metric.DataType = (Field.FieldType) int.Parse(reader["metric_type"].ToString());
                    ret.Add(new_metric);
                }
            }
            reader.Close();
            foreach (Metric m in ret)
            {
                m.Load(conn);
            }
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60,
                Connection = conn,
                CommandText = "sp_ESMERALDA_AddMetric"
            };
            query.Parameters.Add(new SqlParameter("@inName", this.Name));
            query.Parameters.Add(new SqlParameter("@inAbbrev", this.Abbrev));
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            query.Parameters.Add(new SqlParameter("@inID", base.ID));
            query.Parameters.Add(new SqlParameter("@inType", (int) this.DataType));
            query.ExecuteNonQuery();
            base.Save(conn);
        }

        public TimeZoneInfo CurrentTimeZone
        {
            get
            {
                if (this.myTimeZone != null)
                {
                    return this.myTimeZone;
                }
                if (this.DataType != Field.FieldType.DateTime)
                {
                    return null;
                }
                if (this.Name.IndexOf("Year Day") == 0)
                {
                    return TimeZoneInfo.Utc;
                }
                return TimeZoneInfo.FindSystemTimeZoneById(this.Name);
            }
        }
    }
}

