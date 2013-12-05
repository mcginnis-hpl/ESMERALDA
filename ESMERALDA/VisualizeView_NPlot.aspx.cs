using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using NPlot;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Drawing;

namespace ESMERALDA
{
    public partial class VisualizeView_NPlot : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = null;
            string viewid = string.Empty;
            string[] fields = null;
            string[] colors = null;
            bool[] series = null;
            char[] delim = {'|'};
            int width = 0;
            int height = 0;
            string plot_type = string.Empty;

            for (int i = 0; i < base.Request.Params.Count; i++)
            {
                if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                {
                    viewid = base.Request.Params[i];
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "FIELDS")
                {
                    string tmp = base.Request.Params[i];
                    fields = tmp.Split(delim);
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "COLORS")
                {
                    string tmp = base.Request.Params[i];
                    colors = tmp.Split(delim);
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "SERIES")
                {
                    string tmp = base.Request.Params[i];
                    string[] tmp_array = tmp.Split(delim);
                    series = new bool[tmp_array.Length];
                    for (int j = 0; j < tmp_array.Length; j++)
                    {
                        if (string.IsNullOrEmpty(tmp_array[j]))
                        {
                            series[j] = false;
                        }
                        else if (tmp_array[j] == "1")
                        {
                            series[j] = true;
                        }
                        else
                        {
                            series[j] = false;
                        }
                    }
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "WIDTH")
                {
                    width = int.Parse(base.Request.Params[i]);
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "HEIGHT")
                {
                    height = int.Parse(base.Request.Params[i]);
                }
                else if (base.Request.Params.GetKey(i).ToUpper() == "PLOT_TYPE")
                {
                    plot_type = base.Request.Params[i];
                }
            }
            if (viewid == string.Empty)
            {
                return;
            }
            working = (ESMERALDAClasses.View)GetSessionValue(viewid);
            if (working != null)
            {
                PopulateData(working, plot_type, fields, colors, series, width, height);
            }
        }

        protected void PopulateData(ESMERALDAClasses.View working, string plot_type, string[] fields, string[] colors, bool[] series, int width, int height)
        {
            string ret = string.Empty;
            string dbname = working.SourceData.ParentContainer.database_name;
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);

            NPlot.Bitmap.PlotSurface2D npSurface = new NPlot.Bitmap.PlotSurface2D(width, height);

            QueryField x_axis = null;
            for (int i = 0; i < working.Header.Count; i++)
            {
                if (working.Header[i].SQLColumnName == fields[0])
                    x_axis = working.Header[i];
            }
            //Font definitions:
            Font TitleFont = new Font("Arial", 12);
            Font AxisFont = new Font("Arial", 10);
            Font TickFont = new Font("Arial", 8);

            //Legend definition:
            NPlot.Legend npLegend = new NPlot.Legend();

            //Prepare PlotSurface:
            npSurface.Clear();
            npSurface.Title = fields[0] + " vs " + fields[1];
            npSurface.BackColor = System.Drawing.Color.White;

            //Left Y axis grid:
            NPlot.Grid p = new Grid();
            npSurface.Add(p, NPlot.PlotSurface2D.XAxisPosition.Bottom,
                          NPlot.PlotSurface2D.YAxisPosition.Left);
            DataTable dt = working.GetDataTable(conn);

            for (int i = 1; i < fields.Length; i++)
            {
                QueryField y_axis = null;
                for (int j = 0; j < working.Header.Count; j++)
                {
                    if (working.Header[j].SQLColumnName == fields[i])
                        y_axis = working.Header[j];
                }
                if (series[i])
                {
                    // NPlot.LinePlot tmp_plot = null;
                }
                else
                {
                    NPlot.LinePlot tmp_plot = new LinePlot();
                    if (y_axis.DBType == Field.FieldType.DateTime)
                    {
                        DateTime[] data = new DateTime[dt.Rows.Count];
                    }
                    else if (y_axis.DBType == Field.FieldType.Decimal)
                    {
                    }
                    else if (y_axis.DBType == Field.FieldType.Integer)
                    {
                    }
                    else if (y_axis.DBType == Field.FieldType.Text)
                    {

                    }                    
                }
            }

            conn.Close();
        }
    }
}