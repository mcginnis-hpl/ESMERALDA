using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using ESMERALDAClasses;

namespace MetadataMigrator
{
    class Program
    {
        static void MigrateDatasetMetadata(SqlConnection conn)
        {
            string cmd = "SELECT * FROM dataset_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<string[]> metadata = new List<string[]>();
            while (reader.Read())
            {
                string[] entry = new string[3];
                entry[0] = reader["dataset_id"].ToString();
                Console.WriteLine("Processing: " + entry[0]);
                entry[1] = "title";
                entry[2] = reader["dataset_name"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["dataset_id"].ToString();
                entry[1] = "abstract";
                entry[2] = reader["dataset_description"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["dataset_id"].ToString();
                entry[1] = "purpose";
                entry[2] = reader["brief_description"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["dataset_id"].ToString();
                entry[1] = "procdesc";
                entry[2] = reader["acquisition_description"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["dataset_id"].ToString();
                entry[1] = "procdesc";
                entry[2] = reader["processing_description"].ToString();
                metadata.Add(entry);

                if (!reader.IsDBNull(reader.GetOrdinal("dataset_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["dataset_id"].ToString();
                    entry[1] = "url";
                    entry[2] = reader["dataset_url"].ToString();
                    metadata.Add(entry);
                }

                if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                {
                    entry = new string[3];
                    entry[0] = reader["dataset_id"].ToString();
                    entry[1] = "westbc";
                    entry[2] = reader["min_lon"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                {
                    entry = new string[3];
                    entry[0] = reader["dataset_id"].ToString();
                    entry[1] = "eastbc";
                    entry[2] = reader["max_lon"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                {
                    entry = new string[3];
                    entry[0] = reader["dataset_id"].ToString();
                    entry[1] = "northbc";
                    entry[2] = reader["min_lat"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                {
                    entry = new string[3];
                    entry[0] = reader["dataset_id"].ToString();
                    entry[1] = "southbc";
                    entry[2] = reader["max_lat"].ToString();
                    metadata.Add(entry);
                }
            }
            reader.Close();
            cmd = string.Empty;
            foreach (string[] meta in metadata)
            {
                cmd += "INSERT INTO entity_metadata(entity_id, metadata_tag, metadata_value) VALUES ('" + meta[0] + "', '" + meta[1] + "','" + meta[2] + "');";
            }
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            query.ExecuteNonQuery();
        }

        static void MigrateProjectMetadata(SqlConnection conn)
        {
            string cmd = "SELECT * FROM project_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<string[]> metadata = new List<string[]>();
            while (reader.Read())
            {
                string[] entry = new string[3];
                entry[0] = reader["project_id"].ToString();
                Console.WriteLine("Processing: " + entry[0]);
                entry[1] = "title";
                entry[2] = reader["project_name"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["project_id"].ToString();
                entry[1] = "description";
                entry[2] = reader["description"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["project_id"].ToString();
                entry[1] = "acronym";
                entry[2] = reader["acronym"].ToString();
                metadata.Add(entry);

                if (!reader.IsDBNull(reader.GetOrdinal("project_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["project_id"].ToString();
                    entry[1] = "url";
                    entry[2] = reader["project_url"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("start_date")))
                {
                    entry = new string[3];
                    entry[0] = reader["project_id"].ToString();
                    entry[1] = "startdate";
                    entry[2] = reader["start_date"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("end_date")))
                {
                    entry = new string[3];
                    entry[0] = reader["project_id"].ToString();
                    entry[1] = "enddate";
                    entry[2] = reader["end_date"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("logo_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["project_id"].ToString();
                    entry[1] = "logourl";
                    entry[2] = reader["logo_url"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("small_logo_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["project_id"].ToString();
                    entry[1] = "small_logo_url";
                    entry[2] = reader["small_logo_url"].ToString();
                    metadata.Add(entry);
                }
            }
            reader.Close();
            cmd = string.Empty;
            foreach (string[] meta in metadata)
            {
                cmd += "INSERT INTO entity_metadata(entity_id, metadata_tag, metadata_value) VALUES ('" + meta[0] + "', '" + meta[1] + "','" + meta[2] + "');";
            }
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            query.ExecuteNonQuery();
        }

        static void MigrateProgramMetadata(SqlConnection conn)
        {
            string cmd = "SELECT * FROM program_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<string[]> metadata = new List<string[]>();
            while (reader.Read())
            {
                string[] entry = new string[3];
                entry[0] = reader["program_id"].ToString();
                Console.WriteLine("Processing: " + entry[0]);
                entry[1] = "title";
                entry[2] = reader["program_name"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["program_id"].ToString();
                entry[1] = "description";
                entry[2] = reader["description"].ToString();
                metadata.Add(entry);

                entry = new string[3];
                entry[0] = reader["program_id"].ToString();
                entry[1] = "acronym";
                entry[2] = reader["acronym"].ToString();
                metadata.Add(entry);

                if (!reader.IsDBNull(reader.GetOrdinal("program_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["program_id"].ToString();
                    entry[1] = "url";
                    entry[2] = reader["program_url"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("start_date")))
                {
                    entry = new string[3];
                    entry[0] = reader["program_id"].ToString();
                    entry[1] = "startdate";
                    entry[2] = reader["start_date"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("end_date")))
                {
                    entry = new string[3];
                    entry[0] = reader["program_id"].ToString();
                    entry[1] = "enddate";
                    entry[2] = reader["end_date"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("logo_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["program_id"].ToString();
                    entry[1] = "logourl";
                    entry[2] = reader["logo_url"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("small_logo_url")))
                {
                    entry = new string[3];
                    entry[0] = reader["program_id"].ToString();
                    entry[1] = "small_logo_url";
                    entry[2] = reader["small_logo_url"].ToString();
                    metadata.Add(entry);
                }
            }
            reader.Close();
            cmd = string.Empty;
            foreach (string[] meta in metadata)
            {
                cmd += "INSERT INTO entity_metadata(entity_id, metadata_tag, metadata_value) VALUES ('" + meta[0] + "', '" + meta[1] + "','" + meta[2].Replace("'", "''") + "');";
            }
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            query.ExecuteNonQuery();
        }

        static void MigrateFieldMetadata(SqlConnection conn)
        {
            string cmd = "SELECT * FROM field_add_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<string[]> metadata = new List<string[]>();
            while (reader.Read())
            {
                string[] entry = new string[3];
                if (!reader.IsDBNull(reader.GetOrdinal("observation_methodology")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "observation_methodology";
                    entry[2] = reader["observation_methodology"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("instrument")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "instrument";
                    entry[2] = reader["instrument"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("analysis_methodology")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "analysis_methodology";
                    entry[2] = reader["analysis_methodology"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("processing_methodology")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "processing_methodology";
                    entry[2] = reader["processing_methodology"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("description")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "description";
                    entry[2] = reader["description"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("citations")))
                {
                    entry = new string[3];
                    entry[0] = reader["field_id"].ToString();
                    entry[1] = "citations";
                    entry[2] = reader["citations"].ToString();
                    metadata.Add(entry);
                }
            }
            reader.Close();
            cmd = string.Empty;
            foreach (string[] meta in metadata)
            {
                cmd += "INSERT INTO entity_metadata(entity_id, metadata_tag, metadata_value) VALUES ('" + meta[0] + "', '" + meta[1] + "','" + meta[2].Replace("'", "''") + "');";
            }
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd,
                CommandTimeout = 100000
            };
            query.ExecuteNonQuery();
        }

        static void MigratePersonMetadata(SqlConnection conn)
        {
            string cmd = "SELECT * FROM person_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<string[]> metadata = new List<string[]>();
            while (reader.Read())
            {
                string[] entry = new string[3];
                if (!reader.IsDBNull(reader.GetOrdinal("first_name")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "firstname";
                    entry[2] = reader["first_name"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("last_name")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "lastname";
                    entry[2] = reader["last_name"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("affiliation")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "cntorg";
                    entry[2] = reader["affiliation"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("address_line1")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "address";
                    entry[2] = reader["address_line1"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("address_line2")))
                    {
                        string line2 = reader["address_line2"].ToString();
                        if (!string.IsNullOrEmpty(line2))
                            entry[2] += "\n" + line2;
                    }
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("city")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "city";
                    entry[2] = reader["city"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("state")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "state";
                    entry[2] = reader["state"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("zipcode")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "postal";
                    entry[2] = reader["zipcode"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("country")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "country";
                    entry[2] = reader["country"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("phone")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "cntvoice";
                    entry[2] = reader["phone"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("fax")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "cntfax";
                    entry[2] = reader["fax"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("email")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "cntemail";
                    entry[2] = reader["email"].ToString();
                    metadata.Add(entry);
                }
                if (!reader.IsDBNull(reader.GetOrdinal("comment")))
                {
                    entry = new string[3];
                    entry[0] = reader["personid"].ToString();
                    entry[1] = "cntinst";
                    entry[2] = reader["comment"].ToString();
                    metadata.Add(entry);
                }
            }
            reader.Close();
            cmd = string.Empty;
            foreach (string[] meta in metadata)
            {
                cmd += "INSERT INTO entity_metadata(entity_id, metadata_tag, metadata_value) VALUES ('" + meta[0] + "', '" + meta[1] + "','" + meta[2].Replace("'", "''") + "');";
            }
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd,
                CommandTimeout = 100000
            };
            query.ExecuteNonQuery();
        }

        public static void UpdateDatasetEntities(SqlConnection conn)
        {
            Person sean = new Person();
            sean.ID = new Guid("10647fb5-2f63-475d-892a-0ef3fe37bf2b");
            sean.Load(conn);
            List<Guid> ids = new List<Guid>();
            string cmd = "SELECT dataset_id FROM dataset_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    ids.Add(new Guid(reader["dataset_id"].ToString()));
                }
            }
            reader.Close();
            List<Metric> allmetrics = Metric.LoadExistingMetrics(conn);
            List<Conversion> allconversion = Conversion.LoadAll(conn, allmetrics);

            foreach (Guid g in ids)
            {
                Dataset d = new Dataset();
                d.Load(conn, g, allconversion, allmetrics);
                if (d.Owner == null)
                    d.Owner = sean;
                d.Save(conn);
            }
        }

        static void UpdateBounds(SqlConnection conn)
        {
            List<Metric> metrics = Metric.LoadExistingMetrics(conn);
            List<Conversion> conversions = Conversion.LoadAll(conn, metrics);

            string cmd = "SELECT dataset_id FROM dataset_metadata";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            SqlDataReader reader = query.ExecuteReader();
            List<Guid> ids = new List<Guid>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                    ids.Add(new Guid(reader["dataset_id"].ToString()));
            }
            reader.Close();
            foreach(Guid id in ids)
            {
                Dataset d = new Dataset();
                d.Load(conn, id, conversions, metrics);
                Console.WriteLine("Processing: " + d.GetMetadataValue("title"));
                d.UpdateBounds(conn);
                d.Save(conn);
            }
            reader.Close();
        }

        static void UpdateBounds(SqlConnection conn, Guid datasetid)
        {
            List<Metric> metrics = Metric.LoadExistingMetrics(conn);
            List<Conversion> conversions = Conversion.LoadAll(conn, metrics);

           
            List<Guid> ids = new List<Guid>();
            ids.Add(datasetid);
            foreach (Guid id in ids)
            {
                Dataset d = new Dataset();
                d.Load(conn, id, conversions, metrics);
                Console.WriteLine("Processing: " + d.GetMetadataValue("title"));
                d.UpdateBounds(conn);
                d.Save(conn);
            }
        }
        static void Main(string[] args)
        {
            string connstring = "Server=10.1.13.205;Database=Repository_Metadata; User Id=SqlServer_Client; password= p@$$w0rd";
            SqlConnection conn = new SqlConnection(connstring);
            conn.Open();
            // MigrateProgramMetadata(conn);
            // MigrateProjectMetadata(conn);
            // MigrateDatasetMetadata(conn);
            // MigrateFieldMetadata(conn);
            // MigratePersonMetadata(conn);
            // UpdateDatasetEntities(conn);
            UpdateBounds(conn, new Guid("b0a3e752-f02b-40e7-9967-70183e9993a0"));
            // UpdateBounds(conn);
            conn.Close();
        }
    }
}
