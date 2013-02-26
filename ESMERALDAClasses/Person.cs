namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class PersonRelationship : EsmeraldaEntity
    {
        public string relationship;
        public Person person;
        public Guid temp_personid;

        public PersonRelationship(Person inPerson, string inRelationship)
        {
            relationship = inRelationship;
            person = inPerson;
            temp_personid = Guid.Empty;
        }

        public PersonRelationship()
        {
            relationship = string.Empty;
            person = null;
        }

        public void Save(SqlConnection conn, Guid entityid)
        {
            if (ID == Guid.Empty)
                ID = Guid.NewGuid();
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.Parameters.Add(new SqlParameter("@inentityid", entityid));
            query.Parameters.Add(new SqlParameter("@inpersonid", person.ID));
            query.Parameters.Add(new SqlParameter("@inrelationship", relationship));
            query.Parameters.Add(new SqlParameter("@inpersonrelationshipid", ID));
            query.CommandText = "sp_ESMERALDA_WritePersonRelationship";
            query.Connection = conn;
            query.CommandTimeout = 60;
            query.ExecuteNonQuery();
            base.Save(conn);
        }
    }

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
            SqlCommand query = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_WritePerson",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inpersonid", ID));
            query.ExecuteNonQuery();
            base.Save(conn);
        }
    }
}

