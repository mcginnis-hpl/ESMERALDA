using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{
    public class DownloadRequest
    {
        public Guid requestid;
        public Guid sourceid;
        public string filename;
        public string delimiter;
        public Guid userid;

        public DownloadRequest()
        {
            requestid = Guid.Empty;
            filename = string.Empty;
            sourceid = Guid.Empty;
            userid = Guid.Empty;
        }

        public void Write(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_SaveDownloadRequest"
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.Parameters.Add(new SqlParameter("@infilename", filename));
            cmd.Parameters.Add(new SqlParameter("@inviewid", sourceid));
            cmd.Parameters.Add(new SqlParameter("@indelimiter", delimiter));
            cmd.Parameters.Add(new SqlParameter("@inuserid", userid));
            cmd.ExecuteNonQuery();
        }

        public void FlagRequest(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_FlagRequest"
            };
            cmd.Parameters.Add(new SqlParameter("@inrequestid", requestid));
            cmd.ExecuteNonQuery();
        }

        public static List<DownloadRequest> LoadUnprocessedRequests(SqlConnection conn)
        {
            List<DownloadRequest> ret = new List<DownloadRequest>();
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_GetRequests"
            };
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("requestid")))
                {
                    DownloadRequest d = new DownloadRequest();
                    d.sourceid = new Guid(reader["viewid"].ToString());
                    d.requestid = new Guid(reader["requestid"].ToString());
                    d.filename = reader["filename"].ToString();
                    d.delimiter = reader["delimiter"].ToString();
                    d.userid = new Guid(reader["userid"].ToString());
                    ret.Add(d);
                }
            }
            reader.Close();
            return ret;
        }
    }
}
