using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace ESMERALDAClasses
{
    public class Container : EsmeraldaEntity
    {
        public string override_database_name = string.Empty;

        public bool IsSeparateDatabase
        {
            get
            {
                return !string.IsNullOrEmpty(override_database_name);
            }
        }

        public void CreateDatabase(SqlConnection meta_conn)
        {
            string prefix = Utils.CreateDBName(this.GetMetadataValue("title"));
            this.database_name = prefix;
            int offset = 0;
            while (Utils.DBExists(database_name, meta_conn))
            {
                this.database_name = prefix + "_" + offset.ToString();
                offset += 1;
            }

            string ServerName = string.Empty;
            char[] delim1 = new char[] { ';' };
            char[] delim2 = new char[] { '=' };
            string[] tokens = meta_conn.ConnectionString.Split(delim1);
            for (int i = 0; i < tokens.Length; i++)
            {
                string[] vals = tokens[i].Split(delim2);
                if ((vals.Length >= 2) && (vals[0].ToUpper() == "SERVER"))
                {
                    ServerName = vals[1];
                }
            }
            SqlConnection masterConn = new SqlConnection();
            string file_path = @"E:\MSSQLSERVER\DATA\";
            string datafile_name = this.database_name + "_data";
            string datapath_name = file_path + datafile_name + ".mdf";
            string logfile_name = this.database_name + "_log";
            string logpath_name = file_path + logfile_name + ".ldf";
            masterConn.ConnectionString = "SERVER = " + ServerName + "; DATABASE = master;User ID=sa;Pwd=p@$$w0rd";
            SqlCommand myCommand = new SqlCommand("CREATE DATABASE " + this.database_name + " ON PRIMARY (NAME = " + datafile_name + ", FILENAME = '" + datapath_name + "', SIZE = 2MB, FILEGROWTH = 1%) LOG ON (NAME = " + logfile_name + ", FILENAME = '" + logpath_name + "', SIZE = 1MB, MAXSIZE = 250MB, FILEGROWTH = 1%)", masterConn);
            string permissionCommand = "use " + this.database_name + ";create user SqlServer_Client from login SqlServer_Client;exec sp_addrolemember db_owner, SqlServer_Client;";
            permissionCommand += "create user SqlServer_Reader from login SqlServer_Reader;exec sp_addrolemember db_datareader, SqlServer_Reader;";
            try
            {
                masterConn.Open();
                myCommand.ExecuteNonQuery();
                new SqlCommand(permissionCommand, masterConn).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                masterConn.Close();
            }
        }

        public Container parentContainer
        {
            get
            {
                return (Container)ParentEntity;
            }
        }

        public override string GetMetadata(Dataset.MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<project>";
                foreach (string s in Metadata.Keys)
                {
                    List<string> vals = Metadata[s];
                    foreach (string v in vals)
                    {
                        ret += "<" + s + ">" + v + "</" + s + ">";
                    }
                }
                ret += "<relationships>";
                foreach (PersonRelationship pr in Relationships)
                {
                    ret += pr.GetMetadata(format);
                }
                ret += "</relationships>";
                ret += "</project>";
            }
            else if (format == MetadataFormat.BCODMO)
            {
                ret = "Project Name: " + GetMetadataValue("title") + Environment.NewLine;
                ret += Environment.NewLine;
                ret += "Acronym: " + GetMetadataValue("acronym") + Environment.NewLine;
                if (parentContainer != null)
                {
                    ret += Environment.NewLine;
                    ret += "Program Name: " + parentContainer.GetMetadataValue("title") + Environment.NewLine;
                }
                ret += Environment.NewLine;
                ret += "Project URL: " + GetMetadataValue("url") + Environment.NewLine;
                Person pi = null;
                foreach (PersonRelationship pr in Relationships)
                {
                    if (pr.relationship == "Lead Principal Investigator")
                    {
                        pi = pr.person;
                        break;
                    }
                }
                if (pi == null)
                {
                    foreach (PersonRelationship pr in Relationships)
                    {
                        if (pr.relationship == "Principal Investigator")
                        {
                            pi = pr.person;
                            break;
                        }
                    }
                }
                if (pi != null)
                {
                    ret += "Lead PI name and contact information:" + Environment.NewLine;
                    ret += pi.GetMetadata(format);
                    ret += Environment.NewLine;
                }
                string pis = string.Empty;
                foreach (PersonRelationship pr in Relationships)
                {
                    if (pr.person == pi)
                        continue;
                    if (pr.relationship == "Principal Investigator")
                    {
                        pis += pr.person.GetMetadata(format) + Environment.NewLine;
                    }
                }
                if (!string.IsNullOrEmpty(pis))
                {
                    ret += "Co-PI names(s) and contact information:" + Environment.NewLine + pis + Environment.NewLine;
                }
                ret += Environment.NewLine;
                if (Owner != null)
                {
                    ret += "Contact name and contact information:  " + Environment.NewLine;
                    if (Owner != null)
                    {
                        ret += Owner.GetMetadata(format);
                    }
                    ret += Environment.NewLine;
                }
                ret += "Logo URL: " + GetMetadataValue("logourl");
                string bounds = string.Empty;
                string val = GetMetadataValue("southbc");
                if (!string.IsNullOrEmpty(val))
                {
                    bounds += "South: " + val;
                }
                val = GetMetadataValue("westbc");
                if (!string.IsNullOrEmpty(val))
                {
                    bounds += "West: " + val;
                }
                val = GetMetadataValue("northbc");
                if (!string.IsNullOrEmpty(val))
                {
                    bounds += "North: " + val;
                }
                val = GetMetadataValue("eastbc");
                if (!string.IsNullOrEmpty(val))
                {
                    bounds += "East: " + val;
                }
                if (!string.IsNullOrEmpty(bounds))
                {
                    ret += "Geolocation: " + Environment.NewLine;
                    ret += bounds;
                }
            }
            return ret;
        }

        public override void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadContainer",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@incontainerid", ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid parentid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("container_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("parent_id")))
                    {
                        parentid = new Guid(reader["parent_id"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("database_name")))
                    {
                        override_database_name = reader["database_name"].ToString();
                    }
                }
            }
            reader.Close();
            if (parentid != Guid.Empty)
            {
                ParentEntity = new Container();
                ParentEntity.Load(conn, parentid);
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
            query.CommandText = "sp_ESMERALDA_WriteContainer";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@incontainer_id", base.ID));
            if (this.parentContainer != null)
            {
                query.Parameters.Add(new SqlParameter("@inparent_id", this.parentContainer.ID));
            }
            if (!string.IsNullOrEmpty(this.override_database_name))
            {
                query.Parameters.Add(new SqlParameter("@indatabase_name", this.override_database_name));
            }
            query.ExecuteNonQuery();            
            base.Save(conn);
        }

        public string database_name
        {
            get
            {
                if (string.IsNullOrEmpty(this.override_database_name))
                {
                    return this.parentContainer.database_name;
                }
                return this.override_database_name;
            }
            set
            {
                override_database_name = value;
            }
        }

        public void UpdateBounds(SqlConnection conn)
        {
            string cmd = "SELECT bound, value FROM v_ESMERALDA_BoundsTree WHERE project_id='" + ID.ToString() + "'";
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            SqlDataReader reader = query.ExecuteReader();
            double northbc = double.NaN;
            double southbc = double.NaN;
            double eastbc = double.NaN;
            double westbc = double.NaN;
            string bound = string.Empty;
            while (reader.Read())
            {
                if (reader.IsDBNull(reader.GetOrdinal("value")))
                {
                    continue;
                }
                double val = 0;
                bound = reader["bound"].ToString();
                if (bound == "northbc")
                {
                    val = double.Parse(reader["value"].ToString());
                    if (double.IsNaN(northbc))
                        northbc = val;
                    else
                        northbc = Math.Max(northbc, val);
                }
                if (bound == "southbc")
                {
                    val = double.Parse(reader["value"].ToString());
                    if (double.IsNaN(southbc))
                        southbc = val;
                    else
                        southbc = Math.Min(northbc, val);
                }
                if (bound == "eastbc")
                {
                    val = double.Parse(reader["value"].ToString());
                    if (double.IsNaN(eastbc))
                        eastbc = val;
                    else
                        eastbc = Math.Max(northbc, val);
                }
                if (bound == "westbc")
                {
                    val = double.Parse(reader["value"].ToString());
                    if (double.IsNaN(westbc))
                        westbc = val;
                    else
                        westbc = Math.Min(northbc, val);
                }
            }
            reader.Close();
            bool changed = false;
            if (!double.IsNaN(eastbc))
            {
                changed = true;
                SetMetadataValue("eastbc", eastbc.ToString());
            }
            if (!double.IsNaN(westbc))
            {
                changed = true;
                SetMetadataValue("westbc", westbc.ToString());
            }
            if (!double.IsNaN(southbc))
            {
                changed = true;
                SetMetadataValue("southbc", southbc.ToString());
            }
            if (!double.IsNaN(northbc))
            {
                changed = true;
                SetMetadataValue("northbc", northbc.ToString());
            }
            if (changed)
                Save(conn);
            if (parentContainer != null)
                parentContainer.UpdateBounds(conn);
        }
    }
}
