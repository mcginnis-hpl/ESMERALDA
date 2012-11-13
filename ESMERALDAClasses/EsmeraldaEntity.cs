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

        public static void Load(SqlConnection conn, EsmeraldaEntity inEntity)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadEsmereldaEntity",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inID", inEntity.ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid ownerid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("EntityID")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("Keyword")))
                    {
                        inEntity.Keywords.Add(reader["Keyword"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("CreatedBy")))
                    {
                        ownerid = new Guid(reader["CreatedBy"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("CreatedOn")))
                    {
                        inEntity.Timestamp = DateTime.Parse(reader["CreatedOn"].ToString());
                    }
                }
            }
            reader.Close();
            if (ownerid != Guid.Empty)
            {
                inEntity.Owner = Person.Load(conn, ownerid);
            }
        }

        public virtual void Save(SqlConnection conn)
        {
            if (this.Timestamp == DateTime.MinValue)
            {
                this.Timestamp = DateTime.Now;
            }
            SqlCommand query = new SqlCommand();
            string cmd = ("DELETE FROM entity_keywords WHERE EntityID='" + this.ID.ToString() + "';") + "DELETE FROM entity_data WHERE EntityID='" + this.ID.ToString() + "';";
            foreach (string s in this.Keywords)
            {
                cmd = cmd + "INSERT INTO entity_keywords (EntityID, Keyword) VALUES ('" + this.ID.ToString() + "', '" + s + "');";
            }
            cmd = cmd + "INSERT INTO entity_data (EntityID, CreatedBy, CreatedOn) VALUES ('" + this.ID.ToString() + "'";
            if (this.Owner != null)
            {
                cmd = cmd + ", '" + this.Owner.ID.ToString() + "'";
            }
            else
            {
                cmd = cmd + ", NULL";
            }
            cmd = cmd + ", '" + this.Timestamp.ToString() + "')";
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            query.Connection = conn;
            query.CommandTimeout = 60;
            query.ExecuteNonQuery();
        }
    }
}

