namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;

    public class Dataset : EsmeraldaEntity
    {
        public string AcquisitionDescription = string.Empty;
        public string BriefDescription = string.Empty;
        public List<string[]> Data = new List<string[]>();
        public string Description = string.Empty;
        public Field[] Header = null;
        public bool IsEditable = true;
        public double MaxLat = double.NaN;
        public double MaxLon = double.NaN;
        public double MinLat = double.NaN;
        public double MinLon = double.NaN;
        public string Name = string.Empty;
        public Project ParentProject;
        public string ProcessingDescription = string.Empty;
        public string TableName = string.Empty;
        public string URL = string.Empty;
        public int Version = -1;

        public DataTable BuildDataTable(DataTable data, int num_rows)
        {
            int i;
            DataTable ret = new DataTable(this.Name);
            for (i = 0; i < this.Header.Length; i++)
            {
                Field f = this.Header[i];
                if (!f.IsSubfield)
                {
                    DataColumn newfield;
                    if (string.IsNullOrEmpty(f.SQLColumnName))
                    {
                        f.SQLColumnName = Utils.CreateDBName(f.Name);
                    }
                    if (f.DBType == Field.FieldType.DateTime)
                    {
                        newfield = new DataColumn {
                            DataType = typeof(DateTime),
                            ColumnName = f.SQLColumnName
                        };
                        ret.Columns.Add(newfield);
                    }
                    else if (f.DBType == Field.FieldType.Time)
                    {
                        newfield = new DataColumn {
                            DataType = typeof(TimeSpan),
                            ColumnName = f.SQLColumnName
                        };
                        ret.Columns.Add(newfield);
                    }
                    else if (f.DBType == Field.FieldType.Decimal)
                    {
                        newfield = new DataColumn {
                            DataType = typeof(double),
                            ColumnName = f.SQLColumnName
                        };
                        ret.Columns.Add(newfield);
                    }
                    else if (f.DBType == Field.FieldType.Integer)
                    {
                        newfield = new DataColumn {
                            DataType = typeof(int),
                            ColumnName = f.SQLColumnName
                        };
                        ret.Columns.Add(newfield);
                    }
                    else if (f.DBType == Field.FieldType.Text)
                    {
                        newfield = new DataColumn {
                            DataType = typeof(string),
                            ColumnName = f.SQLColumnName
                        };
                        ret.Columns.Add(newfield);
                    }
                }
            }
            int curr_row = -1;
            int row = 0;
            DataRow newrow = null;
            string datum = string.Empty;
            string sourcecol = string.Empty;
            int curr_row_start = -1;
            for (i = 0; i < data.Rows.Count; i++)
            {
                if ((i % 0x3e8) == 0)
                {
                    Debug.WriteLine("Processing " + i.ToString() + " of " + data.Rows.Count.ToString());
                }
                row = (int) data.Rows[i]["RowNumber"];
                if (row != curr_row)
                {
                    if (newrow != null)
                    {
                        ret.Rows.Add(newrow);
                    }
                    newrow = ret.NewRow();
                    curr_row = row;
                    curr_row_start = i;
                    if ((num_rows > 0) && (curr_row > num_rows))
                    {
                        break;
                    }
                }
                sourcecol = (string) data.Rows[i]["SourceColumnName"];
                Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
                Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");
                foreach (Field f in this.Header)
                {
                    if (f.SourceColumnName == sourcecol)
                    {
                        if (!f.IsSubfield)
                        {
                            datum = (string) data.Rows[i]["Data"];
                            if ((f.DBType != Field.FieldType.Text) && string.IsNullOrEmpty(datum))
                            {
                                newrow[f.SQLColumnName] = DBNull.Value;
                            }
                            else
                            {
                                try
                                {
                                    double d_val;
                                    if ((f.DBType == Field.FieldType.DateTime) || (f.DBType == Field.FieldType.Time))
                                    {
                                        DateTime startdate;
                                        double tmp;
                                        if (f.Subfield == null)
                                        {
                                            if (f.DBType == Field.FieldType.DateTime)
                                            {
                                                if (f.FieldMetric.Name.IndexOf("Year Day") == 0)
                                                {
                                                    startdate = DateTime.Parse(f.FieldMetric.Abbrev);
                                                    if (!double.TryParse(datum, out d_val))
                                                    {
                                                        newrow[f.SQLColumnName] = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        newrow[f.SQLColumnName] = startdate + TimeSpan.FromDays(d_val);
                                                    }
                                                }
                                                else
                                                {
                                                    DateTime dt_val;
                                                    if (!DateTime.TryParse(datum, out dt_val))
                                                    {
                                                        newrow[f.SQLColumnName] = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        newrow[f.SQLColumnName] = dt_val;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                TimeSpan ts_val;
                                                if (!double.TryParse(datum, out d_val))
                                                {
                                                    if (!TimeSpan.TryParse(datum, out ts_val))
                                                    {
                                                        newrow[f.SQLColumnName] = DBNull.Value;
                                                    }
                                                    else
                                                    {
                                                        newrow[f.SQLColumnName] = ts_val;
                                                    }
                                                }
                                                else
                                                {
                                                    ts_val = TimeSpan.FromSeconds(d_val);
                                                    newrow[f.SQLColumnName] = ts_val;
                                                }
                                            }
                                            break;
                                        }
                                        int j = curr_row_start;
                                        string datum2 = string.Empty;
                                        TimeSpan timecomponent = TimeSpan.Zero;
                                        DateTime datecomponent = DateTime.MinValue;
                                        while (j < data.Rows.Count)
                                        {
                                            if (((int) data.Rows[j]["RowNumber"]) != row)
                                            {
                                                break;
                                            }
                                            if (((string) data.Rows[j]["SourceColumnName"]) == f.Subfield.SourceColumnName)
                                            {
                                                datum2 = (string) data.Rows[j]["Data"];
                                                break;
                                            }
                                            j++;
                                        }
                                        if (f.DBType == Field.FieldType.DateTime)
                                        {
                                            if (f.FieldMetric.Name.IndexOf("Year Day") == 0)
                                            {
                                                startdate = DateTime.Parse(f.FieldMetric.Abbrev);
                                                if (!double.TryParse(datum, out d_val))
                                                {
                                                    datecomponent = startdate + TimeSpan.FromDays(d_val);
                                                }
                                            }
                                            else
                                            {
                                                DateTime.TryParse(datum, out datecomponent);
                                            }
                                        }
                                        else
                                        {
                                            tmp = 0.0;
                                            if (double.TryParse(datum, out tmp))
                                            {
                                                timecomponent = TimeSpan.FromSeconds(tmp);
                                            }
                                            else
                                            {
                                                TimeSpan.TryParse(datum, out timecomponent);
                                            }
                                        }
                                        if (f.Subfield.DBType == Field.FieldType.DateTime)
                                        {
                                            if (f.FieldMetric.Name.IndexOf("Year Day") == 0)
                                            {
                                                startdate = DateTime.Parse(f.FieldMetric.Abbrev);
                                                if (!double.TryParse(datum2, out d_val))
                                                {
                                                    datecomponent = startdate + TimeSpan.FromDays(d_val);
                                                }
                                            }
                                            else
                                            {
                                                DateTime.TryParse(datum2, out datecomponent);
                                            }
                                        }
                                        else
                                        {
                                            tmp = 0.0;
                                            if (double.TryParse(datum2, out tmp))
                                            {
                                                timecomponent = TimeSpan.FromSeconds(tmp);
                                            }
                                            else
                                            {
                                                TimeSpan.TryParse(datum2, out timecomponent);
                                            }
                                        }
                                        if ((datecomponent == DateTime.MinValue) && (timecomponent == TimeSpan.Zero))
                                        {
                                            newrow[f.SQLColumnName] = DBNull.Value;
                                        }
                                        else
                                        {
                                            newrow[f.SQLColumnName] = Utils.ConvertDateToUTC(datecomponent + timecomponent, f.FieldMetric.CurrentTimeZone);
                                        }
                                        break;
                                    }
                                    if (f.DBType == Field.FieldType.Decimal)
                                    {
                                        if (double.TryParse(datum, out d_val))
                                        {
                                            if (double.IsNaN(d_val))
                                            {
                                                newrow[f.SQLColumnName] = DBNull.Value;
                                            }
                                            else
                                            {
                                                newrow[f.SQLColumnName] = d_val;
                                                if (f.FieldMetric.ID == lat_guid)
                                                {
                                                    if (double.IsNaN(this.MinLat))
                                                    {
                                                        this.MinLat = d_val;
                                                        this.MaxLat = d_val;
                                                    }
                                                    else if (d_val < this.MinLat)
                                                    {
                                                        this.MinLat = d_val;
                                                    }
                                                    else if (d_val > this.MaxLat)
                                                    {
                                                        this.MaxLat = d_val;
                                                    }
                                                }
                                                if (f.FieldMetric.ID == lon_guid)
                                                {
                                                    if (double.IsNaN(this.MinLon))
                                                    {
                                                        this.MinLon = d_val;
                                                        this.MaxLon = d_val;
                                                    }
                                                    else if (d_val < this.MinLon)
                                                    {
                                                        this.MinLon = d_val;
                                                    }
                                                    else if (d_val > this.MaxLon)
                                                    {
                                                        this.MaxLon = d_val;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            newrow[f.SQLColumnName] = DBNull.Value;
                                        }
                                    }
                                    else if (f.DBType == Field.FieldType.Integer)
                                    {
                                        int i_val;
                                        if (int.TryParse(datum, out i_val))
                                        {
                                            newrow[f.SQLColumnName] = i_val;
                                        }
                                        else
                                        {
                                            newrow[f.SQLColumnName] = DBNull.Value;
                                        }
                                    }
                                    else if (f.DBType == Field.FieldType.Text)
                                    {
                                        newrow[f.SQLColumnName] = datum;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            if (newrow != null)
            {
                ret.Rows.Add(newrow);
            }
            return ret;
        }

        public void DeleteExistingData()
        {
            string dbname = this.ParentProject.database_name;
            string tablename = this.TableName;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            string cmd = "TRUNCATE TABLE " + this.TableName;
            new SqlCommand { Connection = dataconn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteNonQuery();
            dataconn.Close();
        }

        public string GetMetadata()
        {
            string ret = string.Empty;
            ret = "<dataset>";
            ret = (((((((ret + "<dataset_name>" + this.Name + "</dataset_name>") + "<brief_description>" + this.BriefDescription + "</brief_description>") + "<description>" + this.Description + "</description>") + "<dataset_url>" + this.URL + "</dataset_url>") + "<acquisition_description>" + this.AcquisitionDescription + "</acquisition_description>") + "<processing_description>" + this.ProcessingDescription + "</processing_description>") + "<version>" + this.Version.ToString() + "</version>") + "<fields>";
            foreach (Field f in this.Header)
            {
                ret = ret + f.GetMetadata();
            }
            return (ret + "</fields>" + "</dataset>");
        }

        public static Dataset Load(SqlConnection conn, Guid inID, List<Metric> metrics)
        {
            Dataset ret = new Dataset {
                ID = inID
            };
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadDataset",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inID", inID));
            SqlDataReader reader = query.ExecuteReader();
            Guid projectid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("dataset_name")))
                    {
                        ret.Name = reader["dataset_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("brief_description")))
                    {
                        ret.BriefDescription = reader["brief_description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("dataset_description")))
                    {
                        ret.Description = reader["dataset_description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("acquisition_description")))
                    {
                        ret.AcquisitionDescription = reader["acquisition_description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("processing_description")))
                    {
                        ret.ProcessingDescription = reader["processing_description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("version")))
                    {
                        ret.Version = int.Parse(reader["version"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                    {
                        projectid = new Guid(reader["project_id"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("sql_table_name")))
                    {
                        ret.TableName = reader["sql_table_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("editable")))
                    {
                        ret.IsEditable = bool.Parse(reader["editable"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                    {
                        ret.MinLon = double.Parse(reader["min_lon"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("min_lat")))
                    {
                        ret.MinLat = double.Parse(reader["min_lat"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("max_lon")))
                    {
                        ret.MaxLon = double.Parse(reader["max_lon"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("max_lat")))
                    {
                        ret.MaxLat = double.Parse(reader["max_lat"].ToString());
                    }
                }
            }
            reader.Close();
            query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadFields",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inDatasetID", inID));
            reader = query.ExecuteReader();
            List<Field> newfields = new List<Field>();
            while (reader.Read())
            {
                if (reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    continue;
                }
                Field newfield = new Field();
                if (!reader.IsDBNull(reader.GetOrdinal("field_name")))
                {
                    newfield.Name = reader["field_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("metric_id")))
                {
                    Guid mid = new Guid(reader["metric_id"].ToString());
                    foreach (Metric m in metrics)
                    {
                        if (m.ID == mid)
                        {
                            newfield.FieldMetric = m;
                            break;
                        }
                    }
                }
                if (!reader.IsDBNull(reader.GetOrdinal("source_column_name")))
                {
                    newfield.SourceColumnName = reader["source_column_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("db_type")))
                {
                    newfield.DBType = (Field.FieldType) int.Parse(reader["db_type"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("field_id")))
                {
                    newfield.ID = new Guid(reader["field_id"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("sql_column_name")))
                {
                    newfield.SQLColumnName = reader["sql_column_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("subfield_id")))
                {
                    newfield.SubfieldID = new Guid(reader["subfield_id"].ToString());
                }
                newfields.Add(newfield);
            }
            reader.Close();
            query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadFieldsAdditionalMetadata",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inDatasetID", inID));
            reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    Guid fieldid = new Guid(reader["field_id"].ToString());
                    foreach (Field f in newfields)
                    {
                        if (f.ID == fieldid)
                        {
                            if (f.Metadata == null)
                            {
                                f.Metadata = new Field_Metadata();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("observation_methodology")))
                            {
                                f.Metadata.observation_methodology = reader["observation_methodology"].ToString();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("instrument")))
                            {
                                f.Metadata.instrument = reader["instrument"].ToString();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("analysis_methodology")))
                            {
                                f.Metadata.analysis_methodology = reader["analysis_methodology"].ToString();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("processing_methodology")))
                            {
                                f.Metadata.processing_methodology = reader["processing_methodology"].ToString();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("citations")))
                            {
                                f.Metadata.citations = reader["citations"].ToString();
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("description")))
                            {
                                f.Metadata.description = reader["description"].ToString();
                            }
                            break;
                        }
                    }
                }
            }
            reader.Close();
            EsmeraldaEntity.Load(conn, ret);
            ret.Header = newfields.ToArray<Field>();
            foreach (Field f in ret.Header)
            {
                EsmeraldaEntity.Load(conn, f);
                if (f.SubfieldID != Guid.Empty)
                {
                    foreach (Field f2 in ret.Header)
                    {
                        if (f2.ID == f.SubfieldID)
                        {
                            f.Subfield = f2;
                            f2.IsSubfield = true;
                            break;
                        }
                    }
                }
            }
            if (projectid != Guid.Empty)
            {
                ret.ParentProject = Project.Load(conn, projectid);
            }
            return ret;
        }

        public DataTable MoveExistingDataToTemp(SqlConnection conn, Guid myId)
        {
            if ((base.ID == Guid.Empty) || string.IsNullOrEmpty(this.TableName))
            {
                return null;
            }
            DataTable dt = new DataTable("tmpdata");
            DataColumn sessionID = new DataColumn {
                DataType = typeof(Guid),
                ColumnName = "SessionID"
            };
            dt.Columns.Add(sessionID);
            DataColumn rowNumber = new DataColumn {
                DataType = Type.GetType("System.Int32"),
                ColumnName = "RowNumber"
            };
            dt.Columns.Add(rowNumber);
            DataColumn data = new DataColumn {
                DataType = Type.GetType("System.String"),
                ColumnName = "Data"
            };
            dt.Columns.Add(data);
            DataColumn sourcecolumnname = new DataColumn {
                DataType = Type.GetType("System.String"),
                ColumnName = "SourceColumnName"
            };
            dt.Columns.Add(sourcecolumnname);
            string dbname = this.ParentProject.database_name;
            string tablename = this.TableName;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            int row = 0;
            string cmd = "SELECT * FROM " + this.TableName;
            SqlDataReader reader = new SqlCommand { Connection = dataconn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < this.Header.Length; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["SessionID"] = myId;
                    dr["RowNumber"] = row;
                    if (!this.Header[i].IsSubfield)
                    {
                        if (this.Header[i].DBType == Field.FieldType.DateTime)
                        {
                            if (this.Header[i].Subfield != null)
                            {
                                dr["Data"] = DateTime.Parse(reader[this.Header[i].SQLColumnName].ToString()).Date.ToString();
                            }
                            else
                            {
                                dr["Data"] = reader[this.Header[i].SQLColumnName].ToString();
                            }
                        }
                        else
                        {
                            dr["Data"] = reader[this.Header[i].SQLColumnName].ToString();
                        }
                    }
                    dr["SourceColumnName"] = this.Header[i].SourceColumnName;
                    dt.Rows.Add(dr);
                    if (this.Header[i].Subfield != null)
                    {
                        DataRow dr2 = dt.NewRow();
                        dr2["Data"] = DateTime.Parse(reader[this.Header[i].SQLColumnName].ToString()).TimeOfDay.ToString();
                        dr2["SessionID"] = myId;
                        dr2["RowNumber"] = row;
                        dr2["SourceColumnName"] = this.Header[i].Subfield.SourceColumnName;
                        dt.Rows.Add(dr2);
                    }
                }
                row++;
            }
            reader.Close();
            dataconn.Close();
            dt.AcceptChanges();
            return dt;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(this.TableName))
            {
                this.TableName = Utils.CreateDBName(this.Name);
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteDataset";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@indataset_id", base.ID));
            query.Parameters.Add(new SqlParameter("@indataset_name", this.Name));
            query.Parameters.Add(new SqlParameter("@indataset_url", this.URL));
            query.Parameters.Add(new SqlParameter("@inbrief_description", this.BriefDescription));
            query.Parameters.Add(new SqlParameter("@indataset_description", this.Description));
            query.Parameters.Add(new SqlParameter("@inacquisition_description", this.AcquisitionDescription));
            query.Parameters.Add(new SqlParameter("@inprocessing_description", this.ProcessingDescription));
            if (this.ParentProject != null)
            {
                query.Parameters.Add(new SqlParameter("@inproject_id", this.ParentProject.ID));
            }
            query.Parameters.Add(new SqlParameter("@inversion", this.Version));
            query.Parameters.Add(new SqlParameter("@insql_table_name", this.TableName));
            query.Parameters.Add(new SqlParameter("@ineditable", this.IsEditable));
            if (!double.IsNaN(this.MinLat))
            {
                query.Parameters.Add(new SqlParameter("@inmin_lat", this.MinLat));
            }
            if (!double.IsNaN(this.MinLon))
            {
                query.Parameters.Add(new SqlParameter("@inmin_lon", this.MinLon));
            }
            if (!double.IsNaN(this.MaxLat))
            {
                query.Parameters.Add(new SqlParameter("@inmax_lat", this.MaxLat));
            }
            if (!double.IsNaN(this.MaxLon))
            {
                query.Parameters.Add(new SqlParameter("@inmax_lon", this.MaxLon));
            }
            query.ExecuteScalar();
            foreach (Field f in this.Header)
            {
                if (f.ID == Guid.Empty)
                {
                    f.ID = Guid.NewGuid();
                }
            }
            foreach (Field f in this.Header)
            {
                if (f.Owner == null)
                {
                    f.Owner = base.Owner;
                }
                f.Save(base.ID, conn);
            }
            base.Save(conn);
        }

        public DataTable SaveTemporaryData(SqlConnection conn, List<string> rows, Dictionary<string, int> column_map, Guid myId, DataTable dt, char[] delim)
        {
            DateTime starttime = DateTime.Now;
            int row_offset = 0;
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60,
                CommandText = "sp_GetMaxTemporaryRowNumber"
            };
            query.Parameters.Add(new SqlParameter("@inID", myId));
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("MaxRowNumber")))
                {
                    row_offset = int.Parse(reader["MaxRowNumber"].ToString()) + 1;
                }
            }
            reader.Close();
            if (dt == null)
            {
                dt = new DataTable("tmpdata");
                DataColumn sessionID = new DataColumn {
                    DataType = typeof(Guid),
                    ColumnName = "SessionID"
                };
                dt.Columns.Add(sessionID);
                DataColumn rowNumber = new DataColumn {
                    DataType = Type.GetType("System.Int32"),
                    ColumnName = "RowNumber"
                };
                dt.Columns.Add(rowNumber);
                DataColumn data = new DataColumn {
                    DataType = Type.GetType("System.String"),
                    ColumnName = "Data"
                };
                dt.Columns.Add(data);
                DataColumn sourcecolumnname = new DataColumn {
                    DataType = Type.GetType("System.String"),
                    ColumnName = "SourceColumnName"
                };
                dt.Columns.Add(sourcecolumnname);
            }
            int col = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.IsNullOrEmpty(rows[i]))
                {
                    continue;
                }
                string[] tokens = rows[i].Split(delim);
                bool valid = false;
                foreach (Field f in this.Header)
                {
                    if (!string.IsNullOrEmpty(tokens[column_map[f.SourceColumnName]].Trim()))
                    {
                        valid = true;
                        break;
                    }
                }
                if (valid)
                {
                    foreach (Field f in this.Header)
                    {
                        col = column_map[f.SourceColumnName];
                        DataRow dr = dt.NewRow();
                        dr["SessionID"] = myId;
                        dr["RowNumber"] = row_offset + i;
                        dr["Data"] = tokens[col];
                        dr["SourceColumnName"] = f.SourceColumnName;
                        dt.Rows.Add(dr);
                    }
                }
            }
            dt.AcceptChanges();
            return dt;
        }

        public void UpdateBounds(SqlConnection conn)
        {
            string lat_field = string.Empty;
            string lon_field = string.Empty;
            Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
            Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");
            foreach (Field f in this.Header)
            {
                if (f.FieldMetric.ID == lat_guid)
                {
                    lat_field = f.SQLColumnName;
                }
                if (f.FieldMetric.ID == lon_guid)
                {
                    lon_field = f.SQLColumnName;
                }
            }
            if (!string.IsNullOrEmpty(lat_field) && !string.IsNullOrEmpty(lon_field))
            {
                SqlConnection dataconn = Utils.ConnectToDatabase(this.ParentProject.database_name);
                SqlCommand query = new SqlCommand {
                    Connection = dataconn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text
                };
                string cmd = "SELECT MIN(" + lat_field + ") AS MinLatitude, MIN(" + lon_field + ") AS MinLongitude, MAX(" + lat_field + ") AS MaxLatitude, MAX(" + lon_field + ") AS MaxLongitude FROM " + this.ParentProject.database_name + ".dbo." + this.TableName;
                query.CommandText = cmd;
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        this.MinLat = double.Parse(reader[0].ToString());
                        this.MinLon = double.Parse(reader[1].ToString());
                        this.MaxLat = double.Parse(reader[2].ToString());
                        this.MaxLon = double.Parse(reader[3].ToString());
                    }
                }
                reader.Close();
                query = new SqlCommand {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_UpdateDatasetBounds"
                };
                query.Parameters.Add(new SqlParameter("@inDatasetID", base.ID));
                query.Parameters.Add(new SqlParameter("@inmin_lat", this.MinLat));
                query.Parameters.Add(new SqlParameter("@inmin_lon", this.MinLon));
                query.Parameters.Add(new SqlParameter("@inmax_lat", this.MaxLat));
                query.Parameters.Add(new SqlParameter("@inmax_lon", this.MaxLon));
                query.ExecuteNonQuery();
                dataconn.Close();
            }
        }

        public bool IsDefined
        {
            get
            {
                foreach (Field f in this.Header)
                {
                    if (f.FieldMetric == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}

