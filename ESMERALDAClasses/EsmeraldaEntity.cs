namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class EsmeraldaEntity
    {
        public Guid ID = Guid.Empty;
        public List<string> Keywords = new List<string>();
        public Person Owner = null;
        public DateTime Timestamp = DateTime.MinValue;
        public bool IsPublic = true;

        public virtual void Load(SqlConnection conn, Guid inID)
        {
            ID = inID;
            IsPublic = true;
            Load(conn);
        }

        public virtual void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadEsmereldaEntity",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inID", ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid ownerid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("EntityID")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("Keyword")))
                    {
                        Keywords.Add(reader["Keyword"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("CreatedBy")))
                    {
                        ownerid = new Guid(reader["CreatedBy"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("CreatedOn")))
                    {
                        Timestamp = DateTime.Parse(reader["CreatedOn"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("IsPublic")))
                    {
                        IsPublic = bool.Parse(reader["IsPublic"].ToString());
                    }
                }
            }
            reader.Close();
            if (ownerid != Guid.Empty && ownerid != ID)
            {
                Owner = new Person();
                Owner.Load(conn, ownerid);
            }
        }

        public virtual void Save(SqlConnection conn)
        {
            if (this.Timestamp == DateTime.MinValue)
            {
                this.Timestamp = DateTime.Now;
            }
            SqlCommand query = new SqlCommand();            
            query.CommandType = CommandType.StoredProcedure;
            query.Parameters.Add(new SqlParameter("@inEntityID", ID));
            if(this.Owner != null)
                query.Parameters.Add(new SqlParameter("@inCreatedBy", Owner.ID));
            query.Parameters.Add(new SqlParameter("@inCreatedOn", this.Timestamp));
            query.Parameters.Add(new SqlParameter("@inIsPublic", IsPublic));
            query.CommandText = "sp_WriteEsmeraldaEntity";
            query.Connection = conn;
            query.CommandTimeout = 60;
            query.ExecuteNonQuery();

            string cmd = string.Empty;
            foreach (string s in this.Keywords)
            {
                cmd = cmd + "INSERT INTO entity_keywords (EntityID, Keyword) VALUES ('" + this.ID.ToString() + "', '" + s + "');";
            }
            if (!string.IsNullOrEmpty(cmd))
            {
                query = new SqlCommand();
                query.CommandText = cmd;
                query.Connection = conn;
                query.CommandTimeout = 60;
                query.CommandType = CommandType.Text;
                query.ExecuteNonQuery();
            }
        }
    }
}

