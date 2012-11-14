namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Project : EsmeraldaEntity
    {
        public string acronym = string.Empty;
        public string description = string.Empty;
        public DateTime end_date = DateTime.MinValue;
        public string logo_url = string.Empty;
        public string override_database_name = string.Empty;
        public Program parentProgram = null;
        public string project_name = string.Empty;
        public string project_url = string.Empty;
        public string small_logo_url = string.Empty;
        public DateTime start_date = DateTime.MinValue;

        public string GetMetadata()
        {
            string ret = string.Empty;
            ret = "<project>";
            return (((((((((ret + "<project_name>" + this.project_name + "</project_name>") + "<acronym>" + this.acronym + "</acronym>") + "<project_url>" + this.project_url + "</project_url>") + "<description>" + this.description + "</description>") + "<start_date>" + this.start_date.ToShortDateString() + "</start_date>") + "<end_date>" + this.end_date.ToShortDateString() + "</end_date>") + "<logo_url>" + this.logo_url + "</logo_url>") + "<small_logo_url>" + this.small_logo_url + "</small_logo_url>") + "</project>");
        }

        public override void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadProject",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@inprojectid", ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid programid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("project_name")))
                    {
                        project_name = reader["project_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("acronym")))
                    {
                        acronym = reader["acronym"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("description")))
                    {
                        description = reader["description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("project_url")))
                    {
                        project_url = reader["project_url"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("start_date")))
                    {
                        start_date = DateTime.Parse(reader["start_date"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("end_date")))
                    {
                        end_date = DateTime.Parse(reader["end_date"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("logo_url")))
                    {
                        logo_url = reader["logo_url"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("small_logo_url")))
                    {
                        small_logo_url = reader["small_logo_url"].ToString();
                    }
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
                parentProgram.Load(conn);
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
            query.CommandText = "sp_WriteProject";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inproject_id", base.ID));
            query.Parameters.Add(new SqlParameter("@inproject_name", this.project_name));
            query.Parameters.Add(new SqlParameter("@inacronym", this.acronym));
            query.Parameters.Add(new SqlParameter("@indescription", this.description));
            query.Parameters.Add(new SqlParameter("@inproject_url", this.project_url));
            if (this.start_date > DateTime.MinValue)
            {
                query.Parameters.Add(new SqlParameter("@instart_date", this.start_date));
            }
            if (this.end_date > DateTime.MinValue)
            {
                query.Parameters.Add(new SqlParameter("@inend_date", this.end_date));
            }
            query.Parameters.Add(new SqlParameter("@inlogo_url", this.logo_url));
            query.Parameters.Add(new SqlParameter("@insmall_logo_url", this.small_logo_url));
            query.Parameters.Add(new SqlParameter("@inaffiliated_projects", string.Empty));
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

