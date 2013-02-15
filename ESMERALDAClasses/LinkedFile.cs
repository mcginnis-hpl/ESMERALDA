using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ESMERALDAClasses
{
    public class LinkedFile : EsmeraldaEntity
    {
        public string FilePath;
        public string Name;
        public string Description;
        public string BriefDescription;
        public int Version;
        public string MIMEType;
        public Project ParentProject;

        public LinkedFile()
            : base()
        {
            Name = string.Empty;
            FilePath = string.Empty;
            Description = string.Empty;
            BriefDescription = string.Empty;
            Version = -1;
            MIMEType = string.Empty;
            ParentProject = null;
        }

        public override void Load(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_LoadLinkedFile";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@inID", ID));
            SqlDataReader reader = query.ExecuteReader();
            Guid enteredbyid = Guid.Empty;
            Guid projectid = Guid.Empty;
            while (reader.Read())
            {
                if (!reader.IsDBNull(reader.GetOrdinal("file_id")))
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("file_name")))
                        Name = reader["file_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("brief_description")))
                        BriefDescription = reader["brief_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("file_description")))
                        Description = reader["file_description"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("version")))
                        Version = int.Parse(reader["version"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("project_id")))
                        projectid = new Guid(reader["project_id"].ToString());
                    if (!reader.IsDBNull(reader.GetOrdinal("mime_type")))
                        MIMEType = reader["mime_type"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("file_path")))
                        FilePath = reader["file_path"].ToString();
                }
            }
            reader.Close();          
            base.Load(conn);
            if (projectid != Guid.Empty)
            {
                ParentProject = new Project();
                ParentProject.Load(conn, projectid);
            }
        }

        public override void Save(SqlConnection conn)
        {
            SqlCommand query = new SqlCommand();
            if (ID == Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            query.CommandType = CommandType.StoredProcedure;
            query.CommandText = "sp_WriteLinkedFile";
            query.CommandTimeout = 60;
            query.Connection = conn;
            query.Parameters.Add(new SqlParameter("@infile_id", ID));
            query.Parameters.Add(new SqlParameter("@infile_name", Name));
            query.Parameters.Add(new SqlParameter("@infile_path", FilePath));
            query.Parameters.Add(new SqlParameter("@inbrief_description", BriefDescription));
            query.Parameters.Add(new SqlParameter("@infile_description", Description));
            if (ParentProject != null)
            {
                query.Parameters.Add(new SqlParameter("@inproject_id", ParentProject.ID));
            }
            query.Parameters.Add(new SqlParameter("@inversion", Version));
            query.Parameters.Add(new SqlParameter("@inmime_type", MIMEType));
            query.ExecuteNonQuery();

            base.Save(conn);
        }
    }
}
