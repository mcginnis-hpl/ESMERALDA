namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class Program : EsmeraldaEntity
    {
        public string database_name;

        protected void CreateDatabase(SqlConnection meta_conn)
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

        public string GetMetadata()
        {
            return string.Empty;
        }

        public override void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_LoadProgram",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@inprogramid", ID));
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("program_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("database_name")))
                    {
                        database_name = reader["database_name"].ToString();
                    }
                }
            }
            reader.Close();
            base.Load(conn);
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(this.database_name))
            {
                this.CreateDatabase(conn);
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteProgram";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inprogram_id", base.ID));
            query.Parameters.Add(new SqlParameter("@indatabase_name", this.database_name));
            query.ExecuteScalar();
            base.Save(conn);
        }
    }
}

