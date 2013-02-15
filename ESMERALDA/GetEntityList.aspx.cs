using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDA
{
    public partial class GetEntityList : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string request_type = string.Empty;
            Guid parentid = Guid.Empty;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "TYPE")
                {
                    request_type = Request.Params[i];
                }
                if (base.Request.Params.GetKey(i).ToUpper() == "PARENTID")
                {
                    parentid = new Guid(Request.Params[i]);
                }
            }
            string ret = GetList(request_type, parentid);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.Write(ret);
            HttpContext.Current.Response.End();
        }

        protected string GetList(string request_type, Guid parent_id)
        {
            string ret = string.Empty;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            if (request_type == "PROJECT")
            {
                SqlCommand query = new SqlCommand();               
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT project_id, project_name, program_name FROM v_ESMERALDA_entityinfo_project ORDER BY program_name, project_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                SqlDataReader reader = query.ExecuteReader();
                string proj_guid = string.Empty;
                string proj_name = string.Empty;
                string prog_name = string.Empty;
                while (reader.Read())
                {
                    proj_guid = reader["project_id"].ToString();
                    prog_name = reader["program_name"].ToString();
                    proj_name = reader["project_name"].ToString();
                    if (string.IsNullOrEmpty(ret))
                    {
                        ret = proj_guid + "," + proj_name + "," + prog_name;
                    }
                    else
                    {
                        ret += ";" + proj_guid + "," + proj_name + "," + prog_name;
                    }
                }
                reader.Close();
            }
            else if (request_type == "DATASET")
            {
                SqlCommand query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT query_id, query_name FROM v_ESMERALDA_entitydata_queries WHERE parent_id='" + parent_id.ToString() + "' ORDER BY query_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                SqlDataReader reader = query.ExecuteReader();
                string ds_guid = string.Empty;
                string ds_name = string.Empty;
                while (reader.Read())
                {
                    ds_guid = reader["query_id"].ToString();
                    ds_name = reader["query_name"].ToString();
                    if (string.IsNullOrEmpty(ret))
                    {
                        ret = ds_guid + "," + ds_name;
                    }
                    else
                    {
                        ret += ";" + ds_guid + "," + ds_name;
                    }
                }
                reader.Close();
            }
            else if (request_type == "FIELD")
            {
                SqlCommand query = new SqlCommand();
                query.CommandType = CommandType.Text;
                query.CommandText = "SELECT field_id, field_name FROM v_ESMERALDA_entitydata_fields WHERE query_id='" + parent_id.ToString() + "' ORDER BY field_name";
                query.CommandTimeout = 60;
                query.Connection = conn;
                SqlDataReader reader = query.ExecuteReader();
                string field_guid = string.Empty;
                string field_name = string.Empty;
                while (reader.Read())
                {
                    field_guid = reader["field_id"].ToString();
                    field_name = reader["field_name"].ToString();
                    if (string.IsNullOrEmpty(ret))
                    {
                        ret = field_guid + "," + field_name;
                    }
                    else
                    {
                        ret += ";" + field_guid + "," + field_name;
                    }
                }
                reader.Close();
            }
            conn.Close();
            return ret;
        }
    }
}