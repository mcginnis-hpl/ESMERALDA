using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using System.Text;
using System.Web.UI.DataVisualization.Charting;

namespace ESMERALDA
{
    public partial class VisualizeView : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = null;
            string viewid = string.Empty;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                {
                    viewid = base.Request.Params[i];
                }
            }
            if (!string.IsNullOrEmpty(viewid))
            {
                working = (ESMERALDAClasses.View)base.GetSessionValueCrossPage("View-" + viewid);
                if(working != null)
                    PopulateControls(working);
            }
            if (!IsPostBack)
            {
                // bind chart type names to ddl
                comboGraphType.DataSource = Enum.GetNames(typeof(System.Web.UI.DataVisualization.Charting.SeriesChartType));
                comboGraphType.DataBind();
            }
        }

        protected void PopulateControls(ESMERALDAClasses.View inView)
        {
            if (listAvailableFields.Items.Count == 0)
            {
                foreach (QueryField f in inView.Header)
                {
                    ListItem li = new ListItem();
                    li.Text = f.Name;
                    li.Value = f.SQLColumnName;
                    listAvailableFields.Items.Add(li);
                }
            }
            listSelectedFields.Items.Clear();
            if (!string.IsNullOrEmpty(fieldValues.Value))
            {
                char[] delim = {'|'};
                string[] tokens = fieldValues.Value.Split(delim);
                string[] colors = colorValues.Value.Split(delim);
                string[] series = seriesValues.Value.Split(delim);
                for(int i=0; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    foreach (QueryField f in inView.Header)
                    {
                        if (f.SQLColumnName == t)
                        {
                            ListItem li = new ListItem();
                            li.Text = f.Name + ": " + colors[i];
                            if (series[i] == "1")
                            {
                                li.Text += "*";
                            }
                            li.Value = f.SQLColumnName;
                            listSelectedFields.Items.Add(li);                            
                            break;
                        }
                    }
                }
            }
        }
        
        protected void PopulateData_MS(ESMERALDAClasses.View working, string plot_type, List<string> fields, List<string> colors, List<bool> series)
        {
            string ret = string.Empty;
            string dbname = working.SourceData.ParentContainer.database_name;
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);            

            DataTable dt = working.GetDataTable(conn);            
            QueryField f = working.GetFieldBySQLName(fields[0]);
            string x_axis = f.SQLColumnName;
            msChart.Series.Clear();
            msChart.Legends.Clear();
            msChart.ChartAreas["ChartArea1"].AxisX.Title = f.Name;
            if (f.DBType == Field.FieldType.Text)
            {
                msChart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
            }
            if (f.DBType == Field.FieldType.Text || f.DBType == Field.FieldType.DateTime)
            {
                msChart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Angle = 90;
            }
            Series s = null;
            bool has_series = false;
            for (int i = 1; i < fields.Count; i++)
            {
                if (series[i])
                {
                    has_series = true;
                    break;
                }
            }

            if (has_series)
            {
                Random r = new Random();
                QueryField series_field = null;
                for (int i = 0; i < series.Count; i++)
                {
                    if(series[i])
                        series_field = working.GetFieldBySQLName(fields[i]);
                }

                List<string> series_list = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    if (!series_list.Contains(dr[series_field.SQLColumnName].ToString()))
                        series_list.Add(dr[series_field.SQLColumnName].ToString());
                }
                for(int i=1; i < fields.Count; i++)
                {
                    if(fields[i] == series_field.SQLColumnName)
                        continue;
                    f = working.GetFieldBySQLName(fields[i]);  
                    msChart.ChartAreas["ChartArea1"].AxisY.Title = f.Name;                                        
                    Dictionary<string, Series> chart_series = new Dictionary<string, Series>();
                    foreach(DataRow dr in dt.Rows)
                    {
                        s = null;
                        if (!chart_series.ContainsKey((string)dr[series_field.SQLColumnName]))
                        {
                            s = new Series();
                            s.Name = (string)dr[series_field.SQLColumnName];
                            s.ChartType = (System.Web.UI.DataVisualization.Charting.SeriesChartType)Enum.Parse(typeof(System.Web.UI.DataVisualization.Charting.SeriesChartType), comboGraphType.SelectedValue);
                            s.XValueMember = x_axis;
                            s.YValueMembers = f.SQLColumnName;
                            int seed_int = r.Next(0, 0x1000000);
                            s.Color = System.Drawing.ColorTranslator.FromHtml(Utils.ToColor(seed_int));
                            chart_series.Add(s.Name, s);
                        }
                        else
                        {
                            s = chart_series[(string)dr[series_field.SQLColumnName]];
                        }
                        s.Points.AddXY(dr[x_axis], dr[f.SQLColumnName]);
                    }
                    foreach (string s1 in series_list)
                    {
                        if (!chart_series.ContainsKey(s1))
                        {
                            s = new Series();
                            s.Name = s1;
                            s.ChartType = (System.Web.UI.DataVisualization.Charting.SeriesChartType)Enum.Parse(typeof(System.Web.UI.DataVisualization.Charting.SeriesChartType), comboGraphType.SelectedValue);
                            s.XValueMember = x_axis;
                            s.YValueMembers = f.SQLColumnName;
                            int seed_int = r.Next(0, 0x1000000);
                            s.Color = System.Drawing.ColorTranslator.FromHtml(Utils.ToColor(seed_int));
                            chart_series.Add(s.Name, s);
                        }
                        s = chart_series[s1];
                        msChart.Series.Add(s);
                    }
                }
            }
            else
            {
                msChart.DataSource = dt;            
                for (int i = 1; i < fields.Count; i++)
                {
                    f = working.GetFieldBySQLName(fields[i]);
                    s = new Series();
                    s.Name = x_axis + " v " + f.SQLColumnName;
                    s.ChartType = (System.Web.UI.DataVisualization.Charting.SeriesChartType)Enum.Parse(typeof(System.Web.UI.DataVisualization.Charting.SeriesChartType), comboGraphType.SelectedValue);                    
                    s.XValueMember = x_axis;
                    s.YValueMembers = f.SQLColumnName;
                    msChart.ChartAreas["ChartArea1"].AxisY.Title = f.Name;
                    s.Color = System.Drawing.ColorTranslator.FromHtml(colors[0]);
                    msChart.Series.Add(s);
                }
            }
            int width = Request.Browser.ScreenPixelsWidth;
            int height = Request.Browser.ScreenPixelsHeight;
            char[] delim = {','};
            if (!string.IsNullOrEmpty(pageWidth.Value))
            {
                try{
                    string[] tokens = pageWidth.Value.Split(delim);
                    if (tokens.Length > 1)
                    {
                        width = int.Parse(tokens[0]);
                        height = int.Parse(tokens[1]);
                    }                
                }
                catch(FormatException)
                {
                    width = Request.Browser.ScreenPixelsWidth;
                    height = Request.Browser.ScreenCharactersHeight;
                }
            }
            msChart.Width = Unit.Pixel((80 * width) / 100);
            msChart.Height = Unit.Pixel((80 * height) / 100);
            msChart.Legends.Add(new Legend());
            if (!has_series)
            {
                msChart.DataBind();
            }
            conn.Close();
        }

        protected void btnCreateGraph_Click(object sender, EventArgs e)
        {
            string plot_type = comboGraphType.SelectedValue;
            ESMERALDAClasses.View working = null;
            string viewid = string.Empty;
            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                {
                    viewid = base.Request.Params[i];
                }
            }
            if (!string.IsNullOrEmpty(viewid))
            {
                working = (ESMERALDAClasses.View)base.GetSessionValueCrossPage("View-" + viewid);
            }
            List<string> fields = new List<string>();
            List<string> colors = new List<string>();
            List<bool> series = new List<bool>();
            string[] fields_array = fieldValues.Value.Split("|".ToCharArray());
            string[] colors_array = colorValues.Value.Split("|".ToCharArray());
            string[] series_array = seriesValues.Value.Split("|".ToCharArray());

            fields.AddRange(fields_array);
            Random r = new Random();                        
            int seed_int = 0;
            string color = string.Empty;
            for (int i = 0; i < colors_array.Length; i++)
            {
                if (series_array[i] == "1")
                {
                    series.Add(true);
                }
                else
                {
                    series.Add(false);
                }
                if (!string.IsNullOrEmpty(colors_array[i]))
                    colors.Add(colors_array[i]);
                else
                {
                    seed_int = r.Next(0, 0x1000000);
                    color = Utils.ToColor(seed_int);
                    colors.Add(color);
                }
            }                                  
            PopulateData_MS(working, plot_type, fields, colors, series);
        }
    }
}