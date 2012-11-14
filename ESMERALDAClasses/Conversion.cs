namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class Conversion : EsmeraldaEntity
    {
        public Metric DestinationMetric = null;
        public string FormulaName = string.Empty;
        public Metric SourceMetric = null;

        public static Conversion Load(SqlConnection conn, Guid inID, List<Metric> inMetrics)
        {
            Conversion ret = new Conversion {
                ID = inID
            };
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadConversion",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@inID", inID));
            SqlDataReader reader = query.ExecuteReader();
            Guid sID = Guid.Empty;
            Guid dID = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("source_metric")))
                {
                    sID = new Guid(reader["source_metric"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("destination_metric")))
                {
                    dID = new Guid(reader["destination_metric"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("formula")))
                {
                    ret.FormulaName = reader["formula"].ToString();
                }
            }
            reader.Close();
            for (int i = 0; i < inMetrics.Count; i++)
            {
                if (inMetrics[i].ID == sID)
                {
                    ret.SourceMetric = inMetrics[i];
                }
                if (inMetrics[i].ID == dID)
                {
                    ret.DestinationMetric = inMetrics[i];
                }
            }
            ret.Load(conn);
            return ret;
        }

        public static List<Conversion> LoadAll(SqlConnection conn, List<Metric> inMetrics)
        {
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandType = CommandType.StoredProcedure, CommandText = "sp_LoadAllConversions", CommandTimeout = 60 }.ExecuteReader();
            Guid sID = Guid.Empty;
            Guid dID = Guid.Empty;
            List<Conversion> ret = new List<Conversion>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("conversion_id")))
                {
                    Conversion conv = new Conversion();
                    sID = Guid.Empty;
                    dID = Guid.Empty;
                    conv.ID = new Guid(reader["conversion_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("source_metric")))
                    {
                        sID = new Guid(reader["source_metric"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("destination_metric")))
                    {
                        dID = new Guid(reader["destination_metric"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("formula")))
                    {
                        conv.FormulaName = reader["formula"].ToString();
                    }
                    for (int i = 0; i < inMetrics.Count; i++)
                    {
                        if (inMetrics[i].ID == sID)
                        {
                            conv.SourceMetric = inMetrics[i];
                        }
                        if (inMetrics[i].ID == dID)
                        {
                            conv.DestinationMetric = inMetrics[i];
                        }
                    }
                    ret.Add(conv);
                }
            }
            reader.Close();
            return ret;
        }
    }
}

