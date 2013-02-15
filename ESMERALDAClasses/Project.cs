namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Project : EsmeraldaEntity
    {
        public string override_database_name = string.Empty;
        public Program parentProgram = null;
        public string GetMetadata()
        {
            string ret = string.Empty;
            return ret;
        }

        public override void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadProject",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@inprojectid", ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid programid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                {                    
                    if (!reader.IsDBNull(reader.GetOrdinal("program_id")))
                    {
                        programid = new Guid(reader["program_id"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("database_name")))
                    {
                        override_database_name = reader["database_name"].ToString();
                    }
                }
            }
            reader.Close();
            if (programid != Guid.Empty)
            {
                parentProgram = new Program();
                parentProgram.Load(conn, programid);
            }
            base.Load(conn);
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteProject";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inproject_id", base.ID));
            if (this.parentProgram != null)
            {
                query.Parameters.Add(new SqlParameter("@inprogram_id", this.parentProgram.ID));
            }
            if (!string.IsNullOrEmpty(this.override_database_name))
            {
                query.Parameters.Add(new SqlParameter("@indatabase_name", this.override_database_name));
            }
            query.ExecuteScalar();
            base.Save(conn);
        }

        public string database_name
        {
            get
            {
                if (string.IsNullOrEmpty(this.override_database_name))
                {
                    return this.parentProgram.database_name;
                }
                return this.override_database_name;
            }
        }
    }
}

