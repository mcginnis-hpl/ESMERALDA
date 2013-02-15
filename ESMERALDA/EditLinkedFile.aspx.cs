using AjaxControlToolkit;
using ESMERALDAClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class EditLinkedFile : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void FilterInput()
        {
        }

        protected void ProcessUpload(object sender, AsyncFileUploadEventArgs e)
        {
            Guid projectID = Guid.Empty;
            int i = 0;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID")
                {
                    projectID = new Guid(base.Request.Params[i]);
                }
            }            
            if (this.uploadFiles2.PostedFile != null)
            {
                LinkedFile lf = new LinkedFile();
                HttpPostedFile userPostedFile = this.uploadFiles2.PostedFile;
                string filename = userPostedFile.FileName;
                string mimetype = userPostedFile.ContentType;
                byte[] data = new byte[userPostedFile.ContentLength];
                userPostedFile.InputStream.Read(data, 0, userPostedFile.ContentLength);
                System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
                string dest_path = rootWebConfig1.AppSettings.Settings["FileStorePath"].Value;
                lf.Name = filename;
                lf.ID = Guid.NewGuid();
                lf.FilePath = dest_path + lf.ID.ToString();
                lf.MIMEType = mimetype;
                lf.Timestamp = DateTime.Now;
                if (projectID != Guid.Empty)
                {
                    SqlConnection conn = ConnectToConfigString("RepositoryData");
                    lf.ParentProject = new Project();
                    lf.ParentProject.Load(conn, projectID);
                    conn.Close();
                }
            }
        }
    }
}