using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using System.Net;
using System.Text;
using System.IO;
using HtmlAgilityPack;

namespace ESMERALDA
{
    public partial class Search : ESMERALDAPage
    {
        protected void DoSearch()
        {
            int i;
            string search_string = this.txtSearchByKeyword.Text.ToUpper();
            List<string> terms = this.GetSearchTerms(search_string);
            string bounds_string = this.searchCoords.Value;
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            List<Guid> inbounds_datasetids = new List<Guid>();
            List<string> fields = this.GetSearchFieldList(conn);
            string query = string.Empty;
            Dictionary<Guid, SearchResult> ret = new Dictionary<Guid, SearchResult>();
            bool has_bounds = !string.IsNullOrEmpty(bounds_string);
            char[] delim = new char[] { ' ' };
            string[] tokens = bounds_string.Split(delim);
            query = "SELECT dataset_id, dataset_name, brief_description, dataset_description, project_id, project_name, field_name, latlon_sql_column_name, latlon_sql_table_name, latlon_metric_id, latlon_database_name, dataset_keyword, project_keyword FROM v_geospatial_search_data WHERE (IsPublic=1";
            if (IsAuthenticated && CurrentUser != null)
            {
                query += " OR CreatedBy='" + CurrentUser.ID + "'";
            }
            query += ")";
            if (has_bounds)
            {
                query = query + " AND (min_lat IS NOT NULL";
                query = query + " AND ((min_lat >= " + tokens[2] + " AND min_lat <= " + tokens[0] + ") OR (" + tokens[2] + " >= min_lat AND " + tokens[2] + " <= max_lat))";
                query = query + " AND ((min_lon >= " + tokens[3] + " AND min_lon <= " + tokens[1] + ") OR (" + tokens[3] + " >= min_lon AND " + tokens[3] + " <= max_lon)))";
            }
            if (fields.Count > 0)
            {
                query = query + " AND (";
                i = 0;
                while (i < fields.Count)
                {
                    string clause = string.Empty;
                    if (i > 0)
                    {
                        clause = clause + " OR";
                    }
                    clause = clause + " (" + fields[i] + " LIKE '%" + terms[0] + "%'";
                    for (int j = 1; j < terms.Count; j++)
                    {
                        clause = clause + " OR " + fields[i] + " LIKE '%" + terms[j] + "%'";
                    }
                    clause = clause + ")";
                    query = query + clause;
                    i++;
                }
                query = query + ")";
            }
            SqlCommand querycmd = new SqlCommand {
                Connection = conn,
                CommandTimeout = 60,
                CommandType = CommandType.Text,
                CommandText = query
            };
            SqlDataReader reader = querycmd.ExecuteReader();
            while (reader.Read())
            {
                string kw;
                SearchResult result = null;
                Guid setid = new Guid(reader["dataset_id"].ToString());
                if (ret.ContainsKey(setid))
                {
                    result = ret[setid];
                }
                if (result == null)
                {
                    result = new SearchResult();
                    ret.Add(setid, result);
                }
                result.DatasetID = setid;
                result.DatasetName = reader["dataset_name"].ToString();
                result.DatasetBriefDescription = reader["brief_description"].ToString();
                result.DatasetDescription = reader["dataset_description"].ToString();
                result.ProjectID = new Guid(reader["project_id"].ToString());
                result.ProjectName = reader["project_name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("field_name")))
                {
                    kw = reader["field_name"].ToString();
                    if (!result.MatchingFields.Contains(kw))
                    {
                        result.MatchingFields.Add(kw);
                    }
                }
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_keyword")))
                {
                    kw = reader["dataset_keyword"].ToString();
                    if (!result.MatchingKeywords.Contains(kw))
                    {
                        result.MatchingKeywords.Add(kw);
                    }
                }
                if (!reader.IsDBNull(reader.GetOrdinal("project_keyword")))
                {
                    kw = reader["project_keyword"].ToString();
                    if (!result.MatchingKeywords.Contains(kw))
                    {
                        result.MatchingKeywords.Add(kw);
                    }
                }
                if (!reader.IsDBNull(reader.GetOrdinal("latlon_sql_column_name")))
                {
                    result.latlon_sql_column_name.Add(reader["latlon_sql_column_name"].ToString());
                }
                if (!reader.IsDBNull(reader.GetOrdinal("latlon_sql_table_name")))
                {
                    result.latlon_sql_table_name = reader["latlon_sql_table_name"].ToString();
                }
                if (!reader.IsDBNull(reader.GetOrdinal("latlon_metric_id")))
                {
                    result.latlon_metric_id.Add(new Guid(reader["latlon_metric_id"].ToString()));
                }
                if (!reader.IsDBNull(reader.GetOrdinal("latlon_database_name")))
                {
                    result.latlon_database_name = reader["latlon_database_name"].ToString();
                }
            }
            reader.Close();
            if (ret.Count > 0)
            {
                Guid lat_guid = new Guid("c8a09a60-e42e-4d12-96ef-9f54a707b255");
                Guid lon_guid = new Guid("22ddbcd9-e1ad-4348-823b-542e6577b735");
                if (has_bounds)
                {
                    foreach (Guid g in ret.Keys)
                    {
                        SearchResult gsr = ret[g];
                        string lat_field = string.Empty;
                        string lon_field = string.Empty;
                        i = 0;
                        while (i < gsr.latlon_metric_id.Count)
                        {
                            if (gsr.latlon_metric_id[i] == lat_guid)
                            {
                                lat_field = gsr.latlon_sql_column_name[i];
                            }
                            else if (gsr.latlon_metric_id[i] == lon_guid)
                            {
                                lon_field = gsr.latlon_sql_column_name[i];
                            }
                            i++;
                        }
                        string cmd = "SELECT TOP(1) " + lat_field + " AS Latitude, " + lon_field + " AS Longitude FROM " + gsr.latlon_database_name + ".dbo." + gsr.latlon_sql_table_name;
                        cmd = cmd + " WHERE " + lat_field + " >= " + tokens[2];
                        cmd = cmd + " AND " + lon_field + " >= " + tokens[3];
                        cmd = cmd + " AND " + lat_field + " <= " + tokens[0];
                        cmd = cmd + " AND " + lon_field + " <= " + tokens[1];
                        reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                gsr.geo_match = true;
                            }
                        }
                        reader.Close();
                    }
                    Dictionary<Guid, SearchResult> ret_tmp = new Dictionary<Guid, SearchResult>();
                    foreach (Guid g in ret.Keys)
                    {
                        if (ret[g].geo_match)
                        {
                            ret_tmp.Add(g, ret[g]);
                        }
                    }
                    ret = ret_tmp;
                }
                List<SearchResult> ranked_results = new List<SearchResult>();
                foreach (Guid g in ret.Keys)
                {
                    SearchResult curr = ret[g];
                    foreach (string s in terms)
                    {
                        if (curr.DatasetBriefDescription.ToUpper().IndexOf(s) >= 0)
                        {
                            curr.Score++;
                        }
                        if (curr.DatasetDescription.ToUpper().IndexOf(s) >= 0)
                        {
                            curr.Score++;
                        }
                        if (curr.DatasetName.ToUpper().IndexOf(s) >= 0)
                        {
                            curr.Score++;
                        }
                        if (curr.ProjectName.ToUpper().IndexOf(s) >= 0)
                        {
                            curr.Score++;
                        }
                        foreach (string mf in curr.MatchingFields)
                        {
                            if (mf.ToUpper().IndexOf(s) >= 0)
                            {
                                curr.Score++;
                                if (string.IsNullOrEmpty(curr.DisplayFields))
                                {
                                    curr.DisplayFields = mf;
                                }
                                else
                                {
                                    curr.DisplayFields = curr.DisplayFields + ", " + mf;
                                }
                            }
                        }
                        foreach (string mf in curr.MatchingKeywords)
                        {
                            if (mf.ToUpper().IndexOf(s) >= 0)
                            {
                                curr.Score++;
                                if (string.IsNullOrEmpty(curr.DisplayFields))
                                {
                                    curr.DisplayFields = mf;
                                }
                                else
                                {
                                    curr.DisplayFields = curr.DisplayFields + ", " + mf;
                                }
                            }
                        }
                    }
                    if (curr.Score != 0)
                    {
                        bool added = false;
                        i = 0;
                        while (i < ranked_results.Count)
                        {
                            if (ranked_results[i].Score < curr.Score)
                            {
                                ranked_results.Insert(i, curr);
                                added = true;
                                break;
                            }
                            i++;
                        }
                        if (!added)
                        {
                            ranked_results.Add(curr);
                        }
                    }
                }
                if (ranked_results.Count <= 0)
                {
                    base.ShowAlert("No results found.");
                }
                else
                {
                    TableHeaderRow thr = new TableHeaderRow();
                    TableHeaderCell thc = new TableHeaderCell {
                        Text = "Dataset"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell {
                        Text = "Description"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell {
                        Text = "Project"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell {
                        Text = "Fields"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell {
                        Text = "Score"
                    };
                    thr.Cells.Add(thc);
                    this.tblSearchByKeywordResults.Rows.Add(thr);
                    for (i = 0; i < ranked_results.Count; i++)
                    {
                        TableRow tr = new TableRow();
                        TableCell tc = new TableCell {
                            Text = "<a href='ViewDataset.aspx?DATASETID=" + ranked_results[i].DatasetID.ToString() + "'>" + ranked_results[i].DatasetName + "</a>"
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell {
                            Text = ranked_results[i].DatasetBriefDescription
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell {
                            Text = "<a href='EditProject.aspx?PROJECTID=" + ranked_results[i].ProjectID.ToString() + "'>" + ranked_results[i].ProjectName + "</a>"
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell {
                            Text = ranked_results[i].DisplayFields
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell {
                            Text = ranked_results[i].Score.ToString()
                        };
                        tr.Cells.Add(tc);
                        this.tblSearchByKeywordResults.Rows.Add(tr);
                    }
                    this.tblSearchByKeywordResults.Visible = true;
                    this.PopulateMapView(ranked_results, conn);
                    conn.Close();
                }
            }
            if (!string.IsNullOrEmpty(search_string))
            {
                List<SearchResult> external = SearchBCODMO(search_string);
                // List<SearchResult> external_nodc = SearchNODC(search_string);
                tblExternalSearchResults.Rows.Clear();
                TableHeaderRow thr = new TableHeaderRow();
                TableHeaderCell thc = new TableHeaderCell();
                thc.Text = "Source";
                thr.Cells.Add(thc);
                thc = new TableHeaderCell();
                thc.Text = "Dataset Name";
                thr.Cells.Add(thc);
                thc = new TableHeaderCell();
                thc.Text = "Dataset Description";
                thr.Cells.Add(thc);
                tblExternalSearchResults.Rows.Add(thr);
                foreach (SearchResult res in external)
                {
                    TableRow tr = new TableRow();
                    TableCell td = new TableCell();
                    td.Text = res.SourceName;
                    tr.Cells.Add(td);
                    td = new TableCell();
                    td.Text = "<a href='" + res.URL + "' target='_blank'>" + res.DatasetName + "</a>";
                    tr.Cells.Add(td);
                    td = new TableCell();
                    td.Text = res.DatasetBriefDescription;
                    tr.Cells.Add(td);
                    tblExternalSearchResults.Rows.Add(tr);
                }
            }
        }

        protected List<SearchResult> SearchBCODMO(string searchstring)
        {
            List<SearchResult> ret = new List<SearchResult>();
            WebRequest req = WebRequest.Create("http://osprey.bco-dmo.org/dataset.cfm?flag=search&sortby=name");
            string postData = "searchFor=" + searchstring;

            byte[] send = Encoding.Default.GetBytes(postData);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = send.Length;

            Stream sout = req.GetRequestStream();
            sout.Write(send, 0, send.Length);
            sout.Flush();
            sout.Close();

            WebResponse res = req.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string returnvalue = sr.ReadToEnd();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(returnvalue);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='entry']");
            foreach (HtmlNode node in nodes)
            {
                HtmlNodeCollection table_nodes = node.SelectNodes("table");
                if(table_nodes == null)
                    continue;
                if (table_nodes.Count > 0)
                {
                    foreach (HtmlNode table_node in table_nodes)
                    {
                        HtmlNodeCollection rows = table_node.SelectNodes("tr");
                        if(rows == null)
                            continue;
                        foreach (HtmlNode row in rows)
                        {
                            HtmlNodeCollection cells = row.SelectNodes("td");
                            if(cells == null)
                                continue;
                            if (cells.Count < 2)
                                continue;
                            HtmlNode link = cells[0].SelectSingleNode("a");
                            if (link != null)
                            {
                                SearchResult r = new SearchResult();
                                r.DatasetName = link.InnerText;
                                r.DatasetBriefDescription = cells[1].InnerText;
                                r.isExternal = true;
                                r.SourceName = "BCO/DMO";
                                foreach (HtmlAttribute attr in link.Attributes)
                                {
                                    if (attr.Name == "href")
                                    {
                                        r.URL = attr.Value;
                                        break;
                                    }
                                }
                                ret.Add(r);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        protected List<SearchResult> SearchNODC(string searchstring)
        {
            List<SearchResult> ret = new List<SearchResult>();
            WebRequest req = WebRequest.Create("http://www.nodc.noaa.gov/cgi-bin/OAS/prd/text/query");
            string postData = "query=" + searchstring;

            byte[] send = Encoding.Default.GetBytes(postData);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = send.Length;

            Stream sout = req.GetRequestStream();
            sout.Write(send, 0, send.Length);
            sout.Flush();
            sout.Close();

            WebResponse res = req.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string returnvalue = sr.ReadToEnd();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(returnvalue);
            HtmlNodeCollection forms = document.DocumentNode.SelectNodes("//form");
            string fetch_url = string.Empty;
            foreach (HtmlNode form in forms)
            {
                foreach (HtmlAttribute attr in form.Attributes)
                {
                    if (attr.Name == "action" && attr.Value.IndexOf("query/response") > 0)
                    {
                        fetch_url = "http://www.nodc.noaa.gov" + attr.Value;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(fetch_url))
            {
                return ret;
            }

            req = WebRequest.Create(fetch_url);
            postData = "query=" + searchstring;

            send = Encoding.Default.GetBytes(postData);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";            
            req.ContentLength = send.Length;

            sout = req.GetRequestStream();
            sout.Write(send, 0, send.Length);
            sout.Flush();
            sout.Close();

            res = req.GetResponse();
            sr = new StreamReader(res.GetResponseStream());
            returnvalue = sr.ReadToEnd();
            document = new HtmlDocument();
            document.LoadHtml(returnvalue);
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='entry']");
            foreach (HtmlNode node in nodes)
            {
                HtmlNodeCollection table_nodes = node.SelectNodes("table");
                if (table_nodes == null)
                    continue;
                if (table_nodes.Count > 0)
                {
                    foreach (HtmlNode table_node in table_nodes)
                    {
                        HtmlNodeCollection rows = table_node.SelectNodes("tr");
                        if (rows == null)
                            continue;
                        foreach (HtmlNode row in rows)
                        {
                            HtmlNodeCollection cells = row.SelectNodes("td");
                            if (cells == null)
                                continue;
                            if (cells.Count < 2)
                                continue;
                            HtmlNode link = cells[0].SelectSingleNode("a");
                            if (link != null)
                            {
                                SearchResult r = new SearchResult();
                                r.DatasetName = link.InnerText;
                                r.DatasetBriefDescription = cells[1].InnerText;
                                r.isExternal = true;
                                r.SourceName = "BCO/DMO";
                                foreach (HtmlAttribute attr in link.Attributes)
                                {
                                    if (attr.Name == "href")
                                    {
                                        r.URL = attr.Value;
                                        break;
                                    }
                                }
                                ret.Add(r);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        protected List<string> GetSearchFieldList(SqlConnection conn)
        {
            List<string> ret = new List<string>();
            SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = "SELECT TOP(1) * FROM v_Search_Metadata" }.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    ret.Add(reader.GetName(i));
                }
            }
            reader.Close();
            return ret;
        }

        protected List<string> GetSearchTerms(string txt)
        {
            char[] delim = new char[] { ' ' };
            string[] tokens = txt.Split(delim);
            List<string> ret = new List<string>();
            ret.AddRange(tokens);
            return ret;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.DoSearch();
            base.ClientScript.RegisterStartupScript(base.GetType(), "PopulateMap", "<script language='JavaScript'>initializeMap();</script>");
        }

        protected void PopulateMapView(List<SearchResult> results, SqlConnection conn)
        {
            string map_string = string.Empty;
            Guid lat_guid = new Guid("c8a09a60-e42e-4d12-96ef-9f54a707b255");
            Guid lon_guid = new Guid("22ddbcd9-e1ad-4348-823b-542e6577b735");
            foreach (SearchResult gsr in results)
            {
                string curr_hull = string.Empty;
                string lat_field = string.Empty;
                string lon_field = string.Empty;
                int i = 0;
                while (i < gsr.latlon_metric_id.Count)
                {
                    if (gsr.latlon_metric_id[i] == lat_guid)
                    {
                        lat_field = gsr.latlon_sql_column_name[i];
                    }
                    else if (gsr.latlon_metric_id[i] == lon_guid)
                    {
                        lon_field = gsr.latlon_sql_column_name[i];
                    }
                    i++;
                }
                if (((!string.IsNullOrEmpty(lat_field) && !string.IsNullOrEmpty(lon_field)) && !string.IsNullOrEmpty(gsr.latlon_database_name)) && !string.IsNullOrEmpty(gsr.latlon_sql_table_name))
                {
                    string cmd = "SELECT min_lat, min_lon, max_lat, max_lon FROM dataset_metadata WHERE dataset_id='" + gsr.DatasetID.ToString() + "'";
                    SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                    List<Point> points = new List<Point>();
                    double min_lat = 0.0;
                    double min_lon = 0.0;
                    double max_lat = 0.0;
                    double max_lon = 0.0;
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            min_lat = double.Parse(reader[0].ToString());
                            min_lon = double.Parse(reader[1].ToString());
                            max_lat = double.Parse(reader[2].ToString());
                            max_lon = double.Parse(reader[3].ToString());
                            points.Add(new Point(double.Parse(reader[0].ToString()), double.Parse(reader[1].ToString())));
                            points.Add(new Point(double.Parse(reader[2].ToString()), double.Parse(reader[1].ToString())));
                            points.Add(new Point(double.Parse(reader[2].ToString()), double.Parse(reader[3].ToString())));
                            points.Add(new Point(double.Parse(reader[0].ToString()), double.Parse(reader[3].ToString())));
                        }
                    }
                    reader.Close();
                    Point[] bounding = points.ToArray<Point>();
                    if (bounding.Length != 0)
                    {
                        string url = "ViewDataset.aspx?DATASETID=" + gsr.DatasetID.ToString();
                        string comment = "<strong>" + gsr.DatasetName + "</strong><br/><p>" + gsr.DatasetDescription + "</p>";
                        curr_hull = string.Concat(new object[] { Utils.RandomColor(), "~", comment, "~", url, "~", bounding[0].x, ",", bounding[0].y });
                        for (i = 1; i < bounding.Length; i++)
                        {
                            curr_hull = string.Concat(new object[] { curr_hull, ";", bounding[i].x, ",", bounding[i].y });
                        }
                        if (string.IsNullOrEmpty(map_string))
                        {
                            map_string = curr_hull;
                        }
                        else
                        {
                            map_string = map_string + "|" + curr_hull;
                        }
                    }
                }
            }
            this.mapdatasets.Value = map_string;
            conn.Close();
            base.ClientScript.RegisterStartupScript(base.GetType(), "PopulateMap", "<script language='JavaScript'>initializeMap();</script>");
        }
    }
}