namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Person : EsmeraldaEntity
    {
        public string address_line1 = string.Empty;
        public string address_line2 = string.Empty;
        public string affiliation = string.Empty;
        public string city = string.Empty;
        public string comment = string.Empty;
        public string country = string.Empty;
        public string email = string.Empty;
        public string fax = string.Empty;
        public string first_name = string.Empty;
        public string honorific = string.Empty;
        public string last_name = string.Empty;
        public string middle_name = string.Empty;
        public string phone = string.Empty;
        public string state = string.Empty;
        public string zipcode = string.Empty;

        public string GetMetadata()
        {
            string ret = "<person>";
            return ((((((((((((((((ret + "<first_name>" + this.first_name + "</first_name>") + "<middle_name>" + this.middle_name + "</middle_name>") + "<last_name>" + this.last_name + "</last_name>") + "<honorific>" + this.honorific + "</honorific>") + "<affiliation>" + this.affiliation + "</affiliation>") + "<address_line1>" + this.address_line1 + "</address_line1>") + "<address_line2>" + this.address_line2 + "</address_line2>") + "<city>" + this.city + "</city>") + "<state>" + this.state + "</state>") + "<country>" + this.country + "</country>") + "<zipcode>" + this.zipcode + "</zipcode>") + "<phone>" + this.phone + "</phone>") + "<fax>" + this.fax + "</fax>") + "<email>" + this.email + "</email>") + "<comment>" + this.comment + "</comment>") + "</person>");
        }

        public static Person Load(SqlConnection conn, Guid inID)
        {
            Person ret = new Person {
                ID = inID
            };
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadPerson",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inpersonid", inID));
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("personid")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("first_name")))
                    {
                        ret.first_name = reader["first_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("middle_name")))
                    {
                        ret.middle_name = reader["middle_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("last_name")))
                    {
                        ret.last_name = reader["last_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("honorific")))
                    {
                        ret.honorific = reader["honorific"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("affiliation")))
                    {
                        ret.affiliation = reader["affiliation"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("address_line1")))
                    {
                        ret.address_line1 = reader["address_line1"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("address_line2")))
                    {
                        ret.address_line2 = reader["address_line2"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("city")))
                    {
                        ret.city = reader["city"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("state")))
                    {
                        ret.state = reader["state"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("country")))
                    {
                        ret.country = reader["country"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("zipcode")))
                    {
                        ret.zipcode = reader["zipcode"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("phone")))
                    {
                        ret.phone = reader["phone"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("fax")))
                    {
                        ret.fax = reader["fax"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("email")))
                    {
                        ret.email = reader["email"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("comment")))
                    {
                        ret.comment = reader["comment"].ToString();
                    }
                }
            }
            reader.Close();
            EsmeraldaEntity.Load(conn, ret);
            return ret;
        }

        public static Person LoadByUsername(SqlConnection conn, string inUsername)
        {
            Person ret = new Person();
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadPersonByUsername",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inusername", inUsername));
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("personid")))
                {
                    ret.ID = new Guid(reader["personid"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("first_name")))
                    {
                        ret.first_name = reader["first_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("middle_name")))
                    {
                        ret.middle_name = reader["middle_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("last_name")))
                    {
                        ret.last_name = reader["last_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("honorific")))
                    {
                        ret.honorific = reader["honorific"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("affiliation")))
                    {
                        ret.affiliation = reader["affiliation"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("address_line1")))
                    {
                        ret.address_line1 = reader["address_line1"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("address_line2")))
                    {
                        ret.address_line2 = reader["address_line2"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("city")))
                    {
                        ret.city = reader["city"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("state")))
                    {
                        ret.state = reader["state"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("country")))
                    {
                        ret.country = reader["country"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("zipcode")))
                    {
                        ret.zipcode = reader["zipcode"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("phone")))
                    {
                        ret.phone = reader["phone"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("fax")))
                    {
                        ret.fax = reader["fax"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("email")))
                    {
                        ret.email = reader["email"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("comment")))
                    {
                        ret.comment = reader["comment"].ToString();
                    }
                }
            }
            reader.Close();
            EsmeraldaEntity.Load(conn, ret);
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WritePerson";
            query.Connection = conn;
            query.CommandTimeout = 60;
            query.Parameters.Add(new SqlParameter("@inpersonid", base.ID));
            query.Parameters.Add(new SqlParameter("@infirst_name", this.first_name));
            query.Parameters.Add(new SqlParameter("@inmiddle_name", this.middle_name));
            query.Parameters.Add(new SqlParameter("@inlast_name", this.last_name));
            query.Parameters.Add(new SqlParameter("@inhonorific", this.honorific));
            query.Parameters.Add(new SqlParameter("@inaffiliation", this.affiliation));
            query.Parameters.Add(new SqlParameter("@inaddress_line1", this.address_line1));
            query.Parameters.Add(new SqlParameter("@inaddress_line2", this.address_line2));
            query.Parameters.Add(new SqlParameter("@incity", this.city));
            query.Parameters.Add(new SqlParameter("@instate", this.state));
            query.Parameters.Add(new SqlParameter("@incountry", this.country));
            query.Parameters.Add(new SqlParameter("@inzipcode", this.zipcode));
            query.Parameters.Add(new SqlParameter("@inphone", this.phone));
            query.Parameters.Add(new SqlParameter("@infax", this.fax));
            query.Parameters.Add(new SqlParameter("@inemail", this.email));
            query.Parameters.Add(new SqlParameter("@incomment", this.comment));
            query.ExecuteScalar();
            base.Save(conn);
        }
    }
}

