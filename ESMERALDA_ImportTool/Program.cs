using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESMERALDAClasses;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using Catfood.Shapefile;
using System.Net.Mail;
using System.Configuration;

namespace ESMERALDA_ImportTool
{
    class ModifierHolder
    {
        public string Modifier;
        public bool IsPrefix;
        public bool IsSuffix;

        public ModifierHolder()
        {
            Modifier = string.Empty;
            IsPrefix = false;
            IsSuffix = false;
        }

        public static List<ModifierHolder> LoadAllModifiers(SqlConnection conn)
        {
            string cmd = "SELECT * FROM Analyte_Modifiers";
            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandText = cmd,
                CommandType = CommandType.Text
            };
            List<ModifierHolder> ret = new List<ModifierHolder>();
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("Modifier")))
                {
                    ModifierHolder mod = new ModifierHolder();
                    mod.Modifier = reader["Modifier"].ToString();
                    mod.IsPrefix = bool.Parse(reader["Prefix"].ToString());
                    mod.IsSuffix = bool.Parse(reader["Suffix"].ToString());
                    ret.Add(mod);
                }
            }
            reader.Close();
            return ret;
        }
    }

    class Program
    {
        public static SqlConnection ConnectToDatabase(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Client; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        public static void BuildChemicalLookupTable(SqlConnection conn)
        {
            List<string[]> entries = new List<string[]>();
            List<ModifierHolder> mods = ModifierHolder.LoadAllModifiers(conn);
            SqlCommand query = null;
            string cmd = "TRUNCATE TABLE Analyte_Map;";
            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = cmd
            };
            query.ExecuteNonQuery();

            query = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = "SELECT * FROM Analyte_Disambiguation_Information"
            };            
            SqlDataReader reader = query.ExecuteReader();
            while(reader.Read())
            {
                string primary = reader["Primary_Name"].ToString();
                if(string.IsNullOrEmpty(primary))
                    continue;                

                for(int i=0; i < reader.FieldCount; i++)
                {
                    if(!reader.IsDBNull(i))
                    {
                        string val = reader[i].ToString();
                        if(!string.IsNullOrEmpty(val))
                        {
                            entries.Add(new string[]{primary, val, string.Empty});
                            foreach (ModifierHolder mod in mods)
                            {
                                if (mod.IsPrefix)
                                {
                                    entries.Add(new string[] { primary, mod.Modifier + " " + val, mod.Modifier });
                                }
                                if (mod.IsSuffix)
                                {
                                    entries.Add(new string[] { primary, val + " " + mod.Modifier, mod.Modifier });
                                }
                            }
                        }
                    }
                }
            }
            reader.Close();
            int counter = 0;
            foreach(string[] pair in entries)
            {
                cmd += "INSERT INTO Analyte_Map(Standard_Name, Analyte_Name, Modifier) VALUES('" + pair[0].Replace("'", "''") + "', '" + pair[1].Replace("'", "''") + "'";
                if (string.IsNullOrEmpty(pair[2]))
                {
                    cmd += ", NULL);";
                }
                else
                {
                    cmd += ", '" + pair[2] + "');";
                }
                counter += 1;
                if (counter >= 100)
                {
                    counter = 0;
                    query = new SqlCommand()
                    {
                        Connection = conn,
                        CommandType = CommandType.Text,
                        CommandText = cmd
                    };
                    query.ExecuteNonQuery();
                    cmd = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(cmd))
            {
                query = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandText = cmd
                };
                query.ExecuteNonQuery();
            }
        }

        public static void PivotDFOTable(SqlConnection conn)
        {
            string connstring = "Server=10.1.13.205;Database=Gulf_Integrated_Spill_Research; User Id=SqlServer_Client; password= p@$$w0rd";
            SqlConnection writeconn = new SqlConnection(connstring);
            writeconn.Open();
            List<string> meta_columns = new List<string>();
            meta_columns.Add("SampleID");
            meta_columns.Add("Vessel");
            meta_columns.Add("Station");
            meta_columns.Add("Cast");
            meta_columns.Add("Bottle");
            string cmd = "SELECT * FROM DFO_Chem_Data";
            SqlCommand write_query = new SqlCommand()
            {
                Connection = writeconn,
                CommandText = "TRUNCATE TABLE DFO_Chem_Data_Pivoted",
                CommandType = CommandType.Text
            };
            write_query.ExecuteNonQuery();

            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandText = cmd,
                CommandType = CommandType.Text
            };
            SqlDataReader reader = query.ExecuteReader();
            string SampleID = string.Empty;
            string Vessel = string.Empty;
            string Station = string.Empty;
            string Cast = string.Empty;
            string Bottle = string.Empty;
            string Analyte = string.Empty;

            while (reader.Read())
            {
                SampleID = reader["SampleID"].ToString();
                Vessel = reader["Vessel"].ToString();
                Station = reader["Station"].ToString();
                Cast = reader["Cast"].ToString();
                Bottle = reader["Bottle"].ToString();
                string write_cmd = string.Empty;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                        continue;
                    if(meta_columns.Contains(reader.GetName(i)))
                        continue;
                    Analyte = reader.GetName(i).Replace("_", " ");
                    write_cmd += "INSERT INTO DFO_Chem_Data_Pivoted(SampleID, Vessel, Station, Cast, Bottle, Analyte, Result) VALUES ('" + SampleID + "', '" + Vessel + "', " + Station + ", " + Cast + ", '" + Bottle + "', '" + Analyte + "', " + reader[i].ToString() + ");";
                }
                write_query = new SqlCommand()
                {
                    Connection = writeconn,
                    CommandText = write_cmd,
                    CommandType = CommandType.Text
                };
                write_query.ExecuteNonQuery();
            }
            reader.Close();
            writeconn.Close();
        }

        public static void PivotCSIROTable(SqlConnection conn)
        {
            string connstring = "Server=10.1.13.205;Database=Gulf_Integrated_Spill_Research; User Id=SqlServer_Client; password= p@$$w0rd";
            SqlConnection writeconn = new SqlConnection(connstring);
            writeconn.Open();
            List<string> meta_columns = new List<string>();
            meta_columns.Add("OID");
            meta_columns.Add("Raw_ID");
            meta_columns.Add("Sample_ID");
            meta_columns.Add("Method_ID");
            meta_columns.Add("Equation_Used");
            meta_columns.Add("Analysis_date");
            meta_columns.Add("GC-MS_SIMfile");
            meta_columns.Add("LinkedValidtnReport");
            meta_columns.Add("txtLinkedValidtnReport");
            meta_columns.Add("LinkedMethodReport");
            meta_columns.Add("txtLinkedMethodReport");
            meta_columns.Add("LinkedImageName");
            meta_columns.Add("txtLinkedImageName");

            string cmd = "SELECT * FROM CSIRO_tblGCMSresults";
            SqlCommand write_query = new SqlCommand()
            {
                Connection = writeconn,
                CommandText = "TRUNCATE TABLE CSIRO_tblGCMSresults_Pivoted",
                CommandType = CommandType.Text
            };
            write_query.ExecuteNonQuery();

            SqlCommand query = new SqlCommand()
            {
                Connection = conn,
                CommandText = cmd,
                CommandType = CommandType.Text
            };
            SqlDataReader reader = query.ExecuteReader();
            Dictionary<string, string> meta_holder = new Dictionary<string,string>();
            string analyte = string.Empty;
            string result = string.Empty;
            string flag = string.Empty;
            string col_name = string.Empty;
            while (reader.Read())
            {
                meta_holder.Clear();
                string field_header = string.Empty;
                foreach (string s in meta_columns)
                {
                    if (reader.IsDBNull(reader.GetOrdinal(s)))
                    {
                        meta_holder.Add(s, null);
                    }
                    else
                    {
                        meta_holder.Add(s, reader[s].ToString());
                    }
                }
                field_header = string.Empty;
                string value_header = string.Empty;
                foreach(string s in meta_holder.Keys)
                {
                    if (string.IsNullOrEmpty(field_header))
                    {
                        field_header = "INSERT INTO [CSIRO_tblGCMSresults_Pivoted]([" + s + "]";                         
                    }
                    else
                    {
                        field_header += ",[" + s + "]";
                    }
                    if(meta_holder[s] == null)
                    {
                        if (string.IsNullOrEmpty(value_header))
                        {
                            value_header = "(NULL";
                        }
                        else{
                            value_header += ",NULL";
                        }
                    }
                    else{
                        if (string.IsNullOrEmpty(value_header))
                        {
                            value_header = "('" + meta_holder[s] + "'";
                        }
                        else{
                            value_header += ",'" + meta_holder[s] + "'";
                        }
                    }
                }
                string write_cmd = string.Empty;
                field_header += ", [Analyte], [Result], [Quality_Flag]) VALUES ";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    col_name = reader.GetName(i);
                    if (meta_columns.Contains(col_name))
                    {
                        continue;
                    }
                    if (reader.IsDBNull(i))
                        continue;
                    if (col_name.IndexOf("Q-") == 0)
                        continue;
                    analyte = col_name.Replace("_", " ");
                    result = reader[i].ToString();
                    flag = reader["Q-" + col_name].ToString();
                    write_cmd += field_header + value_header + ", '" + analyte + "', " + result + ", '" + flag + "');";
                }
                
                write_query = new SqlCommand()
                {
                    Connection = writeconn,
                    CommandText = write_cmd,
                    CommandType = CommandType.Text
                };
                write_query.ExecuteNonQuery();
            }
            reader.Close();
            writeconn.Close();
        }

        static void LoadShapefile(string shapefilename, string dest_table, SqlConnection conn)
        {
            Shapefile shp = new Shapefile(shapefilename);
            foreach (Shape s in shp)
            {
                string[] fields = s.GetMetadataNames();
                foreach(string f in fields)
                {
                    System.Diagnostics.Debug.WriteLine(f);
                }
            }
        }

        private void FlagRequest(SqlConnection conn, Guid requestid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_FlagRequest"
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.ExecuteNonQuery();
        }

        // Send an email from the application one or more users.
        public static void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body, string fromaddress, string fromuser, string frompassword, string emailport, string emailhost)
        {
            MailMessage mail = new MailMessage();
            try
            {
                mail.From = new MailAddress(fromaddress);
            }
            catch (Exception)
            {
                return;
            }
            if (to != null)
            {
                foreach (string s in to)
                {
                    try
                    {
                        mail.To.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (cc != null)
            {
                foreach (string s in cc)
                {
                    try
                    {
                        mail.CC.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (bcc != null)
            {
                foreach (string s in bcc)
                {
                    try
                    {
                        mail.Bcc.Add(new MailAddress(s));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Port = int.Parse(emailport);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = emailhost;
            client.Credentials = new System.Net.NetworkCredential(fromuser, frompassword);
            client.EnableSsl = true; // runtime encrypt the SMTP communications using SSL
            client.Send(mail);
        }

        // Send an email from the application one or more users.
        public static void SendEmail(string[] to, string[] cc, string[] bcc, string subject, string body)
        {
            string fromaddress = System.Configuration.ConfigurationManager.AppSettings.Get("emailaddress");
            string fromuser = System.Configuration.ConfigurationManager.AppSettings.Get("emailuser");
            string frompassword = System.Configuration.ConfigurationManager.AppSettings.Get("emailpassword");
            string emailport = System.Configuration.ConfigurationManager.AppSettings.Get("emailport");
            string emailhost = System.Configuration.ConfigurationManager.AppSettings.Get("emailhost");
            SendEmail(to, cc, bcc, subject, body, fromaddress, fromuser, frompassword, emailport, emailhost);
        }

        private static SqlConnection ConnectToDatabaseReadOnly(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Reader; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        private static void DoProcessing()
        {
                try
                {
                    SqlConnection conn = new SqlConnection((string)Properties.Settings.Default["metadata_conn"]);
                    string save_path = "c:\\TempImageFiles\\";
                    string link_path = "http://foo.com/";
                    conn.Open();
                    List<DownloadRequest> reqs = DownloadRequest.LoadUnprocessedRequests(conn);
                    if (reqs.Count > 0)
                    {
                        List<Metric> metrics = Metric.LoadExistingMetrics(conn);
                        List<Conversion> conversions = Conversion.LoadAll(conn, metrics);
                        foreach (DownloadRequest d in reqs)
                        {
                            ESMERALDAClasses.Person user = new Person();
                            user.Load(conn, d.userid);
                            if (user == null || string.IsNullOrEmpty(user.GetMetadataValue("cntemail")))
                                continue;
                            string source_type = Utils.GetEntityType(d.sourceid, conn);
                            View v = new View();
                            v.Load(conn, d.sourceid, conversions, metrics);
                            string dbname = v.ParentContainer.database_name;
                            SqlConnection dconn = ConnectToDatabaseReadOnly(dbname);
                            FileStream fs = File.Open(save_path + d.filename, FileMode.CreateNew);
                            StreamWriter sw = new StreamWriter(fs);
                            v.WriteDataToStream(d.delimiter, sw, dconn);
                            dconn.Close();
                            fs.Flush();
                            fs.Close();

                            string file_url = link_path + fs;
                            string body = "<p>Your GISR download is ready.  <a href='" + file_url + "'>Click here to download the file.</a>";
                            string subject = "GISR Download Ready";
                            string[] to = new string[1];
                            to[0] = user.GetMetadataValue("cntemail");
                            SendEmail(to, null, null, subject, body);
                            d.FlagRequest(conn);
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                }
        }

        static void Main(string[] args)
        {
            string connstring = "Server=10.1.13.205;Database=Gulf_Integrated_Spill_Research; User Id=SqlServer_Client; password= p@$$w0rd";
            SqlConnection conn = new SqlConnection(connstring);
            conn.Open();
            // DoProcessing();
            // LoadShapefile(args[0], "", conn);
            BuildChemicalLookupTable(conn);
            // PivotCSIROTable(conn);
            // PivotDFOTable(conn);
            conn.Close();

            /*if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
                return;
            Guid dsid = new Guid(args[0]);

            Dataset working = new Dataset();
            string connstring = "Server=10.1.13.205;Database=Repository_Metadata; User Id=SqlServer_Client; password= p@$$w0rd";
            SqlConnection conn = new SqlConnection(connstring);
            conn.Open();
            List<Metric> Metrics = Metric.LoadExistingMetrics(conn);
            List<Conversion> Conversions = Conversion.LoadAll(conn, Metrics);

            working.Load(conn, dsid, Conversions, Metrics);

            string filename = args[1];
            FileStream fs = File.Open(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs, true);
            List<string> rows = new List<string>();
            string curr = string.Empty;
            char delim_char = ',';
            Dictionary<string, int> colmap = new Dictionary<string, int>();
            DataTable saved_table = null;
            SqlConnection working_connection = ConnectToDatabase(working.ParentProject.database_name);
            SqlTableCreator creator = new SqlTableCreator(working_connection);
            creator.DestinationTableName = working.SQLName;
            while (!sr.EndOfStream)
            {
                curr = sr.ReadLine();
                if (!string.IsNullOrEmpty(curr))
                    rows.Add(curr);
                if (rows.Count > 1000)
                {
                    if (saved_table == null)
                    {
                        Console.WriteLine("Building table.");
                        FinishProcessingUpload(conn, rows, ref working, ref colmap, ref delim_char, ref saved_table, Metrics);
                        DataTable newtable = working.BuildDataTable(saved_table, -1);
                        creator.WriteData(newtable);
                    }
                    else
                    {
                        Console.WriteLine("Dumping data.");
                        saved_table.Rows.Clear();
                        saved_table = working.SaveTemporaryData(rows, colmap, saved_table, delim_char);
                        DataTable newtable = working.BuildDataTable(saved_table, -1);
                        creator.WriteData(newtable);
                    }
                    rows.Clear();
                }
            }
            sr.Close();
            fs.Close();
            if (rows.Count > 0)
            {
                if (saved_table == null)
                {
                    Console.WriteLine("Building table.");
                    FinishProcessingUpload(conn, rows, ref working, ref colmap, ref delim_char, ref saved_table, Metrics);
                    DataTable newtable = working.BuildDataTable(saved_table, -1);
                    creator.WriteData(newtable);
                }
                else
                {
                    Console.WriteLine("Dumping data.");
                    saved_table.Rows.Clear();
                    saved_table = working.SaveTemporaryData(rows, colmap, saved_table, delim_char);
                    DataTable newtable = working.BuildDataTable(saved_table, -1);
                    creator.WriteData(newtable);
                }
                rows.Clear();
            }                       
            
            working_connection.Close();
            working.UpdateBounds(conn);
            conn.Close();*/
        }

        /*protected static void FinishProcessingUpload(SqlConnection conn, List<string> rows, ref Dataset working, ref Dictionary<string, int> colmap, ref char delim_char, ref DataTable saved_table, List<Metric> Metrics)
        {
            int i;
            string[] header_fields;
            int j;
            List<string> missing_fields;
            List<string> extra_fields;
            string msg;

            DateTime functionstarttime = DateTime.Now;
            DateTime starttime = DateTime.Now;
            if (rows.Count == 0)
            {
                return;
            }
            TimeSpan debugtime = (TimeSpan)(DateTime.Now - starttime);
            starttime = DateTime.Now;

            char[] delims = { ',', ' ', '\t' };
            int[] num_fields_by_token = new int[delims.Length];
            int num_samples = Math.Min(3, rows.Count);
            int[] working_counts = new int[num_samples];            
            for (i = 0; i < delims.Length; i++)
            {
                for (j = 0; j < num_samples; j++)
                {
                    Random r = new Random();
                    int row_index = r.Next(rows.Count - 1);
                    string[] tokens = Utils.ParseCSVRow(rows[row_index], delims[i]);
                    working_counts[j] = tokens.Length;
                }
                if (num_samples == 1)
                {
                    num_fields_by_token[i] = working_counts[0];
                }
                else if (num_samples == 2)
                {
                    if (working_counts[0] == working_counts[1])
                    {
                        num_fields_by_token[i] = working_counts[0];
                    }
                    else
                    {
                        num_fields_by_token[i] = -1;
                    }
                }
                else
                {
                    if (working_counts[0] == working_counts[1] || working_counts[0] == working_counts[2])
                    {
                        num_fields_by_token[i] = working_counts[0];
                    }
                    else if (working_counts[1] == working_counts[2])
                    {
                        num_fields_by_token[i] = working_counts[1];
                    }
                    else
                    {
                        num_fields_by_token[i] = -1;
                    }
                }
            }
            int num_fields = 0;
            for (i = 0; i < delims.Length; i++)
            {
                if (num_fields_by_token[i] > num_fields)
                {
                    num_fields = num_fields_by_token[i];
                    delim_char = delims[i];
                }
            }
            List<Field> fields = new List<Field>();
            bool error = false;
            missing_fields = new List<string>();
            extra_fields = new List<string>();
            int count = 0;
            if ((working.Header != null) && (working.Header.Count > 0))
            {
                foreach (Field f in working.Header)
                {
                    missing_fields.Add(f.SourceColumnName);
                }
                for (i = 0; i < rows.Count; i++)
                {
                    string s = rows[i];
                    count = 1;
                    foreach (char c in s)
                    {
                        if (c == delim_char)
                        {
                            count++;
                        }
                    }
                    if (count == num_fields)
                    {
                        rows.RemoveRange(0, i + 1);
                        header_fields = Utils.ParseCSVRow(s, delim_char);
                        for (j = 0; j < header_fields.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(header_fields[j].Trim()))
                            {
                                string curr_head_col = header_fields[j].Trim();
                                if (!colmap.ContainsKey(curr_head_col))
                                {
                                    colmap.Add(curr_head_col, j);
                                    if (!missing_fields.Contains(curr_head_col))
                                    {
                                        extra_fields.Add(curr_head_col);
                                    }
                                    else
                                    {
                                        missing_fields.Remove(curr_head_col);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                for (i = 0; i < rows.Count; i++)
                {
                    string s = rows[i];
                    count = 1;
                    foreach (char c in s)
                    {
                        if (c == delim_char)
                        {
                            count++;
                        }
                    }
                    if (count == num_fields)
                    {
                        rows.RemoveRange(0, i + 1);
                        header_fields = null;
                        header_fields = Utils.ParseCSVRow(s, delim_char);
                        for (j = 0; j < header_fields.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(header_fields[j].Trim()))
                            {
                                Field f = new Field
                                {
                                    SourceColumnName = header_fields[j].Trim()
                                };
                                if (!colmap.ContainsKey(f.SourceColumnName))
                                {
                                    colmap.Add(f.SourceColumnName, j);
                                    f.Name = Field.ExtractColumnName(f.SourceColumnName);
                                    f.FieldMetric = Field.RecommendMetric(f.SourceColumnName, Metrics);
                                    fields.Add(f);
                                }
                            }
                        }
                        break;
                    }
                }
                working.Header = new List<QueryField>();
                foreach (Field f2 in fields)
                {
                    f2.Parent = working;
                    working.Header.Add(f2);
                }
            }
            if (missing_fields.Count > 0)
            {
                msg = "The new data is missing the following columns; it can not be added to this dataset: " + missing_fields[0];
                for (i = 1; i < missing_fields.Count; i++)
                {
                    msg = msg + ", " + missing_fields[i];
                }
                throw new Exception(msg);
            }
            else if (extra_fields.Count > 0)
            {
                msg = "The new data has the following extra columns; these fields will not be added to the dataset: " + extra_fields[0];
                for (i = 1; i < extra_fields.Count; i++)
                {
                    msg = msg + ", " + extra_fields[i];
                }
                throw new Exception(msg);
            }
            saved_table = working.SaveTemporaryData(rows, colmap, saved_table, delim_char);
            debugtime = (TimeSpan)(DateTime.Now - starttime);
            starttime = DateTime.Now;
        }*/
    }
}
