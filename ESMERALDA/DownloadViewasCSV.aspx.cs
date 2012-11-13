using ESMERALDAClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace ESMERALDA
{
    public partial class DownloadViewasCSV : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            View working = null;
            string viewid = string.Empty;
            bool isMetadata = false;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                {
                    viewid = base.Request.Params[i];
                }
                if ((base.Request.Params.GetKey(i).ToUpper() == "METADATA") && (base.Request.Params[i] == "1"))
                {
                    isMetadata = true;
                }
            }
            if (!string.IsNullOrEmpty(viewid))
            {
                working = (View)base.GetSessionValue("View-" + viewid);
                if (working != null)
                {
                    string fname = working.SourceData.Name;
                    if (isMetadata)
                    {
                        fname = fname + "-view.xml";
                    }
                    else
                    {
                        fname = fname + "-view.csv";
                    }
                    string attachment = "attachment; filename=" + fname;
                    string data = string.Empty;
                    if (!isMetadata)
                    {
                        data = this.PopulateData(working);
                    }
                    else
                    {
                        data = this.PopulateMetadata(working);
                    }
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ClearHeaders();
                    HttpContext.Current.Response.ClearContent();
                    if (isMetadata)
                    {
                        HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                        HttpContext.Current.Response.ContentType = "text/xml";
                    }
                    else
                    {
                        HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                        HttpContext.Current.Response.ContentType = "text/csv";
                    }
                    HttpContext.Current.Response.AddHeader("Pragma", "public");
                    HttpContext.Current.Response.Write(data);
                    HttpContext.Current.Response.End();
                }
            }
        }

        protected string PopulateData(View working)
        {
            int i;
            string ret = string.Empty;
            string dbname = working.SourceData.ParentProject.database_name;
            string tablename = working.SourceData.TableName;
            SqlConnection conn = base.ConnectToDatabase(dbname);
            int numrows = -1;
            string newline = string.Empty;
            if (string.IsNullOrEmpty(working.SQLQuery))
            {
                i = 0;
                while (i < working.Conditions.Count)
                {
                    if (working.Conditions[i].Type != ViewCondition.ConditionType.Exclude)
                    {
                        if (!string.IsNullOrEmpty(newline))
                        {
                            newline = newline + ",";
                        }
                        newline = newline + working.Conditions[i].SourceField.Name;
                    }
                    i++;
                }
                ret = newline;
            }
            string cmd = working.GetQuery(numrows);
            if (!string.IsNullOrEmpty(cmd))
            {
                SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                if (string.IsNullOrEmpty(working.SQLQuery))
                {
                    while (reader.Read())
                    {
                        newline = string.Empty;
                        for (i = 0; i < working.Conditions.Count; i++)
                        {
                            if (working.Conditions[i].Type != ViewCondition.ConditionType.Exclude)
                            {
                                if (!string.IsNullOrEmpty(newline))
                                {
                                    newline = newline + ",";
                                }
                                if (!reader.IsDBNull(reader.GetOrdinal(working.Conditions[i].SQLName)))
                                {
                                    if (working.Conditions[i].CondConversion != null)
                                    {
                                        newline = newline + working.Conditions[i].CondConversion.DestinationMetric.Format(reader[working.Conditions[i].SQLName].ToString());
                                    }
                                    else
                                    {
                                        newline = newline + working.Conditions[i].SourceField.FieldMetric.Format(reader[working.Conditions[i].SQLName].ToString());
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(newline))
                        {
                            ret = ret + "\n" + newline;
                        }
                    }
                }
                else
                {
                    string header_row = string.Empty;
                    newline = string.Empty;
                    while (reader.Read())
                    {
                        newline = string.Empty;
                        if (string.IsNullOrEmpty(header_row))
                        {
                            i = 0;
                            while (i < reader.FieldCount)
                            {
                                if (!string.IsNullOrEmpty(header_row))
                                {
                                    header_row = header_row + ",";
                                }
                                header_row = header_row + reader.GetName(i);
                                i++;
                            }
                            ret = header_row;
                        }
                        for (i = 0; i < reader.FieldCount; i++)
                        {
                            if (!string.IsNullOrEmpty(newline))
                            {
                                newline = newline + ",";
                            }
                            newline = newline + reader[i].ToString();
                        }
                        if (!string.IsNullOrEmpty(newline))
                        {
                            ret = ret + "\n" + newline;
                        }
                    }
                }
                reader.Close();
                conn.Close();
            }
            return ret;
        }

        protected string PopulateMetadata(View working)
        {
            string ret = string.Empty;
            ret = "<metadata>";
            return (ret + working.GetMetadata() + "</metadata>");
        }
    }
}