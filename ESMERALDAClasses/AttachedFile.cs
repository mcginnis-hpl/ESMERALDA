using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ESMERALDAClasses
{
    public class AttachedFile
    {
        public string Path;
        public string Filename;
        public Guid ID;

        public AttachedFile()
        {
            Path = string.Empty;
            ID = Guid.Empty;
            Filename = string.Empty;
        }

        /// <summary>
        /// Load the metadata for an attached file from the source database.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        /// <param name="inid">The GUID of the attached file to be loaded.</param>
        public void Load(SqlConnection conn, Guid inid)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandText = "sp_ESMERALDA_loadattachedfile",
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@inattachedfileid", inid));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("attachedfileid")))
                {
                    ID = new Guid(reader["attachedfileid"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filepath")))
                {
                    Path = reader["filepath"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("filename")))
                {
                    Filename = reader["filename"].ToString();
                }
            }
            reader.Close();
        }

        /// <summary>
        /// Save the metadata associated with an attached file.
        /// </summary>
        /// <param name="conn">An open connection to the IMETPS database.</param>
        /// <param name="requestid">The ID of the attached file to be saved.</param>
        public void Save(SqlConnection conn, Guid entityid)
        {
            if (ID == Guid.Empty)
                ID = Guid.NewGuid();
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.StoredProcedure,
                CommandText = "sp_ESMERALDA_saveattachedfile"
            };
            cmd.Parameters.Add(new SqlParameter("@inid", ID));
            cmd.Parameters.Add(new SqlParameter("@inentityid", entityid));
            cmd.Parameters.Add(new SqlParameter("@inpath", Path));
            cmd.Parameters.Add(new SqlParameter("@infilename", Filename));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete the saved local (server) copy of an attached file.
        /// </summary>
        public void DeleteLocalCopy()
        {
            try
            {
                File.Delete(Path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Return an array of bytes containing the entire attached file.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            FileStream fs = null;
            byte[] ret = null;
            try
            {
                fs = File.Open(Path, FileMode.Open);
                ret = new byte[fs.Length];
                fs.Read(ret, 0, (int)fs.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    try
                    {
                        fs.Close();
                        fs = null;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return ret;
        }
    }
}
