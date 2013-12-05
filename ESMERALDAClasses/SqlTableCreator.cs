namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;

    public class SqlTableCreator
    {
        private SqlConnection _connection;
        private string _tableName;
        private SqlTransaction _transaction;

        public SqlTableCreator()
        {
        }

        public SqlTableCreator(SqlConnection connection) : this(connection, null)
        {
        }

        public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            this._connection = connection;
            this._transaction = transaction;
        }

        public object Create(DataTable schema)
        {
            return this.Create(schema, (int[]) null);
        }

        public object Create(DataTable schema, int numKeys)
        {
            int[] primaryKeys = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return this.Create(schema, primaryKeys);
        }

        public object Create(DataTable schema, int[] primaryKeys)
        {
            SqlCommand cmd;
            string sql = string.Empty;
            bool table_exists = TableExists(this._tableName, this._connection, this._transaction);
            sql = GetCreateSQL(this._tableName, schema, primaryKeys, table_exists);
            if ((this._transaction != null) && (this._transaction.Connection != null))
            {
                cmd = new SqlCommand(sql, this._connection, this._transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, this._connection);
            }
            return cmd.ExecuteNonQuery();
        }

        public object CreateFromDataTable(DataTable table)
        {
            SqlCommand cmd;
            string sql = string.Empty;
            bool table_exists = TableExists(this._tableName, this._connection, this._transaction);
            sql = GetCreateFromDataTableSQL(this._tableName, table, table_exists);
            if ((this._transaction != null) && (this._transaction.Connection != null))
            {
                cmd = new SqlCommand(sql, this._connection, this._transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, this._connection);
            }
            return cmd.ExecuteNonQuery();
        }

        public static string GetCreateFromDataTableSQL(string tableName, DataTable table, bool table_exists)
        {
            string sql = string.Empty;
            if (table_exists)
            {
                sql = sql + "DROP TABLE [" + tableName + "];";
            }
            sql = sql + "CREATE TABLE [" + tableName + "] (\n";
            foreach (DataColumn column in table.Columns)
            {
                sql = sql + "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";
            if (table.PrimaryKey.Length > 0)
            {
                sql = sql + "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql = sql + "[" + column.ColumnName + "],";
                }
                sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
            }
            if (!((table.PrimaryKey.Length != 0) || sql.EndsWith(")")))
            {
                sql = sql + ")";
            }
            return sql;
        }

        public static string GetCreateSQL(string tableName, DataTable schema, int[] primaryKeys, bool table_exists)
        {
            string sql = string.Empty;
            if (table_exists)
            {
                sql = sql + "DROP TABLE [" + tableName + "];";
            }
            sql = sql + "CREATE TABLE [" + tableName + "] (\n";
            foreach (DataRow column in schema.Rows)
            {
                if (!schema.Columns.Contains("IsHidden") || !((bool) column["IsHidden"]))
                {
                    sql = sql + "\t[" + column["ColumnName"].ToString() + "] " + SQLGetType(column);
                    if (!(!schema.Columns.Contains("AllowDBNull") || ((bool) column["AllowDBNull"])))
                    {
                        sql = sql + " NOT NULL";
                    }
                    sql = sql + ",\n";
                }
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";
            string pk = ", CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (";
            bool hasKeys = (primaryKeys != null) && (primaryKeys.Length > 0);
            if (hasKeys)
            {
                foreach (int key in primaryKeys)
                {
                    pk = pk + schema.Rows[key]["ColumnName"].ToString() + ", ";
                }
            }
            else
            {
                string keys = string.Join(", ", GetPrimaryKeys(schema));
                pk = pk + keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(new char[] { ',', ' ', '\n' }) + ")\n";
            if (hasKeys)
            {
                sql = sql + pk;
            }
            return (sql + ")");
        }

        public static string GetDeleteSQL(string tableName)
        {
            return ("DROP TABLE [" + tableName + "]");
        }

        public static string[] GetPrimaryKeys(DataTable schema)
        {
            List<string> keys = new List<string>();
            foreach (DataRow column in schema.Rows)
            {
                if (schema.Columns.Contains("IsKey") && ((bool) column["IsKey"]))
                {
                    keys.Add(column["ColumnName"].ToString());
                }
            }
            return keys.ToArray();
        }

        public static DataTable GetSchemaTable(DataTable inTable)
        {
            DataTableReader dr = new DataTableReader(inTable);
            DataTable schema = dr.GetSchemaTable();
            dr.Close();
            return schema;
        }

        public static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }

        public static string SQLGetType(DataRow schemaRow)
        {
            string precision = schemaRow["NumericPrecision"].ToString();
            string scale = schemaRow["NumericScale"].ToString();
            return SQLGetType(schemaRow["DataType"], int.Parse(schemaRow["ColumnSize"].ToString()), string.IsNullOrEmpty(precision) ? 0 : int.Parse(precision), string.IsNullOrEmpty(scale) ? 0 : int.Parse(scale));
        }

        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    return ("NVARCHAR(" + ((columnSize == -1) ? "255" : ((columnSize > 0x1f40) ? "MAX" : columnSize.ToString())) + ")");

                case "System.Decimal":
                    if (numericScale <= 0)
                    {
                        if (numericPrecision > 10)
                        {
                            return "BIGINT";
                        }
                        return "INT";
                    }
                    return "REAL";

                case "System.Double":
                case "System.Single":
                    return "REAL";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int16":
                case "System.Int32":
                    return "INT";

                case "System.DateTime":
                case "System.TimeSpan":
                    return "DATETIME";

                case "System.Boolean":
                    return "BIT";

                case "System.Byte":
                    return "TINYINT";

                case "System.Guid":
                    return "UNIQUEIDENTIFIER";
            }
            throw new Exception(type.ToString() + " not implemented.");
        }

        public static bool TableExists(string tablename, SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand mycommand = new SqlCommand("SELECT case when object_id(@tablename)is not null then 1 else 0 end") {
                CommandType = CommandType.Text
            };
            mycommand.Parameters.Add(new SqlParameter("@tablename", tablename));
            mycommand.Connection = conn;
            if (trans != null)
            {
                mycommand.Transaction = trans;
            }
            SqlDataReader reader = mycommand.ExecuteReader();
            bool ret = false;
            while (reader.Read())
            {
                if (!reader.IsDBNull(0) && (reader[0].ToString() == "1"))
                {
                    ret = true;
                    break;
                }
            }
            reader.Close();
            return ret;
        }

        public void WriteData(DataTable dt)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(this._connection) {
                DestinationTableName = this.DestinationTableName,
                BulkCopyTimeout = 0x1770
            };
            try
            {
                bulkCopy.WriteToServer(dt);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public SqlConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                this._connection = value;
            }
        }

        public string DestinationTableName
        {
            get
            {
                if (this._tableName.IndexOf("[") < 0)
                {
                    return "[" + this._tableName + "]";
                }
                else
                {
                    return this._tableName;
                }
            }
            set
            {
                this._tableName = value;
            }
        }

        public SqlTransaction Transaction
        {
            get
            {
                return this._transaction;
            }
            set
            {
                this._transaction = value;
            }
        }
    }
}

