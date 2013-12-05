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
using Subgurim.Controles;

namespace ESMERALDA
{
    public partial class Search : ESMERALDAPage
    {
        protected void DoSearch_Experiment()
        {
            int i;
            DateTime startDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(txtStartDate.Text))
            {
                try
                {
                    startDate = DateTime.Parse(txtStartDate.Text);
                }
                catch (FormatException)
                {
                    ShowAlert("The start date is not properly formatted.");
                    return;
                }
            }
            DateTime endDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(txtEndDate.Text))
            {
                try
                {
                    endDate = DateTime.Parse(txtEndDate.Text);
                }
                catch (FormatException)
                {
                    ShowAlert("The end date is not properly formatted.");
                    return;
                }
            }
            string search_string = this.txtSearchByKeyword.Text.ToUpper();
            List<string> terms = this.GetSearchTerms(search_string);
            string bounds_string = this.searchCoords.Value;
            if (!string.IsNullOrEmpty(txtMinLatitude.Text) && !string.IsNullOrEmpty(txtMinLongitude.Text) && !string.IsNullOrEmpty(txtMaxLatitude.Text) && !string.IsNullOrEmpty(txtMaxLongitude.Text))
            {
                bounds_string = txtMinLatitude.Text + " " + txtMinLongitude.Text + " " + txtMaxLatitude.Text + " " + txtMaxLongitude.Text;
            }
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            List<Guid> inbounds_datasetids = new List<Guid>();
            string query = string.Empty;
            Dictionary<Guid, SearchResult> ret = new Dictionary<Guid, SearchResult>();
            bool has_bounds = !string.IsNullOrEmpty(bounds_string);
            char[] delim = new char[] { ' ' };
            string[] tokens = bounds_string.Split(delim);
            if (terms.Count > 0)
            {
                query = "SELECT dataset_id, metadata_value FROM v_ESMERALDA_search_keywords WHERE (IsPublic=1";
                if (IsAuthenticated && CurrentUser != null)
                {
                    query += " OR CreatedBy='" + CurrentUser.ID + "'";
                }
                query += ")";
                query = query + " AND (";
                query = query + "metadata_value LIKE '%" + terms[0] + "%'";
                for (int j = 1; j < terms.Count; j++)
                {
                    query += " OR metadata_value LIKE '%" + terms[j] + "%'";
                }
                query += ")";
            }
            else
            {
                query = "SELECT DISTINCT dataset_id FROM v_ESMERALDA_search_keywords";
            }
            SqlCommand querycmd = new SqlCommand
            {
                Connection = conn,
                CommandTimeout = 60,
                CommandType = CommandType.Text,
                CommandText = query
            };
            SqlDataReader reader = querycmd.ExecuteReader();
            while (reader.Read())
            {
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
                if (terms.Count > 0)
                {
                    string ds_value = reader["metadata_value"].ToString();
                    if (!result.MatchingKeywords.Contains(ds_value))
                    {
                        foreach (string s in terms)
                        {
                            if (ds_value.ToUpper().IndexOf(s) >= 0)
                            {
                                result.MatchingKeywords.Add(ds_value);
                                break;
                            }
                        }
                    }
                }
            }
            reader.Close();
            if (startDate > DateTime.MinValue)
            {
                if (endDate == DateTime.MinValue)
                    endDate = DateTime.Now;
                query = "SELECT DISTINCT entity_id FROM entity_datetime_map WHERE entity_id IN ('" + ret[ret.Keys.ElementAt(0)].DatasetID.ToString() + "'";
                for (int j = 1; j < ret.Count; j++)
                {
                    query += ", '" + ret[ret.Keys.ElementAt(j)].DatasetID.ToString() + "'";
                }
                query += ")";
                query += " AND timestamp >= '" + startDate.ToShortDateString() + "' AND timestamp <= '" + endDate.ToShortDateString() + "'";
                querycmd = new SqlCommand
                {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                reader = querycmd.ExecuteReader();
                List<SearchResult> keepers = new List<SearchResult>();
                while (reader.Read())
                {
                    Guid setid = new Guid(reader["entity_id"].ToString());
                    if (!ret.ContainsKey(setid))
                        continue;
                    SearchResult sr = ret[setid];
                    if (!keepers.Contains(sr))
                        keepers.Add(sr);
                }
                reader.Close();
                ret.Clear();
                foreach (SearchResult sr in keepers)
                {
                    ret.Add(sr.DatasetID, sr);
                }
            }
            if (has_bounds && ret.Count > 0)
            {
                tokens = bounds_string.Split(delim);
                query = "SELECT min_lat, min_lon, max_lat, max_lon, dataset_id FROM v_ESMERALDA_geospatial_search_data WHERE dataset_id IN ('" + ret[ret.Keys.ElementAt(0)].DatasetID.ToString() + "'";
                for (int j = 1; j < ret.Count; j++)
                {
                    query += ", '" + ret[ret.Keys.ElementAt(j)].DatasetID.ToString() + "'";
                }
                query += ")";
                string max_lat = string.Format("{0:0.00000}", double.Parse(tokens[0]));
                string max_lon = string.Format("{0:0.00000}", double.Parse(tokens[1]));
                string min_lat = string.Format("{0:0.00000}", double.Parse(tokens[2]));
                string min_lon = string.Format("{0:0.00000}", double.Parse(tokens[3]));
                string cond1 = "(" + min_lon + " <= max_lon)";
                string cond2 = "(" + max_lon + " >= min_lon)";
                string cond3 = "(" + max_lat + " >= min_lat)";
                string cond4 = "(" + min_lat + " <= max_lat)";
                query = query + " AND (min_lat IS NOT NULL)";
                query = query + " AND (" + cond1 + " AND " + cond2 + " AND " + cond3 + " AND " + cond4 + ")";
                querycmd = new SqlCommand
                {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                reader = querycmd.ExecuteReader();
                List<SearchResult> keepers = new List<SearchResult>();
                while (reader.Read())
                {
                    Guid setid = new Guid(reader["dataset_id"].ToString());
                    if (!ret.ContainsKey(setid))
                        continue;
                    SearchResult sr = ret[setid];
                    if (!keepers.Contains(sr))
                        keepers.Add(sr);
                }
                reader.Close();
                ret.Clear();
                foreach (SearchResult sr in keepers)
                {
                    ret.Add(sr.DatasetID, sr);
                }
            }
            if (ret.Count > 0)
            {
                query = "SELECT dataset_id, dataset_name, dataset_description, dataset_purpose, container_name, container_description, container_id, min_lat, min_lon, max_lat, max_lon FROM v_ESMERALDA_dataset_metadata WHERE dataset_id IN ('" + ret[ret.Keys.ElementAt(0)].DatasetID.ToString() + "'";
                for (int j = 1; j < ret.Count; j++)
                {
                    query += ", '" + ret[ret.Keys.ElementAt(j)].DatasetID.ToString() + "'";
                }
                query += ")";
                querycmd = new SqlCommand
                {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                reader = querycmd.ExecuteReader();
                List<SearchResult> keepers = new List<SearchResult>();
                while (reader.Read())
                {
                    Guid setid = new Guid(reader["dataset_id"].ToString());
                    SearchResult result = ret[setid];
                    result.DatasetName = reader["dataset_name"].ToString();
                    result.DatasetBriefDescription = reader["dataset_purpose"].ToString();
                    result.DatasetDescription = reader["dataset_description"].ToString();
                    result.ContainerID = new Guid(reader["container_id"].ToString());
                    result.ContainerName = reader["container_name"].ToString();
                    if (!reader.IsDBNull(reader.GetOrdinal("min_lon")))
                    {
                        result.min_lat = double.Parse(reader["min_lat"].ToString());
                        result.min_lon = double.Parse(reader["min_lon"].ToString());
                        result.max_lat = double.Parse(reader["max_lat"].ToString());
                        result.max_lon = double.Parse(reader["max_lon"].ToString());
                    }
                }
                reader.Close();
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
                        if (curr.ContainerName.ToUpper().IndexOf(s) >= 0)
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
                    if (curr.Score != 0 || terms.Count == 0)
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
                    TableHeaderCell thc = new TableHeaderCell
                    {
                        Text = "Dataset"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell
                    {
                        Text = "Description"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell
                    {
                        Text = "Folder"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell
                    {
                        Text = "Fields"
                    };
                    thr.Cells.Add(thc);
                    thc = new TableHeaderCell
                    {
                        Text = "Score"
                    };
                    thr.Cells.Add(thc);
                    this.tblSearchByKeywordResults.Rows.Add(thr);
                    for (i = 0; i < ranked_results.Count; i++)
                    {
                        TableRow tr = new TableRow();
                        TableCell tc = new TableCell
                        {
                            Text = "<a href='ViewDataset.aspx?DATASETID=" + ranked_results[i].DatasetID.ToString() + "'>" + ranked_results[i].DatasetName + "</a>"
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell
                        {
                            Text = ranked_results[i].DatasetBriefDescription
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell
                        {
                            Text = "<a href='EditContainer.aspx?CONTAINERID=" + ranked_results[i].ContainerID.ToString() + "'>" + ranked_results[i].ContainerName + "</a>"
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell
                        {
                            Text = ranked_results[i].DisplayFields
                        };
                        tr.Cells.Add(tc);
                        tc = new TableCell
                        {
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
            try
            {
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
            }
            catch (Exception ex)
            {
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

        protected List<string> GetSearchTerms(string txt)
        {
            char[] delim = new char[] { ' ' };
            string[] tokens = txt.Split(delim);
            List<string> ret = new List<string>();
            foreach (string s in tokens)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                ret.AddRange(tokens);
            }
            return ret;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // this.DoSearch();
            Page.Title = GetAppString("appstring_shortname") + " - Search";
            this.DoSearch_Experiment();
            // base.ClientScript.RegisterStartupScript(base.GetType(), "PopulateMap", "<script language='JavaScript'>initializeMap();</script>");
        }

        protected void PopulateMapView(List<SearchResult> results, SqlConnection conn)
        {
            string map_string = string.Empty;
            Random r = new Random();
            List<string> ids = new List<string>();
            StringBuilder startup = new StringBuilder();
            StringBuilder init_map = new StringBuilder();
            map1.resetPolygon();
            map1.resetMarkers();
            map1.resetListeners();

            GControl control = new GControl(GControl.preBuilt.LargeMapControl);
            map1.Add(control);

            GMapUIOptions options = new GMapUIOptions();
            options.maptypes_hybrid = true;
            options.keyboard = true;
            options.maptypes_physical = false;
            options.zoom_scrollwheel = true;

            map1.Add(new GMapUI(options));

            init_map.Append("function initializeMap() {");
            init_map.Append("var drawingManager = new google.maps.drawing.DrawingManager({");
            init_map.Append("drawingControl: true,");
            init_map.Append("drawingControlOptions: {");
            init_map.Append("position: google.maps.ControlPosition.TOP_CENTER,");
            init_map.Append("drawingModes: [google.maps.drawing.OverlayType.RECTANGLE]");
            init_map.Append("},");
            init_map.Append("circleOptions: {");
            init_map.Append("fillColor: '#ffff00',");
            init_map.Append("fillOpacity: 1,");
            init_map.Append("strokeWeight: 5,");
            init_map.Append("clickable: false,");
            init_map.Append("zIndex: 1,");
            init_map.Append("editable: true");
            init_map.Append("}");
            init_map.Append("});");
            init_map.Append("drawingManager.setMap(" + map1.GMap_Id + ");");
            init_map.Append("}");

            startup.Append("var polys = [];");
            startup.Append("var theMap=" + map1.GMap_Id + ";");
            startup.Append("function toggleOverlay(incolor) {");
            startup.Append("var i=0; for(i=0; i < polys.length; i++) {");
            startup.Append("if(polys[i].fillColor == incolor) { polys[i].setVisible(true); } else { polys[i].setVisible(false); }");
            startup.Append("}");
            startup.Append("}");
            startup.Append("function initPolys() {");
            List<string> colors = new List<string>();
            double min_lat = 0;
            double min_lon = 0;
            double max_lat = 0;
            double max_lon = 0;            
            foreach (SearchResult gsr in results)
            {
                if (double.IsNaN(gsr.min_lat))
                    continue;
                string id_string = gsr.DatasetID.ToString().Replace("{", "").Replace("}", "");
                GPolygon p = new GPolygon();
                int seed_int = r.Next(0, 0x1000000);
                string color = Utils.ToColor(seed_int);
                while (colors.Contains(color))
                {
                    seed_int = r.Next(0, 0x1000000);
                    color = Utils.ToColor(seed_int);
                }
                colors.Add(color);
                p.strokeColor = color;
                p.fillColor = color;
                p.fillOpacity = 0;
                min_lat = Math.Min(min_lat, gsr.min_lat);
                min_lon = Math.Min(min_lon, gsr.min_lon);
                max_lat = Math.Max(max_lat, gsr.max_lat);
                max_lon = Math.Max(max_lon, gsr.max_lon);
                p.points.Add(new GLatLng(gsr.min_lat, gsr.min_lon));
                p.points.Add(new GLatLng(gsr.max_lat, gsr.min_lon));
                p.points.Add(new GLatLng(gsr.max_lat, gsr.max_lon));
                p.points.Add(new GLatLng(gsr.min_lat, gsr.max_lon));
                map1.Add(p);

                p = new GPolygon();
                p.strokeColor = color;
                p.fillColor = color;
                p.fillOpacity = 0.5;
                p.points.Add(new GLatLng(gsr.min_lat, gsr.min_lon));
                p.points.Add(new GLatLng(gsr.max_lat, gsr.min_lon));
                p.points.Add(new GLatLng(gsr.max_lat, gsr.max_lon));
                p.points.Add(new GLatLng(gsr.min_lat, gsr.max_lon));
                // p.ID = "line-" + id_string;
                // map1.Add(p);

                GMarker m = new GMarker(new GLatLng(gsr.min_lat + ((gsr.max_lat - gsr.min_lat) / 2), gsr.min_lon + ((gsr.max_lon - gsr.min_lon) / 2)));
                m.options = new GMarkerOptions();
                string url = "ViewDataset.aspx?DATASETID=" + gsr.DatasetID.ToString();
                m.options.title = gsr.DatasetName;
                string markerID = p.PolygonID;
                startup.Append(p.ToString(map1.GMap_Id));
                string js = string.Format("{0}.setVisible(false);polys.push({0});", markerID);
                startup.Append(js);
                map1.Add(m);
                string markup = "<h3>";
                if (!string.IsNullOrEmpty(gsr.ContainerName))
                    markup += gsr.ContainerName + ": ";
                markup += gsr.DatasetName + "</h3><p>" + gsr.DatasetBriefDescription + "</p><p>" + gsr.DatasetDescription + "</p>";
                js = string.Format(@"function() {{ toggleOverlay('{0}'); setDataNotes('" + markup + "');}}", color);
                map1.Add(new GListener(m.ID, GListener.Event.mouseover, js));
            }
            map1.setCenter(new GLatLng(min_lat + ((max_lat - min_lat) / 2), min_lon + ((max_lon - min_lon) / 2)), 1);
            conn.Close();            
            startup.Append("}");
            map1.Add("initPolys();", true);            
            map1.Add(startup.ToString());
            map1.Add(init_map.ToString());
        }
    }
}