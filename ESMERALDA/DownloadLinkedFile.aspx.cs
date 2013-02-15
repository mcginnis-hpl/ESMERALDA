using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDA
{
    public partial class DownloadLinkedFile : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Guid fileid = Guid.Empty;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "FILEID")
                {
                    fileid = new Guid(base.Request.Params[i]);
                }
            }
            if (fileid != Guid.Empty)
            {
                SqlConnection conn = ConnectToConfigString("RepositoryConnection");
                LinkedFile lf = new LinkedFile();
                lf.Load(conn, fileid);
                conn.Close();
                if (lf != null && !string.IsNullOrEmpty(lf.FilePath))
                {
                    string attachment = "attachment; filename=" + lf.Name;
                    byte[] data = null;
                    try
                    {
                        data = File.ReadAllBytes(lf.FilePath);
                    }
                    catch (Exception)
                    {
                    }
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.ClearHeaders();
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.AddHeader("content-disposition", attachment);                    
                    HttpContext.Current.Response.AddHeader("Pragma", "public");
                    if (data != null)
                    {
                        HttpContext.Current.Response.Write(data);
                        HttpContext.Current.Response.ContentType = lf.MIMEType;
                    }
                    else
                    {
                        string msg = "Could not load file.  Please contact your system administrator.";
                        HttpContext.Current.Response.Write(msg);
                        HttpContext.Current.Response.ContentType = "text/html";
                    }
                    HttpContext.Current.Response.End();
                }                
            }
        }
    }
}