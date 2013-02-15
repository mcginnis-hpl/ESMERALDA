using ESMERALDAClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Ionic.Zip;
using System.IO;

namespace ESMERALDA
{
    public partial class DownloadViewasCSV : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            View working = null;
            string viewid = string.Empty;
            bool isMetadata = false;
            string delimiter = ",";
            bool compress = false;
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
                if ((base.Request.Params.GetKey(i).ToUpper() == "COMPRESS") && (base.Request.Params[i] == "1"))
                {
                    compress = true;
                }
                if (base.Request.Params.GetKey(i).ToUpper() == "DELIM")
                {
                    if (Request.Params[i] == "TAB")
                    {
                        delimiter = "\t";
                    }
                    else
                    {
                        delimiter = ",";
                    }
                }
            }
            if (!string.IsNullOrEmpty(viewid))
            {
                working = (View)base.GetSessionValue("View-" + viewid);
                if (working != null)
                {
                    string fname = working.SourceData.GetMetadataValue("title");
                    string extension = "csv";
                    if (isMetadata)
                    {
                        extension = "xml";                        
                    }
                    else if (compress)
                    {
                        extension = "zip";
                    }
                    else
                    {
                        extension = "csv";
                    }
                    fname = fname + "-view." + extension;
                    string attachment = "attachment; filename=" + fname;                    
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
                        if (!compress)
                        {
                            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                            HttpContext.Current.Response.ContentType = "text/csv";
                        }
                        else
                        {
                            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                            HttpContext.Current.Response.ContentType = "application/zip";
                        }
                    }
                    HttpContext.Current.Response.AddHeader("Pragma", "public");
                    if (compress)
                    {
                        string inner_fname = fname + "-view.csv";
                        ZipOutputStream zipout = new ZipOutputStream(Response.OutputStream);
                        zipout.PutNextEntry(inner_fname);

                        StreamWriter outWriter = new StreamWriter(zipout);
                        PopulateData(working, delimiter, outWriter);
                        zipout.Dispose();                  
                    }
                    else
                    {
                        if(isMetadata)
                        {
                            string data = this.PopulateMetadata(working);
                            HttpContext.Current.Response.Write(data);
                        }
                        else
                        {
                            StreamWriter outWriter = new StreamWriter(Response.OutputStream);
                            PopulateData(working, delimiter, outWriter);
                        }
                    }                    
                    HttpContext.Current.Response.End();
                }
            }
        }

        protected void PopulateData(View working, string delimiter, StreamWriter outStream)
        {
            int i;
            string dbname = working.SourceData.ParentProject.database_name;
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);
            int numrows = -1;
            string newline = string.Empty;
            if (string.IsNullOrEmpty(working.SQLQuery))
            {
                i = 0;
                while (i < working.Header.Count)
                {
                    if (((ViewCondition)working.Header[i]).Type != ViewCondition.ConditionType.Exclude)
                    {
                        if (!string.IsNullOrEmpty(newline))
                        {
                            newline = newline + delimiter;
                        }
                        newline = newline + ((ViewCondition)working.Header[i]).SourceField.Name;
                    }
                    i++;
                }
                outStream.WriteLine(newline);
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
                        for (i = 0; i < working.Header.Count; i++)
                        {
                            if (((ViewCondition)working.Header[i]).Type != ViewCondition.ConditionType.Exclude)
                            {
                                if (!string.IsNullOrEmpty(newline))
                                {
                                    newline = newline + delimiter;
                                }
                                if (!reader.IsDBNull(reader.GetOrdinal(working.Header[i].SQLColumnName)))
                                {
                                    if (((ViewCondition)working.Header[i]).CondConversion != null)
                                    {
                                        newline = newline + ((ViewCondition)working.Header[i]).CondConversion.DestinationMetric.Format(reader[working.Header[i].SQLColumnName].ToString());
                                    }
                                    else
                                    {
                                        newline = newline + ((ViewCondition)working.Header[i]).SourceField.FieldMetric.Format(reader[working.Header[i].SQLColumnName].ToString());
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(newline))
                        {
                            outStream.WriteLine(newline);
                            outStream.Flush();
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
                                    header_row = header_row + delimiter;
                                }
                                header_row = header_row + reader.GetName(i);
                                i++;
                            }
                            outStream.WriteLine(header_row);
                        }
                        for (i = 0; i < reader.FieldCount; i++)
                        {
                            if (!string.IsNullOrEmpty(newline))
                            {
                                newline = newline + delimiter;
                            }
                            newline = newline + reader[i].ToString();
                        }
                        if (!string.IsNullOrEmpty(newline))
                        {
                            outStream.WriteLine(newline);
                            outStream.Flush();
                        }
                    }
                }
                reader.Close();
                conn.Close();
            }
        }

        protected string PopulateMetadata(View working)
        {
            string ret = string.Empty;
            ret = "<metadata>";
            return (ret + working.GetMetadata() + "</metadata>");
        }
    }
}