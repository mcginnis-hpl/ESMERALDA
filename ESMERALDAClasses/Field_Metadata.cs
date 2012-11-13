namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Field_Metadata
    {
        public string analysis_methodology = string.Empty;
        public string citations = string.Empty;
        public string description = string.Empty;
        public string instrument = string.Empty;
        public string observation_methodology = string.Empty;
        public string processing_methodology = string.Empty;

        public string GetMetadata()
        {
            return ((((((string.Empty + "<description>" + this.description + "</description>") + "<observation_methodology>" + this.observation_methodology + "</observation_methodology>") + "<instrument>" + this.instrument + "</instrument>") + "<analysis_methodology>" + this.analysis_methodology + "</analysis_methodology>") + "<processing_methodology>" + this.processing_methodology + "</processing_methodology>") + "<citations>" + this.citations + "</citations>");
        }

        public void Save(SqlConnection conn, Guid parentID)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_WriteFieldAdditionalMetadata",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@infield_id", parentID));
            query.Parameters.Add(new SqlParameter("@inobservation_methodology", this.observation_methodology));
            query.Parameters.Add(new SqlParameter("@ininstrument", this.instrument));
            query.Parameters.Add(new SqlParameter("@inanalysis_methodology", this.analysis_methodology));
            query.Parameters.Add(new SqlParameter("@inprocessing_methodology", this.processing_methodology));
            query.Parameters.Add(new SqlParameter("@incitations", this.citations));
            query.Parameters.Add(new SqlParameter("@indescription", this.description));
            query.ExecuteNonQuery();
        }
    }
}

