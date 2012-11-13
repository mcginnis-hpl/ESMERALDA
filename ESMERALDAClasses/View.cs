namespace ESMERALDAClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class View : EsmeraldaEntity
    {
        public string BriefDescription;
        public List<ViewCondition> Conditions;
        public Person CreatedBy;
        public DateTime CreatedOn;
        public string Description;
        public string Name;
        public Dataset SourceData;
        public string SQLQuery;
        public string ViewSQLName;

        public View(Dataset inData)
        {
            this.SourceData = inData;
            this.Conditions = new List<ViewCondition>();
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.BriefDescription = string.Empty;
            this.CreatedBy = null;
            this.CreatedOn = DateTime.MinValue;
            this.ViewSQLName = string.Empty;
            base.ID = Guid.NewGuid();
            this.SQLQuery = string.Empty;
        }

        public void AutopopulateConditions()
        {
            foreach (Field f in this.SourceData.Header)
            {
                if (!f.IsSubfield)
                {
                    ViewCondition cond = new ViewCondition(f, ViewCondition.ConditionType.None, this);
                    this.Conditions.Add(cond);
                }
            }
        }

        protected void CreateSQLView(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.Text
            };
            string cmd = "IF OBJECT_ID ('dbo." + this.ViewSQLName + "', 'V') IS NOT NULL DROP VIEW dbo." + this.ViewSQLName + " ;";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();
            query = new SqlCommand {
                CommandType = CommandType.Text
            };
            cmd = "CREATE VIEW " + this.ViewSQLName + " AS " + this.GetQuery(-1) + ";";
            query.CommandText = cmd;
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.ExecuteNonQuery();
        }

        public string GetMetadata()
        {
            string ret = "<data_view>";
            ret = (((ret + "<view_name>" + this.Name + "</view_name>") + "<brief_description>" + this.BriefDescription + "</brief_description>") + "<description>" + this.Description + "</description>") + "<created_on>" + this.CreatedOn.ToShortDateString() + "</created_on>";
            if (this.CreatedBy != null)
            {
                ret = ret + "<createdby>" + this.CreatedBy.GetMetadata() + "</createdby>";
            }
            if (this.SourceData != null)
            {
                if (this.SourceData.ParentProject != null)
                {
                    if (this.SourceData.ParentProject.parentProgram != null)
                    {
                        ret = ret + "<parent_program>" + this.SourceData.ParentProject.parentProgram.GetMetadata() + "</parent_program>";
                    }
                    ret = ret + "<parent_project>" + this.SourceData.ParentProject.GetMetadata() + "</parent_project>";
                }
                ret = ret + "<dataset>" + this.SourceData.GetMetadata() + "</dataset>";
            }
            return (ret + "</data_view>");
        }

        public string GetQuery(int numrows)
        {
            int i;
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(this.SQLQuery))
            {
                return this.SQLQuery;
            }
            ret = "SELECT";
            if (numrows > 0)
            {
                ret = ret + " TOP(" + numrows.ToString() + ")";
            }
            bool init = false;
            for (i = 0; i < this.Conditions.Count; i++)
            {
                if ((this.Conditions[i].SourceField != null) && (this.Conditions[i].Type != ViewCondition.ConditionType.Exclude))
                {
                    if (init)
                    {
                        ret = ret + ",";
                    }
                    else
                    {
                        init = true;
                    }
                    if (this.Conditions[i].Type == ViewCondition.ConditionType.Formula)
                    {
                        string formula_text = this.Conditions[i].Condition;
                        foreach (Field f in this.SourceData.Header)
                        {
                            if (formula_text.IndexOf("[" + f.Name + "]") >= 0)
                            {
                                formula_text = formula_text.Replace("[" + f.Name + "]", f.FormattedColumnName);
                            }
                            else if (formula_text.IndexOf("[" + f.SQLColumnName + "]") >= 0)
                            {
                                formula_text = formula_text.Replace("[" + f.SQLColumnName + "]", f.FormattedColumnName);
                            }
                        }
                        ret = ret + " (" + formula_text + ") AS " + this.Conditions[i].SQLName;
                    }
                    else if ((this.Conditions[i].Type == ViewCondition.ConditionType.Conversion) && (this.Conditions[i].SourceField.DBType != Field.FieldType.DateTime))
                    {
                        ret = ret + " Repository_Metadata.dbo." + this.Conditions[i].CondConversion.FormulaName + "(" + this.Conditions[i].SourceField.FormattedColumnName + ") AS " + this.Conditions[i].SQLName;
                    }
                    else
                    {
                        ret = ret + this.Conditions[i].BuildClause();
                    }
                }
            }
            if (!init)
            {
                return string.Empty;
            }
            ret = ret + " FROM [" + this.SourceData.TableName + "]";
            init = false;
            for (i = 0; i < this.Conditions.Count; i++)
            {
                if (this.Conditions[i].Type == ViewCondition.ConditionType.Filter)
                {
                    if (init)
                    {
                        ret = ret + " AND";
                    }
                    else
                    {
                        ret = ret + " WHERE";
                        init = true;
                    }
                    ret = ret + " (" + this.Conditions[i].SourceField.FormattedColumnName + " " + this.Conditions[i].Condition + ")";
                }
            }
            init = false;
            for (i = 0; i < this.Conditions.Count; i++)
            {
                if (this.Conditions[i].Type == ViewCondition.ConditionType.SortAscending)
                {
                    if (init)
                    {
                        ret = ret + ",";
                    }
                    else
                    {
                        ret = ret + " ORDER BY";
                        init = true;
                    }
                    ret = ret + " " + this.Conditions[i].SourceField.FormattedColumnName + " ASC";
                }
                else if (this.Conditions[i].Type == ViewCondition.ConditionType.SortDescending)
                {
                    if (init)
                    {
                        ret = ret + ",";
                    }
                    else
                    {
                        ret = ret + " ORDER BY";
                        init = true;
                    }
                    ret = ret + " " + this.Conditions[i].SourceField.FormattedColumnName + " DESC";
                }
            }
            return ret;
        }

        public static View LoadView(SqlConnection conn, Guid viewID, List<Conversion> globalConversions, List<Metric> metrics)
        {
            SqlCommand query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadView",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inID", viewID));
            SqlDataReader reader = query.ExecuteReader();
            Guid enteredbyid = Guid.Empty;
            Guid sourceid = Guid.Empty;
            string view_name = string.Empty;
            string view_briefdescription = string.Empty;
            string view_description = string.Empty;
            string view_sqlname = string.Empty;
            string view_query = string.Empty;
            DateTime view_createdon = DateTime.MinValue;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    sourceid = new Guid(reader["dataset_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("view_name")))
                    {
                        view_name = reader["view_name"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("description")))
                    {
                        view_description = reader["description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("brief_description")))
                    {
                        view_briefdescription = reader["brief_description"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("createdon")))
                    {
                        view_createdon = DateTime.Parse(reader["createdon"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("createdby")))
                    {
                        enteredbyid = new Guid(reader["createdby"].ToString());
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("view_sqlname")))
                    {
                        view_sqlname = reader["view_sqlname"].ToString();
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("query")))
                    {
                        view_query = reader["query"].ToString();
                    }
                }
            }
            reader.Close();
            Dataset source = Dataset.Load(conn, sourceid, metrics);
            View ret = new View(source) {
                ID = viewID,
                Name = view_name,
                BriefDescription = view_briefdescription,
                Description = view_description,
                CreatedOn = view_createdon,
                ViewSQLName = view_sqlname,
                SQLQuery = view_query
            };
            query = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_LoadViewConditions",
                CommandTimeout = 60,
                Connection = conn
            };
            query.Parameters.Add(new SqlParameter("@inview_id", viewID));
            reader = query.ExecuteReader();
            Guid conversion_id = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("view_id")))
                {
                    Field sourceField = null;
                    Guid fieldid = new Guid(reader["field_id"].ToString());
                    int field_type = int.Parse(reader["condition_type"].ToString());
                    foreach (Field f in source.Header)
                    {
                        if (f.ID == fieldid)
                        {
                            sourceField = f;
                            break;
                        }
                    }
                    if (sourceField == null)
                    {
                        continue;
                    }
                    ViewCondition con = new ViewCondition(sourceField, (ViewCondition.ConditionType) field_type, ret) {
                        ID = new Guid(reader["condition_id"].ToString()),
                        Condition = reader["condition_text"].ToString()
                    };
                    if (!reader.IsDBNull(reader.GetOrdinal("condition_conversion")))
                    {
                        conversion_id = new Guid(reader["condition_conversion"].ToString());
                        for (int i = 0; i < globalConversions.Count; i++)
                        {
                            if (globalConversions[i].ID == conversion_id)
                            {
                                con.CondConversion = globalConversions[i];
                            }
                        }
                    }
                    con.SQLName = reader["sql_name"].ToString();
                    EsmeraldaEntity.Load(conn, con);
                    ret.Conditions.Add(con);
                }
            }
            reader.Close();
            EsmeraldaEntity.Load(conn, ret);
            return ret;
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (base.ID == Guid.Empty)
            {
                base.ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteView";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inview_id", base.ID));
            query.Parameters.Add(new SqlParameter("@indataset_id", this.SourceData.ID));
            query.Parameters.Add(new SqlParameter("@inview_name", this.Name));
            if (string.IsNullOrEmpty(this.ViewSQLName))
            {
                this.ViewSQLName = Utils.CreateDBName(this.Name);
            }
            query.Parameters.Add(new SqlParameter("@inview_sqlname", this.ViewSQLName));
            query.Parameters.Add(new SqlParameter("@indescription", this.Description));
            query.Parameters.Add(new SqlParameter("@inbrief_description", this.BriefDescription));
            if (this.CreatedOn == DateTime.MinValue)
            {
                this.CreatedOn = DateTime.Now;
            }
            query.Parameters.Add(new SqlParameter("@increatedon", this.CreatedOn));
            if (this.CreatedBy != null)
            {
                query.Parameters.Add(new SqlParameter("@increatedby", this.CreatedBy.ID));
            }
            query.Parameters.Add(new SqlParameter("@query", this.SQLQuery));
            query.ExecuteNonQuery();
            foreach (ViewCondition v in this.Conditions)
            {
                if (v.Owner == null)
                {
                    v.Owner = base.Owner;
                }
                v.Save(conn, base.ID);
            }
            base.Save(conn);
            SqlConnection dataconn = Utils.ConnectToDatabase(this.SourceData.ParentProject.database_name);
            this.CreateSQLView(dataconn);
            dataconn.Close();
        }
    }
}

