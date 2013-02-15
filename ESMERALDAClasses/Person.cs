namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Person : EsmeraldaEntity
    {
        public string GetMetadata()
        {
            string ret = "<person>";
            return ret;
        }

        public override void Load(SqlConnection conn)
        {            
            base.Load(conn);
        }

        public void LoadByUsername(SqlConnection conn, string inUsername)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadPersonByUsername",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inusername", inUsername));
            SqlDataReader reader = query.ExecuteReader();
            Guid personid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("personid")))
                {
                    personid = new Guid(reader["personid"].ToString());
                }
            }
            reader.Close();
            Load(conn, personid);            
        }

        public override void Save(SqlConnection conn)
        {            
            base.Save(conn);
        }
    }
}

