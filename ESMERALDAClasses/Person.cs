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

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<personrelationship>";
                ret += "<relationship>" + relationship + "</relationship>";
                ret += person.GetMetadata(format);                
                ret += "</personrelationship>";
            }
            else if (format == MetadataFormat.BCODMO)
            {
                ret += person.GetMetadata(format);
                ret += "Role: " + relationship;
            }
            else if (format == MetadataFormat.FGDC)
            {
                ret = "<cntinfo>";
                string val = person.GetMetadataValue("cntorg");
                if (!string.IsNullOrEmpty(val))
                {
                    ret += "<cntorgp><cntorg>" + val + "</cntorg></cntorgp>";
                }

                ret += "<cntaddr>";
                ret += "<addrtype>Mailing</addrtype>";
                if (!string.IsNullOrEmpty(person.GetMetadataValue("address")))
                {
                    ret += "<addr>" + person.GetMetadataValue("address") + "</addr>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("city")))
                {
                    ret += "<city>" + person.GetMetadataValue("city") + "</city>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("state")))
                {
                    ret += "<state>" + person.GetMetadataValue("state") + "</state>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("country")))
                {
                    ret += "<country>" + person.GetMetadataValue("country") + "</country>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("postal")))
                {
                    ret += "<postal>" + person.GetMetadataValue("postal") + "</postal>";
                }
                ret += "</cntaddr>";
                if (!string.IsNullOrEmpty(person.GetMetadataValue("cntvoice")))
                {
                    ret += "<cntvoice>" + person.GetMetadataValue("cntvoice") + "</cntvoice>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("cntfax")))
                {
                    ret += "<cntfax>" + person.GetMetadataValue("cntfax") + "</cntfax>";
                }
                if (!string.IsNullOrEmpty(person.GetMetadataValue("cntemail")))
                {
                    ret += "<cntemail>" + person.GetMetadataValue("cntemail") + "</cntemail>";
                }
                ret += "<cntpos>" + relationship + "</cntpos>";
                ret += "</cntinfo>";   
            }
            return ret;
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
        public bool IsAdministrator;
        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<person>";
                foreach (string s in Metadata.Keys)
                {
                    System.Collections.Generic.List<string> meta = Metadata[s];
                    for (int i = 0; i < meta.Count; i++)
                    {
                        ret += "<" + s + ">" + meta[i] + "</" + s + ">";
                    }
                }
                ret += "</person>";
            }
            else if (format == MetadataFormat.BCODMO)
            {
                string val = GetMetadataValue("firstname");
                if (!string.IsNullOrEmpty(val))
                    val += " ";
                val += GetMetadataValue("lastname");
                if (!string.IsNullOrEmpty(val))
                    ret += val + Environment.NewLine; 
                val = GetMetadataValue("honorific");
                if (!string.IsNullOrEmpty(val))
                    ret += val + Environment.NewLine;
                val = GetMetadataValue("cntorg");
                if (!string.IsNullOrEmpty(val))
                    ret += val + Environment.NewLine;
                val = GetMetadataValue("address");
                if (!string.IsNullOrEmpty(val))
                    ret += val + Environment.NewLine;
                val = GetMetadataValue("city");
                if (!string.IsNullOrEmpty(val))
                    val += ", ";
                val += GetMetadataValue("state");
                if (!string.IsNullOrEmpty(val))
                    val += " ";
                val += GetMetadataValue("postal");
                if (!string.IsNullOrEmpty(val))
                    val += " ";
                val += GetMetadataValue("country");
                if (!string.IsNullOrEmpty(val))
                    ret += val + Environment.NewLine;
                val = GetMetadataValue("cntvoice");
                if(!string.IsNullOrEmpty(val))
                    ret += "Voice: " + val + Environment.NewLine;
                val = GetMetadataValue("cntfax");
                if (!string.IsNullOrEmpty(val))
                    ret += "Fax: " + val + Environment.NewLine;
                val = GetMetadataValue("cntemail");
                if (!string.IsNullOrEmpty(val))
                    ret += "Email: " + val + Environment.NewLine;
            }
            else if (format == MetadataFormat.FGDC)
            {
                ret = "<cntinfo>";
                string val = GetMetadataValue("cntorg");
                if(!string.IsNullOrEmpty(val))
                {
                    ret += "<cntorgp><cntorg>" + val + "</cntorg></cntorgp>";
                }

                ret += "<cntaddr>";
                ret += "<addrtype>Mailing</addrtype>";
                if(!string.IsNullOrEmpty(GetMetadataValue("address")))
                {              
                    ret += "<addr>" + GetMetadataValue("address") + "</addr>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("city")))
                {              
                    ret += "<city>" + GetMetadataValue("city") + "</city>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("state")))
                {              
                    ret += "<state>" + GetMetadataValue("state") + "</state>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("country")))
                {              
                    ret += "<country>" + GetMetadataValue("country") + "</country>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("postal")))
                {              
                    ret += "<postal>" + GetMetadataValue("postal") + "</postal>";
                }
                ret += "</cntaddr>";
                if(!string.IsNullOrEmpty(GetMetadataValue("cntvoice")))
                {
                    ret += "<cntvoice>"+ GetMetadataValue("cntvoice") + "</cntvoice>";
                }
                if (!string.IsNullOrEmpty(GetMetadataValue("cntfax")))
                {
                    ret += "<cntfax>" + GetMetadataValue("cntfax") + "</cntfax>";
                }
                if (!string.IsNullOrEmpty(GetMetadataValue("cntemail")))
                {
                    ret += "<cntemail>" + GetMetadataValue("cntemail") + "</cntemail>";
                }
                ret += "</cntinfo>";
            }
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
                if (!reader.IsDBNull(reader.GetOrdinal("IsAdministrator")))
                {
                    if (reader.GetBoolean(reader.GetOrdinal("IsAdministrator")))
                    {
                        IsAdministrator = true;
                    }
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

