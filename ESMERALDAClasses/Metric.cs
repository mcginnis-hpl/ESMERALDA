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
        protected TimeZoneInfo myTimeZone = null;
        public string Name = string.Empty;

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

        public string GetMetadata()
        {
            return (((((string.Empty + "<metric>") + "<metric_name>" + this.Name + "</metric_name>") + "<metric_abbreviation>" + this.Abbrev + "</metric_abbreviation>") + "<data_type>" + Utils.MapFieldType(this.DataType) + "</data_type>") + "</metric>");
        }

        public static List<Metric> LoadExistingMetrics(SqlConnection conn)
        {
            List<Metric> ret = new List<Metric>();
            if (conn.State != ConnectionState.Open)
            {
                throw new Exception("SQL Server connection is not currently open.");
            }
            SqlDataReader reader = new SqlCommand { CommandType = CommandType.StoredProcedure, CommandTimeout = 0x1770, Connection = conn, CommandText = "sp_GetAllMetrics" }.ExecuteReader();
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
                EsmeraldaEntity.Load(conn, m);
            }
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60,
                Connection = conn,
                CommandText = "sp_AddMetric"
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

