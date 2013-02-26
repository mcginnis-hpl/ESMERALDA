using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ESMERALDAClasses
{
    public class Dataset : QuerySet
    {
        public int Version;
        public bool IsEditable;

        public override string GetMetadata()
        {
            string ret = string.Empty;            
            return ret;
        }

        public Dataset()
            : base()
        {
            Header = new List<QueryField>();
            Version = -1;
            IsEditable = true;
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

        public string BuildMetadata(Project parentProject, Program parentProgram)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            doc.AppendChild(node);
            return doc.InnerXml;
        }

        public override void Load(SqlConnection conn, Guid inID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            ID = inID;
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadDataset";
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
                    if (!reader.IsDBNull(reader.GetOrdinal("version")))
                        Version = int.Parse(reader["version"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                        projectid = new Guid(reader["project_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("sql_table_name")))
                        SQLName = reader["sql_table_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("editable")))
                        IsEditable = bool.Parse(reader["editable"].ToString());
                }
            }
            reader.Close();

            query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_LoadFields";
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
                SQLName = Utils.CreateDBName(GetMetadataValue("title"));
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_ESMERALDA_WriteDataset";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@indataset_id", ID));
            if (ParentProject != null)
            {
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentProject.ID));
            }
            query.Parameters.Add(new SqlParameter("@inversion", Version));
            query.Parameters.Add(new SqlParameter("@insql_table_name", SQLName));
            query.Parameters.Add(new SqlParameter("@ineditable", IsEditable));
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
            query.CommandText = "sp_ESMERALDA_GetMaxTemporaryRowNumber";
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
                string cmd = "SELECT MIN(" + lat_field + ") AS MinLatitude, MIN(" + lon_field + ") AS MinLongitude, MAX(" + lat_field + ") AS MaxLatitude, MAX(" + lon_field + ") AS MaxLongitude FROM " + ParentProject.database_name + ".dbo." + SQLName;
                query.CommandText = cmd;
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        SetMetadataValue("southbc", reader[0].ToString());
                        SetMetadataValue("westbc", reader[1].ToString());
                        SetMetadataValue("northbc", reader[2].ToString());
                        SetMetadataValue("eastbc", reader[3].ToString());
                    }
                }
                reader.Close();

                Save(conn);
                dataconn.Close();
            }
        }

        public void GetFieldMinMax(Field inField, ref object outMin, ref object outMax)
        {
            string dbname = ParentProject.database_name;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            SqlCommand query = new SqlCommand();
            query.Connection = dataconn;
            query.CommandTimeout = 60;
            query.CommandType = CommandType.Text;
            string cmd = "SELECT MIN(" + inField.SQLColumnName + ") AS MinValue, MAX(" + inField.SQLColumnName + ") AS MaxValue FROM " + ParentProject.database_name + ".dbo." + SQLName;
            query.CommandText = cmd;
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("MinValue")))
                {
                    if (inField.DBType == Field.FieldType.DateTime)
                    {
                        outMin = DateTime.Parse(reader["MinValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Decimal)
                    {
                        outMin = double.Parse(reader["MinValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Integer)
                    {
                        outMin = int.Parse(reader["MinValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Text)
                    {
                        outMin = reader["MinValue"].ToString();
                    }
                }
                if (!reader.IsDBNull(reader.GetOrdinal("MaxValue")))
                {
                    if (inField.DBType == Field.FieldType.DateTime)
                    {
                        outMin = DateTime.Parse(reader["MaxValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Decimal)
                    {
                        outMin = double.Parse(reader["MaxValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Integer)
                    {
                        outMin = int.Parse(reader["MaxValue"].ToString());
                    }
                    else if (inField.DBType == Field.FieldType.Text)
                    {
                        outMin = reader["MaxValue"].ToString();
                    }
                }
            }
            reader.Close();
            dataconn.Close();
        }

        public DataTable BuildDataTable(DataTable data, int num_rows)
        {
            DataTable ret = new DataTable(GetMetadataValue("title"));
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

        public void AutopopulateFields(List<Metric> metrics)
        {
            SqlConnection conn = null;
            try
            {
                conn = Utils.ConnectToDatabaseReadOnly(ParentProject.database_name);
                string cmd = "SELECT TOP(1) * FROM " + SQLName;
                SqlCommand query = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandText = cmd
                };
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string col_name = reader.GetName(i);
                        string col_type = reader.GetDataTypeName(i);
                        Field f = new Field();
                        f.SourceColumnName = col_name;
                        f.SQLColumnName = col_name;
                        f.Name = col_name;
                        Guid field_guid = Guid.Empty;
                        if (col_type == "int")
                        {
                            f.DBType = Field.FieldType.Integer;
                            field_guid = Metric.GenericInt;
                        }
                        else if (col_type == "float" || col_type == "numeric" || col_type == "real")
                        {
                            f.DBType = Field.FieldType.Decimal;
                            field_guid = Metric.GenericDecimal;
                        }
                        else if (col_type == "varchar" || col_type == "nvarchar" || col_type == "uniqueidentifier")
                        {
                            f.DBType = Field.FieldType.Text;
                            field_guid = Metric.GenericText;
                        }
                        else if (col_type == "datetime")
                        {
                            f.DBType = Field.FieldType.DateTime;
                            field_guid = Metric.GenericDatetime;
                        }
                        if (field_guid != Guid.Empty)
                        {
                            foreach (Metric m in metrics)
                            {
                                if (m.ID == field_guid)
                                {
                                    f.FieldMetric = m;
                                    break;
                                }
                            }
                        }
                        Header.Add(f);
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                if(conn != null)
                    conn.Close();
            }
        }
    }
}
