using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using LumenWorks.Framework.IO.Csv;

namespace ESMERALDAClasses
{
    /*
     * Metadata Values
     * title
     * purpose
     * abstract
     * procdesc
     * acqdesc
     * url
     * 
     * */

    public class Dataset : QuerySet
    {
        public int Version;
        public bool IsEditable;
        public bool IsDirty;

        public override string GetMetadata(MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == MetadataFormat.XML)
            {
                ret = "<dataset>";
                /*if (Owner != null)
                {
                    ret += "<owner>" + Owner.GetMetadata(format) + "</owner>";
                }*/
                if (Relationships.Count > 0)
                {
                    foreach (PersonRelationship pr in Relationships)
                    {
                        ret += pr.GetMetadata(format);
                    }
                }
                foreach (string s in Metadata.Keys)
                {
                    System.Collections.Generic.List<string> meta = Metadata[s];
                    for (int i = 0; i < meta.Count; i++)
                    {
                        ret += "<" + s + ">" + meta[i] + "</" + s + ">";
                    }
                }
                ret += "<fields>";
                foreach (Field f in Header)
                {
                    ret += f.GetMetadata(format);
                }
                ret += "</fields>";
                ret += "<version>" + Version.ToString() + "</version>";
                ret += "</dataset>";
            }
            else if (format == MetadataFormat.BCODMO)
            {
                ret += "Originating PI name and contact information:" + Environment.NewLine;
                string val = string.Empty;
                foreach (PersonRelationship r in Relationships)
                {
                    if (r.relationship == "Principal Investigator")
                    {
                        val = r.person.GetMetadata(format);
                    }
                    if (!string.IsNullOrEmpty(val))
                        val += Environment.NewLine;
                    ret += val;
                }
                ret += Environment.NewLine;
                ret += "Contact name and contact information:  " + Environment.NewLine;
                if (Owner != null)
                {
                    ret += Owner.GetMetadata(format);
                }
                ret += Environment.NewLine;
                ret += "Dataset Name: " + GetMetadataValue("title") + Environment.NewLine;
                ret += "Dataset Description: " + GetMetadataValue("purpose") + Environment.NewLine;
                if (ParentContainer != null)
                {
                    ret += "Project: " + ParentContainer.GetMetadataValue("title") + Environment.NewLine;
                }
                val = string.Empty;
                string bounds = string.Empty;
                val = GetMetadataValue("southbc");
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
                    ret += "Location: " + Environment.NewLine;
                    ret += bounds;
                }
                ret += Environment.NewLine;
                ret += "Parameter names, definitions and units:" + Environment.NewLine;
                foreach (Field f in Header)
                {
                    ret += f.GetMetadata(format);
                }
                ret += Environment.NewLine;
                ret += "Sampling and Analytical Methodology:" + Environment.NewLine;
                val = GetMetadataValue("acqdesc");
                if (!string.IsNullOrEmpty(val))
                {
                    ret += val + Environment.NewLine;
                }
                ret += Environment.NewLine;
                ret += "Data Processing:" + Environment.NewLine;
                val = GetMetadataValue("procdesc");
                if (!string.IsNullOrEmpty(val))
                {
                    ret += val + Environment.NewLine;
                }
            }
            else if (format == MetadataFormat.FGDC)
            {
                PersonRelationship pi = null;
                foreach (PersonRelationship r in Relationships)
                {
                    if (r.relationship == "Principal Investigator")
                    {
                        pi = r;
                    }
                }
                ret += "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";    
                ret += "<metadata>";
                ret += "<idinfo>";
                ret += "<citation>";
                ret += "<citeinfo>";
                if(pi != null)
                {
                    ret += "<origin>" + pi.GetMetadataValue("cntorg") + "</origin>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("pubdate")))
                {
                    ret += "<pubdate>" + GetMetadataValue("pubdate") + "</pubdate>";
                }
                ret += "<title>" + GetMetadataValue("title") + "</title>";
                //<geoform>Digital vector data</geoform>
                ret += "</citeinfo>";
                ret += "</citation>";
                ret += "<descript>";
                if(!string.IsNullOrEmpty(GetMetadataValue("abstract")))
                {
                    ret += "<abstract>" + GetMetadataValue("abstract") + "</abstract>";
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("purpose")))
                {
                    ret += "<purpose>" + GetMetadataValue("purpose") + "</purpose>";
                }
                ret += "</descript>";
                if (ParentContainer != null)
                {
                    string val = ParentContainer.GetMetadataValue("startdate");
                    DateTime startdate = DateTime.MinValue;
                    DateTime enddate = DateTime.MinValue;
                    if(!string.IsNullOrEmpty(val))
                    {                        
                        startdate = DateTime.Parse(val);
                    }
                    val = ParentContainer.GetMetadataValue("enddate");
                    if(!string.IsNullOrEmpty(val))
                    {
                        enddate = DateTime.Parse(val);
                    }
                    if(startdate != DateTime.MinValue)
                    {
                        if(enddate != DateTime.MinValue)
                        {
                            ret += "<timeperd><timeinfo><rngdates>";
                            ret += "<begdate>" + startdate.ToString("YYYYmmdd") + "</begdate>";
                            ret += "<enddate>" + enddate.ToString("YYYYmmdd") + "</enddate>";
                            ret += "</rngdates></timeinfo></timeperd>";
                        }
                        else
                        {
                            ret += "<timeperd><timeinfo><sngdate>";
                            ret += "<caldate>" + startdate.ToString("YYYYmmdd") + "</caldate>";                            
                            ret += "</sngdates></timeinfo></timeperd>";
                        }
                    }
                    else if(enddate != DateTime.MinValue)
                    {
                        ret += "<timeperd><timeinfo><sngdate>";
                        ret += "<caldate>" + enddate.ToString("YYYYmmdd") + "</caldate>";                            
                        ret += "</sngdates></timeinfo></timeperd>";
                    }                                                        
                }
                if(!string.IsNullOrEmpty(GetMetadataValue("status")))
                {
                    ret += "<status><progress>" + GetMetadataValue("status") + "</progress></status>";
                }                
                string westbc = GetMetadataValue("westbc");
                string eastbc = GetMetadataValue("eastbc");
                string northbc = GetMetadataValue("northbc");
                string southbc = GetMetadataValue("southbc");
                if (!string.IsNullOrEmpty(westbc) || !string.IsNullOrEmpty(eastbc) || !string.IsNullOrEmpty(northbc) || !string.IsNullOrEmpty(southbc))
                {
                    ret += "<spdom><bounding>";
                    if(!string.IsNullOrEmpty(westbc))
                        ret += "<westbc>" + westbc + "</westbc>";
                    if (!string.IsNullOrEmpty(eastbc))
                        ret += "<eastbc>" + eastbc + "</eastbc>";
                    if (!string.IsNullOrEmpty(northbc))
                        ret += "<northbc>" + northbc + "</northbc>";
                    if (!string.IsNullOrEmpty(southbc))
                        ret += "<southbc>" + southbc + "</southbc>";
                    ret += "</bounding></spdom>";
                }
                List<string> keywords = GetMetadataValueArray("keyword");
                if(keywords.Count > 0)
                {
                    ret += "<theme>";
                    foreach (string kt in keywords)
                    {
                        ret += "<themekey>" + kt + "</themekey>";
                    }
                    ret += "</theme>";
                }
                ret += "</idinfo>";
                ret += "<ptcontac>";
                if (pi != null)
                {
                    ret += pi.GetMetadata(format);
                }
                ret += "</ptcontac>";
                if (Header.Count > 0)
                {
                    ret += "<eainfo><detailed>";
                    foreach (Field f in Header)
                    {
                        string val = f.GetMetadata(format);
                        if (!string.IsNullOrEmpty(val))
                        {
                            ret += val;
                        }
                    }
                    ret += "</detailed></eainfo>";
                }
                ret += "</metadata>";
            }
            
            return ret;
        }

        public Dataset()
            : base()
        {
            Header = new List<QueryField>();
            Version = -1;
            IsEditable = true;
            IsDirty = false;
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
            query.CommandText = "sp_ESMERALDA_LoadDataset";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", inID));
            SqlDataReader reader = query.ExecuteReader();
            Guid enteredbyid = Guid.Empty;
            Guid containerid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {                   
                    if (!reader.IsDBNull(reader.GetOrdinal("version")))
                        Version = int.Parse(reader["version"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                        containerid = new Guid(reader["project_id"].ToString());
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
            if (containerid != Guid.Empty)
            {
                ParentEntity = new Container();
                ParentEntity.Load(conn, containerid);
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
            if (ParentContainer != null)
            {
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentContainer.ID));
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
                f.Save(this, conn);
            }

            base.Save(conn);            
        }

        public DataTable SaveTemporaryData(CsvReader csv, DataTable dt)
        {
            DateTime starttime = DateTime.Now;

            if (dt == null || dt.Rows.Count == 0)
            {
                dt = new DataTable("tmpdata");

                foreach (Field f in Header)
                {
                    DataColumn data = new DataColumn();
                    data.DataType = System.Type.GetType("System.String");
                    data.ColumnName = f.SourceColumnName;
                    dt.Columns.Add(data);
                }
            }
            else
            {
                foreach (Field f in Header)
                {
                    if(!dt.Columns.Contains(f.SourceColumnName))
                    {
                        DataColumn data = new DataColumn();
                        data.DataType = System.Type.GetType("System.String");
                        data.ColumnName = f.SourceColumnName;
                        dt.Columns.Add(data);
                    }
                }
            }
            int fieldCount = csv.FieldCount;
            string[] headers = csv.GetFieldHeaders();
            while(csv.ReadNextRecord())
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < fieldCount; i++)
                {
                    dr[headers[i]] = csv[i];
                }
                dt.Rows.Add(dr);                
            }
            dt.AcceptChanges();
            return dt;
        }

        public DataTable MoveExistingDataToTemp(SqlConnection conn, Guid myId, bool doFull)
        {
            if (ID == Guid.Empty || string.IsNullOrEmpty(SQLName))
                return null;
            DataTable dt = new DataTable("tmpdata");

            foreach (Field f in Header)
            {
                DataColumn data = new DataColumn();
                data.DataType = System.Type.GetType("System.String");
                data.ColumnName = f.SourceColumnName;
                dt.Columns.Add(data);
            }
            string dbname = ParentContainer.database_name;
            string tablename = SQLName;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            int row = 0;
            string cmd = "SELECT";
            if (!doFull)
            {
                cmd += " TOP(200)";
            }
            cmd += " * FROM [" + SQLName + "]";
            SqlCommand query = new SqlCommand();
            query.Connection = dataconn;
            query.CommandTimeout = 60;
            query.CommandType = CommandType.Text;
            query.CommandText = cmd;
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < Header.Count; i++)
                {
                    Field f = (Field)Header[i];
                    if (f.IsSubfield)
                    {
                        continue;
                    }
                    if (f.Subfield == null)
                    {
                        dr[f.SourceColumnName] = reader[f.SQLColumnName].ToString();
                    }
                    else
                    {
                        DateTime tmp = DateTime.Parse(reader[f.SQLColumnName].ToString());
                        if (f.DBType == Field.FieldType.DateTime)
                        {
                            dr[f.SourceColumnName] = tmp.ToShortDateString();
                        }
                        else if (f.DBType == Field.FieldType.Time)
                        {
                            dr[f.SourceColumnName] = tmp.ToShortTimeString();
                        }
                        if (f.Subfield.DBType == Field.FieldType.Time)
                        {
                            dr[f.Subfield.SourceColumnName] = tmp.ToShortTimeString();
                        }
                        else if (f.Subfield.DBType == Field.FieldType.DateTime)
                        {
                            dr[f.Subfield.SourceColumnName] = tmp.ToShortDateString();
                        }
                    }
                }
                dt.Rows.Add(dr);
                row += 1;
            }
            reader.Close();
            dataconn.Close();
            dt.AcceptChanges();
            return dt;
        }

        public void DeleteExistingData()
        {
            string dbname = ParentContainer.database_name;
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
            string datetime_field = string.Empty;

            Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
            Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");

            foreach (Field f in Header)
            {
                if (f.FieldMetric == null)
                    continue;
                if (f.FieldMetric.ID == lat_guid)
                {
                    lat_field = f.SQLColumnName;
                }
                if (f.FieldMetric.ID == lon_guid)
                {
                    lon_field = f.SQLColumnName;
                }
                if (f.DBType == Field.FieldType.DateTime)
                {
                    datetime_field = f.SQLColumnName;
                }
            }
            if (!string.IsNullOrEmpty(lat_field) && !string.IsNullOrEmpty(lon_field))
            {
                string dbname = ParentContainer.database_name;
                SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
                SqlCommand query = new SqlCommand();
                query.Connection = dataconn;
                query.CommandTimeout = 3000;
                query.CommandType = CommandType.Text;
                string cmd = "SELECT MIN([" + lat_field + "]) AS MinLatitude, MIN([" + lon_field + "]) AS MinLongitude, MAX([" + lat_field + "]) AS MaxLatitude, MAX([" + lon_field + "]) AS MaxLongitude FROM [" + ParentContainer.database_name + "].dbo.[" + SQLName + "]";
                cmd += " WHERE [" + lat_field + "] <= 90 AND [" + lat_field + "] >= -90 AND [" + lon_field + "] >= -180 AND [" + lon_field + "] <= 180";
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
            if (!string.IsNullOrEmpty(datetime_field))
            {
                try
                {
                    string dbname = ParentContainer.database_name;
                    SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
                    SqlCommand query = new SqlCommand();
                    query.Connection = dataconn;
                    query.CommandTimeout = 60;
                    query.CommandType = CommandType.Text;
                    string cmd = "SELECT DISTINCT DATEADD(dd, 0, DATEDIFF(dd, 0, [" + datetime_field + "])) AS timestamp_day FROM [" + ParentContainer.database_name + "].dbo.[" + SQLName + "]";
                    query.CommandText = cmd;
                    SqlDataReader reader = query.ExecuteReader();
                    List<string> days = new List<string>();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            days.Add(reader[0].ToString());
                        }
                    }
                    reader.Close();
                    dataconn.Close();

                    cmd = "DELETE FROM entity_datetime_map WHERE entity_id='" + ID.ToString() + "';";
                    foreach (string s in days)
                    {
                        cmd += "INSERT INTO entity_datetime_map(entity_id, timestamp) VALUES ('" + ID.ToString() + "', '" + s + "');";
                    }
                    query = new SqlCommand();
                    query.Connection = conn;
                    query.CommandTimeout = 60;
                    query.CommandType = CommandType.Text;
                    query.CommandText = cmd;
                    query.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
            if (ParentContainer != null)
                ParentContainer.UpdateBounds(conn);
        }

        public void GetFieldMinMax(Field inField, ref object outMin, ref object outMax)
        {
            string dbname = ParentContainer.database_name;
            SqlConnection dataconn = Utils.ConnectToDatabase(dbname);
            SqlCommand query = new SqlCommand();
            query.Connection = dataconn;
            query.CommandTimeout = 60;
            query.CommandType = CommandType.Text;
            string cmd = "SELECT MIN(" + inField.SQLColumnName + ") AS MinValue, MAX(" + inField.SQLColumnName + ") AS MaxValue FROM " + ParentContainer.database_name + ".dbo." + SQLName;
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

        public DataTable BuildDataTable(DataTable data, int offset, int num_rows)
        {
            DataTable ret = new DataTable(GetMetadataValue("title"));
            int i = 0;
            for (i = 0; i < Header.Count; i++)
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
            DataRow newrow = null;
            DataRow lastvaluerow = null;
            string datum = string.Empty;
            string sourcecol = string.Empty;
            i = offset;
            if (i < 0)
                i = 0;
            while (i < num_rows + offset && i < data.Rows.Count)
            {
                newrow = ret.NewRow();
                DateTime dt_val;
                TimeSpan ts_val;
                double d_val;
                int i_val;
                Guid lat_guid = new Guid("C8A09A60-E42E-4D12-96EF-9F54A707B255");
                Guid lon_guid = new Guid("22DDBCD9-E1AD-4348-823B-542E6577B735");
                foreach (Field f in Header)
                {
                    if (f.IsSubfield)
                        continue;
                    if (data.Rows[i][f.SourceColumnName] == DBNull.Value)
                    {
                        newrow[f.SQLColumnName] = DBNull.Value;
                        continue;
                    }
                    datum = (string)data.Rows[i][f.SourceColumnName]; 
                        
                    if (f.IsTiered && string.IsNullOrEmpty(datum))
                    {
                        newrow[f.SQLColumnName] = lastvaluerow[f.SQLColumnName];
                    }
                    else
                    {
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
                                                    if (datum.Length == 8)
                                                    {
                                                        string year = datum.Substring(0, 4);
                                                        string month = datum.Substring(4, 2);
                                                        string day = datum.Substring(6, 2);

                                                        try
                                                        {
                                                            newrow[f.SQLColumnName] = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            newrow[f.SQLColumnName] = DBNull.Value;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        newrow[f.SQLColumnName] = DBNull.Value;
                                                    }                                                    
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
                                        string datum2 = string.Empty;
                                        TimeSpan timecomponent = TimeSpan.Zero;
                                        DateTime datecomponent = DateTime.MinValue;

                                        datum2 = (string)data.Rows[i][f.Subfield.SourceColumnName];
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
                                                if (!DateTime.TryParse(datum, out datecomponent))
                                                {
                                                    if (datum.Length == 8)
                                                    {
                                                        string year = datum.Substring(0, 4);
                                                        string month = datum.Substring(4, 2);
                                                        string day = datum.Substring(6, 2);

                                                        try
                                                        {
                                                            datecomponent = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                                                        }
                                                        catch (Exception ex)
                                                        {                                                           
                                                        }
                                                    }
                                                }                                                
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
                                                if (!DateTime.TryParse(datum2, out datecomponent))
                                                {
                                                    if (datum2.Length == 8)
                                                    {
                                                        string year = datum.Substring(0, 4);
                                                        string month = datum.Substring(4, 2);
                                                        string day = datum.Substring(6, 2);

                                                        try
                                                        {
                                                            datecomponent = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                        }
                                                    }
                                                }
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
                    }
                    if (lastvaluerow != null)
                        lastvaluerow[f.SQLColumnName] = newrow[f.SQLColumnName];
                    else
                    {
                        lastvaluerow = ret.NewRow();
                        lastvaluerow.ItemArray = (object[])newrow.ItemArray.Clone();
                    }
                }
                ret.Rows.Add(newrow);
                i += 1;
            }            
            return ret;
        }

        public void AutopopulateFields(List<Metric> metrics)
        {
            SqlConnection conn = null;
            try
            {
                conn = Utils.ConnectToDatabaseReadOnly(ParentContainer.database_name);
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
                        f.SQLColumnName = Utils.CreateDBName(col_name);
                        f.Name = col_name.Replace("'", "").Replace("\"", "");
                        Guid field_guid = Guid.Empty;
                        if (col_type == "int" || col_type == "bigint" || col_type == "smallint")
                        {
                            f.DBType = Field.FieldType.Integer;
                            field_guid = Metric.GenericInt;
                        }
                        else if (col_type == "float" || col_type == "numeric" || col_type == "real" || col_type == "decimal")
                        {
                            f.DBType = Field.FieldType.Decimal;
                            field_guid = Metric.GenericDecimal;
                        }
                        else if (col_type == "varchar" || col_type == "nvarchar" || col_type == "uniqueidentifier" || col_type == "char" || col_type == "nchar")
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
