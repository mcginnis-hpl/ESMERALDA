namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class EsmeraldaEntity
    {
        public Guid ID = Guid.Empty;
        public Person Owner = null;
        public DateTime Timestamp = DateTime.MinValue;
        public bool IsPublic = true;
        protected Dictionary<string, List<string>> Metadata;
        public List<PersonRelationship> Relationships;

        public EsmeraldaEntity()
        {
            Metadata = new Dictionary<string, List<string>>();
            Relationships = new List<PersonRelationship>();
        }

        public string GetMetadataValue(string inKey)
        {
            if (!Metadata.ContainsKey(inKey))
                return string.Empty;
            return Metadata[inKey][0];
        }

        public List<string> GetMetadataValueArray(string inKey)
        {
            if (!Metadata.ContainsKey(inKey))
                return new List<string>();
            return Metadata[inKey];
        }

        public void SetMetadataValue(string inKey, string inValue)
        {
            if (!Metadata.ContainsKey(inKey))
            {
                List<string> val = new List<string>();
                val.Add(inValue);
                Metadata.Add(inKey, val);
            }
            else
            {
                Metadata[inKey][0] = inValue;
            }
        }

        public void ClearMetadataValue(string inKey)
        {
            if (!Metadata.ContainsKey(inKey))
                return;
            Metadata[inKey].Clear();
        }

        public void AddMetadataValue(string inKey, string inValue)
        {
            if (!Metadata.ContainsKey(inKey))
            {
                List<string> val = new List<string>();
                val.Add(inValue);
                Metadata.Add(inKey, val);
            }
            else
            {
                Metadata[inKey].Add(inValue);
            }
        }

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
                CommandText = "sp_ESMERALDA_LoadEsmereldaEntity",
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
            query = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadEntityMetadata",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inentity_id", ID));
            reader = query.ExecuteReader();
            string tag = string.Empty;
            string value = string.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("metadata_tag")))
                {
                    tag = reader["metadata_tag"].ToString();
                    value = string.Empty;
                    if (!reader.IsDBNull(reader.GetOrdinal("metadata_value")))
                    {
                        value = reader["metadata_value"].ToString();
                    }
                    AddMetadataValue(tag, value);
                }
            }
            reader.Close();
            if (ownerid != Guid.Empty && ownerid != ID)
            {
                Owner = new Person();
                Owner.Load(conn, ownerid);
            }
            query = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadPersonRelationships",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inentityid", ID));
            reader = query.ExecuteReader();
            while (reader.Read())
            {
                PersonRelationship p = new PersonRelationship();
                Guid personid = new Guid(reader["personid"].ToString());
                p.temp_personid = personid;
                p.relationship = reader["relationship"].ToString();
                p.ID = new Guid(reader["personrelationshipid"].ToString());
                Relationships.Add(p);
            }
            reader.Close();
            foreach (PersonRelationship pr in Relationships)
            {
                pr.person = new Person();
                pr.person.Load(conn, pr.temp_personid);
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
            query.CommandText = "sp_ESMERALDA_WriteEsmeraldaEntity";
            query.Connection = conn;
            query.CommandTimeout = 60;
            query.ExecuteNonQuery();

            string cmd = "DELETE FROM entity_metadata WHERE entity_id='" + this.ID.ToString() + "';";
            foreach (string tag in Metadata.Keys)
            {
                foreach (string val in Metadata[tag])
                {
                    cmd = cmd + "INSERT INTO entity_metadata (entity_id, metadata_tag, metadata_value) VALUES ('" + this.ID.ToString() + "', '" + tag + "', '" + val +"');";
                }
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
            cmd = "DELETE FROM person_relationship WHERE entityid='" + this.ID.ToString() + "';";
            if (!string.IsNullOrEmpty(cmd))
            {
                query = new SqlCommand();
                query.CommandText = cmd;
                query.Connection = conn;
                query.CommandTimeout = 60;
                query.CommandType = CommandType.Text;
                query.ExecuteNonQuery();
            }

            foreach (PersonRelationship p in Relationships)
            {
                p.Save(conn, this.ID);
            }
        }
    }
}

