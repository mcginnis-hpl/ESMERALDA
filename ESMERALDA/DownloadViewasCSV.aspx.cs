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
            Dataset.MetadataFormat meta_format = EsmeraldaEntity.MetadataFormat.XML;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                {
                    viewid = base.Request.Params[i];
                }
                if ((base.Request.Params.GetKey(i).ToUpper() == "METADATA"))
                {
                    isMetadata = true;
                    if (base.Request.Params[i] == "BCODMO")
                    {
                        meta_format = EsmeraldaEntity.MetadataFormat.BCODMO;
                    }
                    else if (base.Request.Params[i] == "FGDC")
                    {
                        meta_format = EsmeraldaEntity.MetadataFormat.FGDC;
                    }
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
                working = (View)base.GetSessionValueCrossPage("View-" + viewid);
                if (working != null)
                {
                    string fname = working.SourceData.GetMetadataValue("title");
                    string extension = "csv";
                    if (isMetadata)
                    {
                        if (meta_format == EsmeraldaEntity.MetadataFormat.XML || meta_format == EsmeraldaEntity.MetadataFormat.FGDC)
                        {
                            extension = "xml";
                        }
                        else
                        {
                            extension = "txt";
                        }
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
                        if (meta_format == EsmeraldaEntity.MetadataFormat.XML || meta_format == EsmeraldaEntity.MetadataFormat.FGDC)
                        {
                            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                            HttpContext.Current.Response.ContentType = "text/xml";
                        }
                        else
                        {
                            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                            HttpContext.Current.Response.ContentType = "text/plain";
                        }
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
                            string data = this.PopulateMetadata(working, meta_format);
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

        protected void DoLinkedDownload(View working, string delimiter, StreamWriter outStream)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            working.Save(conn);            

            DownloadRequest req = new DownloadRequest();
            req.filename = working.ID.ToString() + ".csv";
            req.delimiter = delimiter;
            req.requestid = Guid.NewGuid();
            req.sourceid = working.ID;
            req.userid = CurrentUser.ID;
            req.Write(conn);

            string html = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head id=\"Head1\" runat=\"server\"><link href=\"css/style.css\" type=\"text/css\" rel=\"stylesheet\" /></head><body>";
            html += "<p>Your download is larger than the supported limit for streaming download.  The download service will create a file on our ftp server, and a url will be sent to your email address when that process is complete.</p></body></html>";
            HttpContext.Current.Response.AddHeader("content-disposition", string.Empty);
            HttpContext.Current.Response.ContentType = "text/html";
            conn.Close();
        }

        protected void PopulateData(View working, string delimiter, StreamWriter outStream)
        {
            string dbname = working.SourceData.ParentContainer.database_name;
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);
            int rowcount = working.GetRowCount(conn);
            int download_limit = int.Parse(GetAppString("appstring_downloadlimit"));
            working.WriteDataToStream(delimiter, outStream, conn);
            /*if (rowcount > download_limit)
            {
                DoLinkedDownload(working, delimiter, outStream);
            }
            else
            {
                working.WriteDataToStream(delimiter, outStream, conn);
            }*/
            conn.Close();
        }

        protected string PopulateMetadata(View working, Dataset.MetadataFormat format)
        {
            string ret = string.Empty;
            if (format == EsmeraldaEntity.MetadataFormat.XML)
            {
                ret += "<?xml version=\"1.0\"?>";
                ret += "<metadata>";
                ret += working.GetMetadata(ESMERALDAClasses.EsmeraldaEntity.MetadataFormat.XML);
                ret += "</metadata>";
                return ret;
            }
            else
            {
                ret = working.GetMetadata(format);
                return ret;
            }
        }
    }
}