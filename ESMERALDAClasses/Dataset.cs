using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{
    public class Dataset : QuerySet
    {
        public string URL;
        public string Description;
        public string BriefDescription;
        public string AcquisitionDescription;
        public string ProcessingDescription;
        public int Version;
        public bool IsEditable;
        public double MinLat;
        public double MinLon;
        public double MaxLat;
        public double MaxLon;

        public override string GetMetadata()
        {
            string ret = string.Empty;
            ret = "<dataset>";
            ret += "<dataset_name>" + Name + "</dataset_name>";
            ret += "<brief_description>" + BriefDescription + "</brief_description>";
            ret += "<description>" + Description + "</description>";
            ret += "<dataset_url>" + URL + "</dataset_url>";
            ret += "<acquisition_description>" + AcquisitionDescription + "</acquisition_description>";
            ret += "<processing_description>" + ProcessingDescription + "</processing_description>";
            ret += "<version>" + Version.ToString() + "</version>";
            ret += "<fields>";
            foreach (QueryField f in Header)
            {
                ret += f.GetMetadata();
            }
            ret += "</fields>";
            ret += "</dataset>";
            return ret;
        }

        public Dataset()
            : base()
        {
            Header = new List<QueryField>();
            Name = string.Empty;
            URL = string.Empty;
            Description = string.Empty;
            BriefDescription = string.Empty;
            AcquisitionDescription = string.Empty;
            ProcessingDescription = string.Empty;
            Version = -1;
            IsEditable = true;
            MinLat = double.NaN;
            MinLon = double.NaN;
            MaxLat = double.NaN;
            MaxLon = double.NaN;
        }

        public bool IsDefined
        {
            get
            {
                foreach (Field f in Header)
                {
                    if (f.FieldMetric == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override void Load(SqlConnection conn, Guid inID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            ID = inID;
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_LoadDataset";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", inID));
            SqlDataReader reader = query.ExecuteReader();
            Guid enteredbyid = Guid.Empty;
            Guid projectid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("dataset_name")))
                        Name = reader["dataset_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("brief_description")))
                        BriefDescription = reader["brief_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("dataset_description")))
                        Description = reader["dataset_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("acquisition_description")))
                        AcquisitionDescription = reader["acquisition_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("processing_description")))
                        ProcessingDescription = reader["processing_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("version")))
                        Version = int.Parse(reader["version"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                        projectid = new Guid(reader["project_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("sql_table_name")))
                        SQLName = reader["sql_table_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("editable")))
                        IsEditable = bool.Parse(reader["editable"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                        MinLon = double.Parse(reader["min_lon"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("min_lat")))
                        MinLat = double.Parse(reader["min_lat"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("max_lon")))
                        MaxLon = double.Parse(reader["max_lon"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("max_lat")))
                        MaxLat = double.Parse(reader["max_lat"].ToString());
                }
            }
            reader.Close();

            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_LoadFields";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inDatasetID", inID));
            reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    Field newfield = new Field();
                    if (!reader.IsDBNull(reader.GetOrdinal("field_name")))
                        newfield.Name = reader["field_name"].ToString();
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
                        newfield.SourceColumnName = reader["source_column_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("db_type")))
                    {
                        newfield.DBType = (Field.FieldType)int.Parse(reader["db_type"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("field_id")))
                        newfield.ID = new Guid(reader["field_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("sql_column_name")))
                        newfield.SQLColumnName = reader["sql_column_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("subfield_id")))
                        newfield.SubfieldID = new Guid(reader["subfield_id"].ToString());
                    newfield.Parent = this;
                    Header.Add(newfield);
                }
            }
            reader.Close();

            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_LoadFieldsAdditionalMetadata";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inDatasetID", inID));
            reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    Guid fieldid = new Guid(reader["field_id"].ToString());
                    foreach (Field f in Header)
                    {
                        if (f.ID == fieldid)
                        {
                            if (f.Metadata == null)
                                f.Metadata = new Field_Metadata();

                            if (!reader.IsDBNull(reader.GetOrdinal("observation_methodology")))
                                f.Metadata.observation_methodology = reader["observation_methodology"].ToString();
                            if (!reader.IsDBNull(reader.GetOrdinal("instrument")))
                                f.Metadata.instrument = reader["instrument"].ToString();
                            if (!reader.IsDBNull(reader.GetOrdinal("analysis_methodology")))
                                f.Metadata.analysis_methodology = reader["analysis_methodology"].ToString();
                            if (!reader.IsDBNull(reader.GetOrdinal("processing_methodology")))
                                f.Metadata.processing_methodology = reader["processing_methodology"].ToString();
                            if (!reader.IsDBNull(reader.GetOrdinal("citations")))
                                f.Metadata.citations = reader["citations"].ToString();
                            if (!reader.IsDBNull(reader.GetOrdinal("description")))
                                f.Metadata.description = reader["description"].ToString();
                            break;
                        }
                    }
                }
            }
            reader.Close();

            base.Load(conn);        
            foreach (Field f in Header)
            {
                f.Load(conn);
                if (f.SubfieldID != Guid.Empty)
                {
                    foreach (Field f2 in Header)
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
                ParentProject = new Project();
                ParentProject.Load(conn, projectid);
            }            
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (ID == Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(SQLName))
            {
                SQLName = Utils.CreateDBName(Name);
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteDataset";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@indataset_id", ID));
            query.Parameters.Add(new SqlParameter("@indataset_name", Name));
            query.Parameters.Add(new SqlParameter("@indataset_url", URL));
            query.Parameters.Add(new SqlParameter("@inbrief_description", BriefDescription));
            query.Parameters.Add(new SqlParameter("@indataset_description", Description));
            query.Parameters.Add(new SqlParameter("@inacquisition_description", AcquisitionDescription));
            query.Parameters.Add(new SqlParameter("@inprocessing_description", ProcessingDescription));
            if (ParentProject != null)
            {
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentProject.ID));
            }
            query.Parameters.Add(new SqlParameter("@inversion", Version));
            query.Parameters.Add(new SqlParameter("@insql_table_name", SQLName));
            query.Parameters.Add(new SqlParameter("@ineditable", IsEditable));
            if (!double.IsNaN(MinLat))
                query.Parameters.Add(new SqlParameter("@inmin_lat", MinLat));
            if (!double.IsNaN(MinLon))
                query.Parameters.Add(new SqlParameter("@inmin_lon", MinLon));
            if (!double.IsNaN(MaxLat))
                query.Parameters.Add(new SqlParameter("@inmax_lat", MaxLat));
            if (!double.IsNaN(MaxLon))
                query.Parameters.Add(new SqlParameter("@inmax_lon", MaxLon));
            query.ExecuteScalar();

            foreach (Field f in Header)
            {
                if (f.ID == Guid.Empty)
                    f.ID = Guid.NewGuid();
            }
            foreach (Field f in Header)
            {
                if (f.Owner == null)
                    f.Owner = Owner;
                f.Save(ID, conn);
            }

            base.Save(conn);
        }

        public DataTable SaveTemporaryData(SqlConnection conn, List<string> rows, Dictionary<string, int> column_map, Guid myId, DataTable dt, char[] delim)
        {
            DateTime starttime = DateTime.Now;

            int row_offset = 0;
            SqlCommand query = new SqlCommand();
            query.Connection = conn;
            query.CommandType = CommandType.StoredProcedure;
            query.CommandTimeout = 60;
            query.CommandText = "sp_GetMaxTemporaryRowNumber";
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

                DataColumn sessionID = new DataColumn();
                sessionID.DataType = typeof(Guid);
                sessionID.ColumnName = "SessionID";
                dt.Columns.Add(sessionID);

                DataColumn rowNumber = new DataColumn();
                rowNumber.DataType = System.Type.GetType("System.Int32");
                rowNumber.ColumnName = "RowNumber";
                dt.Columns.Add(rowNumber);

                DataColumn data = new DataColumn();
                data.DataType = System.Type.GetType("System.String");
                data.ColumnName = "Data";
                dt.Columns.Add(data);

                DataColumn sourcecolumnname = new DataColumn();
                sourcecolumnname.DataType = System.Type.GetType("System.String");
                sourcecolumnname.ColumnName = "SourceColumnName";
                dt.Columns.Add(sourcecolumnname);
            }

            int col = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (string.IsNullOrEmpty(rows[i]))
                    continue;
                string[] tokens = rows[i].Split(delim);
                bool valid = false;
                foreach (Field f in Header)
                {
                    if (!string.IsNullOrEmpty(tokens[column_map[f.SourceColumnName]].Trim()))
                    {
                        valid = true;
                        break;
                    }
                }
                if (!valid)
                    continue;
                foreach (Field f in Header)
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
            dt.AcceptChanges();
            return dt;
        }

        public DataTable MoveExistingDataToTemp(SqlConnection conn, Guid myId)
        {
            if (ID == Guid.Empty || string.IsNullOrEmpty(SQLName))
                return null;
            DataTable dt = new DataTable("tmpdata");

            DataColumn sessionID = new DataColumn();
            sessionID.DataType = typeof(Guid);
            sessionID.ColumnName = "SessionID";
            dt.Columns.Add(sessionID);

            DataColumn rowNumber = new DataColumn();
            rowNumber.DataType = System.Type.GetType("System.Int32");
            rowNumber.ColumnName = "RowNumber";
            dt.Columns.Add(rowNumber);

            DataColumn data = new DataColumn();
            data.DataType = System.Type.GetType("System.String");
            data.ColumnName = "Data";
            dt.Columns.Add(data);

            DataColumn sourcecolumnname = new DataColumn();
            sourcecolumnname.DataType = System.Type.GetType("System.String");
            sourcecolumnname.ColumnName = "SourceColumnName";
            dt.Columns.Add(sourcecolumnname);

            string dbname = ParentProject.database_name;
            string tablename = SQLName;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            int row = 0;
            string cmd = "SELECT * FROM " + SQLName;
            SqlCommand query = new SqlCommand();
            query.Connection = dataconn;
            query.CommandTimeout = 60;
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < Header.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["SessionID"] = myId;
                    dr["RowNumber"] = row;
                    if (!Header[i].IsSubfield)
                    {
                        if (Header[i].DBType == Field.FieldType.DateTime)
                        {
                            if (((Field)Header[i]).Subfield != null)
                            {
                                dr["Data"] = DateTime.Parse(reader[Header[i].SQLColumnName].ToString()).Date.ToString();
                            }
                            else
                            {
                                dr["Data"] = reader[Header[i].SQLColumnName].ToString();
                            }
                        }
                        else
                        {
                            dr["Data"] = reader[Header[i].SQLColumnName].ToString();
                        }
                    }
                    dr["SourceColumnName"] = ((Field)Header[i]).SourceColumnName;
                    dt.Rows.Add(dr);
                    if (((Field)Header[i]).Subfield != null)
                    {
                        DataRow dr2 = dt.NewRow();
                        dr2["Data"] = DateTime.Parse(reader[Header[i].SQLColumnName].ToString()).TimeOfDay.ToString();
                        dr2["SessionID"] = myId;
                        dr2["RowNumber"] = row;
                        dr2["SourceColumnName"] = ((Field)Header[i]).Subfield.SourceColumnName;
                        dt.Rows.Add(dr2);
                    }
                }
                row += 1;
            }
            reader.Close();
            dataconn.Close();
            dt.AcceptChanges();
            return dt;
            /*SqlBulkCopy bulkCopy = new SqlBulkCopy(conn);
            bulkCopy.DestinationTableName = "dbo.temporary_storage";

            try
            {
                // Write from the source to the destination.
                bulkCopy.WriteToServer(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }*/
        }

        public void DeleteExistingData()
        {
            string dbname = ParentProject.database_name;
            string tablename = SQLName;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            string cmd = "TRUNCATE TABLE " + SQLName;
            SqlCommand query = new SqlCommand();
            query.Connection = dataconn;
            query.CommandTimeout = 60;
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            query.ExecuteNonQuery();
            dataconn.Close();
        }

        public void UpdateBounds(SqlConnection conn)
        {
            string lat_field = string.Empty;
            string lon_field = string.Empty;

            Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
            Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");

            foreach (Field f in Header)
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
                string dbname = ParentProject.database_name;
                SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
                SqlCommand query = new SqlCommand();
                query.Connection = dataconn;
                query.CommandTimeout = 60;
                query.CommandType = CommandType.Text;
                string cmd = "SELECT MIN(" + lat_field + ") AS MinLatitude, MIN(" + lon_field + ") AS MinLongitude, MAX(" + lat_field + ") AS MaxLatitude, MAX(" + lon_field +

") AS MaxLongitude FROM " + ParentProject.database_name + ".dbo." + SQLName;
                query.CommandText = cmd;
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        MinLat = double.Parse(reader[0].ToString());
                        MinLon = double.Parse(reader[1].ToString());
                        MaxLat = double.Parse(reader[2].ToString());
                        MaxLon = double.Parse(reader[3].ToString());
                    }
                }
                reader.Close();

                query = new SqlCommand();
                query.Connection = conn;
                query.CommandTimeout = 60;
                query.CommandType = CommandType.StoredProcedure;
                query.CommandText = "sp_UpdateDatasetBounds";
                query.Parameters.Add(new SqlParameter("@inDatasetID", ID));
                query.Parameters.Add(new SqlParameter("@inmin_lat", MinLat));
                query.Parameters.Add(new SqlParameter("@inmin_lon", MinLon));
                query.Parameters.Add(new SqlParameter("@inmax_lat", MaxLat));
                query.Parameters.Add(new SqlParameter("@inmax_lon", MaxLon));
                query.ExecuteNonQuery();

                dataconn.Close();
            }
        }

        public DataTable BuildDataTable(DataTable data, int num_rows)
        {
            DataTable ret = new DataTable(Name);
            for (int i = 0; i < Header.Count; i++)
            {
                Field f = (Field)Header[i];
                if (f.IsSubfield)
                    continue;
                if (string.IsNullOrEmpty(f.SQLColumnName))
                {
                    f.SQLColumnName = Utils.CreateDBName(f.Name);
                }
                if (f.DBType == Field.FieldType.DateTime)
                {
                    DataColumn newfield = new DataColumn();
                    newfield.DataType = typeof(DateTime);
                    newfield.ColumnName = f.SQLColumnName;
                    ret.Columns.Add(newfield);
                }
                else if (f.DBType == Field.FieldType.Time)
                {
                    DataColumn newfield = new DataColumn();
                    newfield.DataType = typeof(TimeSpan);
                    newfield.ColumnName = f.SQLColumnName;
                    ret.Columns.Add(newfield);
                }
                else if (f.DBType == Field.FieldType.Decimal)
                {
                    DataColumn newfield = new DataColumn();
                    newfield.DataType = typeof(double);
                    newfield.ColumnName = f.SQLColumnName;
                    ret.Columns.Add(newfield);
                }
                else if (f.DBType == Field.FieldType.Integer)
                {
                    DataColumn newfield = new DataColumn();
                    newfield.DataType = typeof(int);
                    newfield.ColumnName = f.SQLColumnName;
                    ret.Columns.Add(newfield);
                }
                else if (f.DBType == Field.FieldType.Text)
                {
                    DataColumn newfield = new DataColumn();
                    newfield.DataType = typeof(string);
                    newfield.ColumnName = f.SQLColumnName;
                    ret.Columns.Add(newfield);
                }
            }
            int curr_row = -1;
            int row = 0;
            DataRow newrow = null;
            string datum = string.Empty;
            string sourcecol = string.Empty;
            int curr_row_start = -1;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                if (i % 1000 == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Processing " + i.ToString() + " of " + data.Rows.Count.ToString());
                }
                row = (int)data.Rows[i]["RowNumber"];
                if (row != curr_row)
                {
                    if (newrow != null)
                        ret.Rows.Add(newrow);
                    newrow = ret.NewRow();
                    curr_row = row;
                    curr_row_start = i;
                    if (num_rows > 0 && curr_row > num_rows)
                        break;
                }
                sourcecol = (string)data.Rows[i]["SourceColumnName"];
                DateTime dt_val;
                TimeSpan ts_val;
                double d_val;
                int i_val;
                Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
                Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");
                foreach (Field f in Header)
                {
                    if (f.SourceColumnName == sourcecol)
                    {
                        if (f.IsSubfield)
                            break;

                        datum = (string)data.Rows[i]["Data"];
                        if (f.DBType != Field.FieldType.Text && string.IsNullOrEmpty(datum))
                        {
                            newrow[f.SQLColumnName] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                if (f.DBType == Field.FieldType.DateTime || f.DBType == Field.FieldType.Time)
                                {
                                    if (f.Subfield == null)
                                    {
                                        if (f.DBType == Field.FieldType.DateTime)
                                        {
                                            if (f.FieldMetric.Name.IndexOf("Year Day") == 0)
                                            {
                                                DateTime startdate = DateTime.Parse(f.FieldMetric.Abbrev);
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
                                    }
                                    else
                                    {
                                        int j = curr_row_start;
                                        string datum2 = string.Empty;
                                        TimeSpan timecomponent = TimeSpan.Zero;
                                        DateTime datecomponent = DateTime.MinValue;

                                        while (j < data.Rows.Count)
                                        {
                                            if ((int)data.Rows[j]["RowNumber"] != row)
                                            {
                                                break;
                                            }
                                            if ((string)data.Rows[j]["SourceColumnName"] == f.Subfield.SourceColumnName)
                                            {
                                                datum2 = (string)data.Rows[j]["Data"];
                                                break;
                                            }
                                            j += 1;
                                        }
                                        if (f.DBType == Field.FieldType.DateTime)
                                        {
                                            if (f.FieldMetric.Name.IndexOf("Year Day") == 0)
                                            {
                                                DateTime startdate = DateTime.Parse(f.FieldMetric.Abbrev);
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
                                            double tmp = 0;
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
                                                DateTime startdate = DateTime.Parse(f.FieldMetric.Abbrev);
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
                                            double tmp = 0;
                                            if (double.TryParse(datum2, out tmp))
                                            {
                                                timecomponent = TimeSpan.FromSeconds(tmp);
                                            }
                                            else
                                            {
                                                TimeSpan.TryParse(datum2, out timecomponent);
                                            }
                                        }
                                        if (datecomponent == DateTime.MinValue && timecomponent == TimeSpan.Zero)
                                        {
                                            newrow[f.SQLColumnName] = DBNull.Value;
                                        }
                                        else
                                        {
                                            newrow[f.SQLColumnName] = Utils.ConvertDateToUTC(datecomponent + timecomponent, f.FieldMetric.CurrentTimeZone);
                                        }
                                    }
                                }
                                else if (f.DBType == Field.FieldType.Decimal)
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
                                                if (double.IsNaN(MinLat))
                                                {
                                                    MinLat = d_val;
                                                    MaxLat = d_val;
                                                }
                                                else if (d_val < MinLat)
                                                {
                                                    MinLat = d_val;
                                                }
                                                else if (d_val > MaxLat)
                                                {
                                                    MaxLat = d_val;
                                                }
                                            }
                                            if (f.FieldMetric.ID == lon_guid)
                                            {
                                                if (double.IsNaN(MinLon))
                                                {
                                                    MinLon = d_val;
                                                    MaxLon = d_val;
                                                }
                                                else if (d_val < MinLon)
                                                {
                                                    MinLon = d_val;
                                                }
                                                else if (d_val > MaxLon)
                                                {
                                                    MaxLon = d_val;
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
                                System.Diagnostics.Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                            }
                        }
                        break;
                    }
                }
            }
            if (newrow != null)
                ret.Rows.Add(newrow);
            return ret;
        }
    }
}
