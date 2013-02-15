using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ESMERALDAClasses;
using System.Text;

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
                working = (ESMERALDAClasses.View)base.GetSessionValue("View-" + viewid);
                if(working != null)
                    PopulateControls(working);
            }
        }

        protected void PopulateControls(ESMERALDAClasses.View inView)
        {
            string xaxis_value = comboXAxis.SelectedValue;
            string yaxis_value = comboYAxis.SelectedValue;
            string field1_value = comboField1.SelectedValue;
            string field2_value = comboField2.SelectedValue;
            string field3_value = comboField3.SelectedValue;
            string field4_value = comboField4.SelectedValue;

            comboXAxis.Items.Clear();
            comboYAxis.Items.Clear();
            comboField1.Items.Clear();
            comboField1.Items.Add(new ListItem(string.Empty, string.Empty));
            comboField2.Items.Clear();
            comboField2.Items.Add(new ListItem(string.Empty, string.Empty));
            comboField3.Items.Clear();
            comboField3.Items.Add(new ListItem(string.Empty, string.Empty));
            comboField4.Items.Clear();
            comboField4.Items.Add(new ListItem(string.Empty, string.Empty));

            comboXAxis.Items.Add(new ListItem(string.Empty, string.Empty));
            comboYAxis.Items.Add(new ListItem(string.Empty, string.Empty));

            foreach (ViewCondition vc in inView.Header)
            {
                if(vc.DBType != Field.FieldType.Decimal && vc.DBType != Field.FieldType.Integer && vc.DBType != Field.FieldType.DateTime)
                    continue;

                ListItem li = new ListItem(vc.Name, vc.Name);
                comboXAxis.Items.Add(li);

                if (vc.Name == xaxis_value)
                    comboXAxis.SelectedIndex = comboXAxis.Items.Count - 1;

                li = new ListItem(vc.Name, vc.Name);
                comboYAxis.Items.Add(li);

                if (vc.Name == yaxis_value)
                    comboYAxis.SelectedIndex = comboYAxis.Items.Count - 1;

                li = new ListItem(vc.Name, vc.Name);
                comboField1.Items.Add(li);

                if (vc.Name == field1_value)
                    comboField1.SelectedIndex = comboField1.Items.Count - 1;

                li = new ListItem(vc.Name, vc.Name);
                comboField2.Items.Add(li);

                if (vc.Name == field2_value)
                    comboField2.SelectedIndex = comboField2.Items.Count - 1;

                li = new ListItem(vc.Name, vc.Name);
                comboField3.Items.Add(li);

                if (vc.Name == field3_value)
                    comboField3.SelectedIndex = comboField3.Items.Count - 1;

                li = new ListItem(vc.Name, vc.Name);
                comboField4.Items.Add(li);

                if (vc.Name == field4_value)
                    comboField4.SelectedIndex = comboField4.Items.Count - 1;
            }
            chartType.Value = comboGraphType.SelectedValue;
        }

        protected void PopulateData(ESMERALDAClasses.View working, string plot_type, List<string> fields)
        {
            string ret = string.Empty;
            string dbname = working.SourceData.ParentProject.database_name;
            SqlConnection conn = base.ConnectToDatabaseReadOnly(dbname);
            int numrows = -1;
            chartType.Value = plot_type;
            labels.Value = string.Empty;
            types.Value = string.Empty;

            List<ViewCondition> vcs = new List<ViewCondition>();
            foreach (string s in fields)
            {
                foreach (ViewCondition vc in working.Header)
                {
                    if (vc.Name == s)
                    {
                        vcs.Add(vc);
                        if (string.IsNullOrEmpty(labels.Value))
                        {
                            labels.Value = s;
                            types.Value = Field.GetFieldTypeName(vc.DBType);
                        }
                        else
                        {
                            labels.Value = labels.Value + "~" + s;
                            types.Value = types.Value + "~" + Field.GetFieldTypeName(vc.DBType);
                        }
                    }
                }
            }

            string value_string = string.Empty;

            string cmd = working.GetQuery(numrows);
            string xval = string.Empty;
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(cmd))
            {
                SqlDataReader reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                while (reader.Read())
                {
                    string row = string.Empty;

                    foreach (ViewCondition vc in vcs)
                    {
                        xval = string.Empty;
                        if (!reader.IsDBNull(reader.GetOrdinal(vc.SQLColumnName)))
                        {
                            if (vc.CondConversion != null)
                            {
                                xval = (vc.CondConversion.DestinationMetric.Format(reader[vc.SQLColumnName].ToString()));
                            }
                            else
                            {
                                xval = (vc.SourceField.FieldMetric.Format(reader[vc.SQLColumnName].ToString()));
                            }                            
                        }
                        if (string.IsNullOrEmpty(row))
                            row = xval;
                        else
                            row += "," + xval;
                    }
                    if (sb.Length > 0)
                    {
                        sb.Append(";");
                    }
                    sb.Append(row);
                }
                reader.Close();
            }
            conn.Close();
            points.Value = sb.ToString();
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
                working = (ESMERALDAClasses.View)base.GetSessionValue("View-" + viewid);
            }
            List<string> fields = new List<string>();
            if (working != null)
            {
                if (plot_type == "SCATTER")
                {
                    fields.Add(comboXAxis.SelectedValue);
                    fields.Add(comboYAxis.SelectedValue);
                    PopulateData(working, plot_type, fields);                    
                }
                else if (plot_type == "LINE")
                {
                    fields.Add(comboXAxis.SelectedValue);
                    fields.Add(comboYAxis.SelectedValue);
                    PopulateData(working, plot_type, fields);                    
                }
                else if (plot_type == "COLUMN")
                {
                    if(!string.IsNullOrEmpty(comboField1.SelectedValue))
                        fields.Add(comboField1.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField2.SelectedValue))
                        fields.Add(comboField2.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField3.SelectedValue))
                        fields.Add(comboField3.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField4.SelectedValue))
                        fields.Add(comboField4.SelectedValue);
                    PopulateData(working, plot_type, fields);
                }
                else if (plot_type == "BAR")
                {
                    if (!string.IsNullOrEmpty(comboField1.SelectedValue))
                        fields.Add(comboField1.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField2.SelectedValue))
                        fields.Add(comboField2.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField3.SelectedValue))
                        fields.Add(comboField3.SelectedValue);
                    if (!string.IsNullOrEmpty(comboField4.SelectedValue))
                        fields.Add(comboField4.SelectedValue);
                    PopulateData(working, plot_type, fields);
                }
            }
        }
    }
}