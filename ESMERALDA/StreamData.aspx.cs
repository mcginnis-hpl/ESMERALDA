using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

using ESMERALDAClasses;

namespace ESMERALDA
{
    public partial class StreamData : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Guid viewid = Guid.Empty;
            int numrows = 100;
            for(int i=0; i < Request.Params.Count; i++)
            {
                if(Request.Params.GetKey(i) == "VIEWID")
                {
                    viewid = new Guid(Request.Params[i]);
                }
                if(Request.Params.GetKey(i) == "NUMROWS")
                {
                    numrows = int.Parse(Request.Params[i]);
                }
            }
            if(viewid != Guid.Empty)
            {
                ESMERALDAClasses.View working = (ESMERALDAClasses.View)GetSessionValue(viewid.ToString());
                DateTime start = DateTime.Now;
                PopulateData(working, numrows);
                System.Diagnostics.Debug.WriteLine("New way: " + (DateTime.Now - start).TotalMilliseconds.ToString() + "ms");
            }
        }

        protected void PopulateData(ESMERALDAClasses.View working, int numrows)
        {
            string dbname = working.SourceData.ParentProject.database_name;            
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);

            Response.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            Response.Write("<html xmlns=\"http://www.w3.org/1999/xhtml\"><head runat=\"server\"><title></title>");
            Random r = new Random();
            Response.Write("<link href=\"css/style.css?foo=" + r.Next().ToString() + "\" type=\"text/css\" rel=\"stylesheet\" /></head>");
            Response.Write("<body style='background-color:white'><div>");            
            Response.Write("<table class='previewTable'>");
            SqlDataReader reader = null;
            SqlCommand query = null;
            string mod_flag = string.Empty;
            int i = 0;
            if (!string.IsNullOrEmpty(working.SQLQuery))
            {
                query = new SqlCommand
                {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text,
                    CommandText = working.SQLQuery
                };
                reader = null;
                bool header_done = false;
                try
                {
                    reader = query.ExecuteReader();
                }
                catch (Exception ex)
                {                    
                    return;
                }
                while (reader.Read())
                {
                    if(!header_done)
                    {
                        Response.Write("<tr>");
                        i = 0;
                        while (i < reader.FieldCount)
                        {
                            Response.Write("<th>" + reader.GetName(i) + "</th>");
                            i++;
                        }                        
                        Response.Write("</tr>");
                        header_done = true;
                    }
                    Response.Write("<tr>");
                    for (i = 0; i < reader.FieldCount; i++)
                    {                        
                        Response.Write("<td>" + reader[i].ToString() + "</td>");                        
                    }
                    Response.Write("</tr>");
                }
                reader.Close();
            }
            else
            {
                string cmd = working.GetQuery(numrows);
                i = 0;
                Response.Write("<tr>");
                while (i < working.Header.Count)
                {
                    if ((((ViewCondition)working.Header[i]).SourceField != null) && (((ViewCondition)working.Header[i]).Type != ViewCondition.ConditionType.Exclude))
                    {
                        Response.Write("<th>" + ((ViewCondition)working.Header[i]).SQLColumnName + "</th>");
                    }
                    i++;
                }
                Response.Write("</tr>");
                if (!string.IsNullOrEmpty(cmd))
                {
                    try
                    {
                        reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                    while (reader.Read())
                    {
                        Response.Write("<tr>");
                        for (i = 0; i < working.Header.Count; i++)
                        {
                            if ((((ViewCondition)working.Header[i]).SourceField != null) && (((ViewCondition)working.Header[i]).Type != ViewCondition.ConditionType.Exclude))
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal(working.Header[i].SQLColumnName)))
                                {
                                    mod_flag = string.Empty;
                                    if ((((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Conversion) || (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Formula))
                                    {
                                        mod_flag = " class='modifiedHeaderCell'";
                                    }
                                    if (((ViewCondition)working.Header[i]).CondConversion != null)
                                    {
                                        Response.Write("<td" + mod_flag + ">" + ((ViewCondition)working.Header[i]).CondConversion.DestinationMetric.Format(reader[working.Header[i].SQLColumnName].ToString()) + "</td>");
                                    }
                                    else
                                    {
                                        Response.Write("<td" + mod_flag + ">" + ((ViewCondition)working.Header[i]).SourceField.FieldMetric.Format(reader[working.Header[i].SQLColumnName].ToString()) + "</td>");
                                    }
                                }
                                else
                                {
                                    Response.Write("<td></td>");
                                }
                            }
                        }
                        Response.Write("</tr>");
                    }
                    reader.Close();
                }
            }
            Response.Write("</table>");
            Response.Write("</div></body>");
            conn.Close();
        }
    }
}