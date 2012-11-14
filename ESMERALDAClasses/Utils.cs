namespace ESMERALDAClasses
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using System.Text.RegularExpressions;

    public class Utils
    {
        public static SqlConnection ConnectToDatabase(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Client; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        public static SqlConnection ConnectToDatabaseReadOnly(string dbName)
        {
            SqlConnection ret = new SqlConnection("Server=10.1.13.205;Database=" + dbName + "; User Id= SqlServer_Client; password= p@$$w0rd;");
            ret.Open();
            return ret;
        }

        public static DateTime ConvertDateFromUTC(string inValue, TimeZoneInfo timezone)
        {
            DateTime val = DateTime.MinValue;
            if (!DateTime.TryParse(inValue, out val))
            {
                return DateTime.MinValue;
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(val, DateTimeKind.Utc.ToString(), timezone.Id.ToString());
        }

        public static DateTime ConvertDateToUTC(DateTime val, TimeZoneInfo timezone)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(val, timezone.Id.ToString(), DateTimeKind.Utc.ToString());
        }

        public static DateTime ConvertDateToUTC(string inValue, TimeZoneInfo timezone)
        {
            if (string.IsNullOrEmpty(inValue))
            {
                return DateTime.MinValue;
            }
            DateTime val = DateTime.MinValue;
            if (!DateTime.TryParse(inValue, out val))
            {
                try
                {
                    val = ConvertEpochTime(double.Parse(inValue));
                }
                catch (FormatException)
                {
                    return DateTime.MinValue;
                }
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(val, timezone.Id.ToString(), DateTimeKind.Utc.ToString());
        }

        public static DateTime ConvertEpochTime(double intime)
        {
            DateTime ret = new DateTime(0x770, 1, 1, 0, 0, 0, 0);
            return ret.ToLocalTime().AddDays(intime);
        }

        public static string CreateDBName(string inField)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(inField, "").Replace(" ", "_").Replace("-", "_");
        }

        public static string MapFieldType(Field.FieldType inType)
        {
            if (inType == Field.FieldType.DateTime)
            {
                return "DateTime";
            }
            if (inType == Field.FieldType.Decimal)
            {
                return "Decimal";
            }
            if (inType == Field.FieldType.Integer)
            {
                return "Integer";
            }
            if (inType != Field.FieldType.None)
            {
                if (inType == Field.FieldType.Text)
                {
                    return "Text";
                }
                if (inType == Field.FieldType.Time)
                {
                    return "Time";
                }
            }
            return "None";
        }

        public static string RandomColor()
        {
            Random random = new Random();
            return string.Format("#{0:X6}", random.Next(0x1000000));
        }

        public static string ToCSV(DataTable table)
        {
            StringBuilder result = new StringBuilder();
            int i = 0;
            while (i < table.Columns.Count)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append((i == (table.Columns.Count - 1)) ? "\n" : ",");
                i++;
            }
            foreach (DataRow row in table.Rows)
            {
                for (i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(row[i].ToString());
                    result.Append((i == (table.Columns.Count - 1)) ? "\n" : ",");
                }
            }
            return result.ToString();
        }

        public static string GetEntityType(Guid inID, SqlConnection conn)
        {
            string ret = string.Empty;
            SqlCommand query = new SqlCommand {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadView",
                CommandTimeout = 60
            };
            query.Parameters.Add(new SqlParameter("@inID", inID));
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                {
                    ret = "VIEW";
                }
            }
            reader.Close();
            if (string.IsNullOrEmpty(ret))
            {
                query = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "sp_LoadDataset",
                    CommandTimeout = 60
                };
                query.Parameters.Add(new SqlParameter("@inID", inID));
                reader = query.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                    {
                        ret = "DATASET";
                    }
                }
                reader.Close();
            }
            return ret;
        }
    }
}

