using ESMERALDAClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class ViewDataset : ESMERALDAPage
    {
        protected void btnAddColumn_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View view = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            this.ParseValueString(this.viewValues.Value, view);
            ViewCondition cond = new ViewCondition(null, ViewCondition.ConditionType.None, view);
            view.Conditions.Add(cond);
            base.SetSessionValue("WorkingView", view);
            this.PopulateData(view);
            this.BuildValueString(view);
        }

        protected void btnExecuteQuery_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View view = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            view.SQLQuery = this.txtQuery.Text;
            base.SetSessionValue("WorkingView", view);
            this.PopulateData(view);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View view = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            view.SQLQuery = string.Empty;
            this.ParseValueString(this.viewValues.Value, view);
            base.SetSessionValue("WorkingView", view);
            this.PopulateData(view);
            this.BuildValueString(view);
        }

        protected void BuildValueString(ESMERALDAClasses.View working)
        {
            string ret = string.Empty;
            string work = string.Empty;
            for (int i = 0; i < working.Conditions.Count; i++)
            {
                work = working.Conditions[i].ID.ToString();
                if (working.Conditions[i].SourceField == null)
                {
                    work = work + "|";
                }
                else
                {
                    work = work + "|" + working.Conditions[i].SourceField.ID.ToString();
                }
                work = ((work + "|" + ((int)working.Conditions[i].Type).ToString()) + "|" + working.Conditions[i].Condition) + "|" + working.Conditions[i].SQLName;
                if (string.IsNullOrEmpty(ret))
                {
                    ret = work;
                }
                else
                {
                    ret = ret + ";" + work;
                }
            }
            this.viewValues.Value = ret;
        }

        private void lb_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            this.ParseValueString(this.viewValues.Value, working);
            if (working != null)
            {
                working.Name = this.txtViewName.Text;
                working.Description = this.txtDescription.Text;
                working.BriefDescription = this.txtBriefDescription.Text;
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                working.Save(conn);
                conn.Close();
            }
            this.PopulateData(working);
            this.BuildValueString(working);
            this.PopulateMetadata(working);
            base.SetSessionValue("WorkingView", working);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtRowsToRetrieve.Text))
            {
                this.txtRowsToRetrieve.Text = "100";
            }
            ESMERALDAClasses.View view = null;
            if (!base.IsPostBack)
            {
                base.RemoveSessionValue("WorkingView");
                Guid datasetid = Guid.Empty;
                Guid viewid = Guid.Empty;
                for (int i = 0; i < base.Request.Params.Count; i++)
                {
                    if (base.Request.Params.GetKey(i).ToUpper() == "DATASETID")
                    {
                        datasetid = new Guid(base.Request.Params[i]);
                    }
                    if (base.Request.Params.GetKey(i).ToUpper() == "VIEWID")
                    {
                        viewid = new Guid(base.Request.Params[i]);
                    }
                }
                SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
                if (viewid != Guid.Empty)
                {
                    view = ESMERALDAClasses.View.LoadView(conn, viewid, base.Conversions, base.Metrics);
                }
                else
                {
                    Dataset data = null;
                    data = Dataset.Load(conn, datasetid, base.Metrics);
                    if (view == null)
                    {
                        view = new ESMERALDAClasses.View(data);
                        view.AutopopulateConditions();
                    }
                }
                base.SetSessionValue("WorkingView", view);
                this.PopulateData(view);
                this.BuildValueString(view);
                this.PopulateCommonControls(view);
                conn.Close();
            }
            else
            {
                view = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
                if (view != null)
                {
                    this.PopulateCommonControls(view);
                }
            }
        }

        protected void ParseValueString(string inString, ESMERALDAClasses.View working)
        {
            char[] delim1 = new char[] { ';' };
            char[] delim2 = new char[] { '|' };
            string[] lines = this.viewValues.Value.Split(delim1);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] tokens = lines[i].Split(delim2);
                if (tokens.Length >= 3)
                {
                    Guid conditionid = new Guid(tokens[0]);
                    foreach (ViewCondition cond in working.Conditions)
                    {
                        if (!(cond.ID == conditionid))
                        {
                            continue;
                        }
                        foreach (Field f in working.SourceData.Header)
                        {
                            if (f.ID.ToString() == tokens[1])
                            {
                                cond.SourceField = f;
                                break;
                            }
                        }
                        if (cond.SourceField != null)
                        {
                            cond.SQLName = tokens[4];
                            if (string.IsNullOrEmpty(cond.SQLName))
                            {
                                cond.SQLName = cond.SourceField.SQLColumnName;
                                int field_count = 0;
                                foreach (ViewCondition cond1 in working.Conditions)
                                {
                                    if ((cond1.SQLName == cond.SQLName) && (cond1.ID != cond.ID))
                                    {
                                        field_count++;
                                    }
                                }
                                if (field_count > 0)
                                {
                                    cond.SQLName = cond.SQLName + field_count.ToString();
                                }
                            }
                            cond.Type = (ViewCondition.ConditionType)int.Parse(tokens[2]);
                            cond.Condition = tokens[3];
                            if (cond.Type == ViewCondition.ConditionType.Conversion)
                            {
                                Guid cond_id = new Guid(cond.Condition);
                                foreach (Conversion c in base.Conversions)
                                {
                                    if (c.ID == cond_id)
                                    {
                                        cond.CondConversion = c;
                                        break;
                                    }
                                }
                                if (cond.CondConversion == null)
                                {
                                    throw new Exception("Could not find field conversion for: " + cond.SourceField.Name);
                                }
                            }
                            if (!string.IsNullOrEmpty(tokens[4]))
                            {
                                cond.SQLName = tokens[4];
                            }
                            break;
                        }
                    }
                }
            }
        }

        protected void PopulateCommonControls(ESMERALDAClasses.View view)
        {
            if (base.IsAuthenticated)
            {
                if (view.SourceData != null)
                {
                    if (view.SourceData.IsEditable)
                    {
                        HtmlGenericControl span = new HtmlGenericControl("span")
                        {
                            InnerHtml = "<a href='EditDataset.aspx?DATASETID=" + view.SourceData.ID + "'>Edit Dataset</a><br/>"
                        };
                        this.commoncontrols.Controls.Add(span);
                    }
                    LinkButton lb = new LinkButton
                    {
                        Text = "Save View"
                    };
                    lb.Click += new EventHandler(this.lb_Click);
                    this.commoncontrols.Controls.Add(lb);
                }
            }
            else
            {
                this.commoncontrols.Controls.Clear();
            }
        }

        protected void PopulateData(ESMERALDAClasses.View working)
        {
            string dbname = working.SourceData.ParentProject.database_name;
            string tablename = working.SourceData.TableName;
            SqlConnection dataconn = base.ConnectToDatabase(dbname);
            this.PopulateFilters(working);
            this.PopulatePreviewData(dataconn, working);
            this.PopulateLinks(working);
            dataconn.Close();
            this.querytag.InnerHtml = "Custom SQL Query (Source Data SQL Name: <a href='javascript:addField(\"" + working.SourceData.TableName + "\")'>" + working.SourceData.TableName + "</a>)";
        }

        protected void PopulateFilters(ESMERALDAClasses.View working)
        {
            string metadata_table = string.Empty;
            TableHeaderRow thr = new TableHeaderRow();
            TableHeaderCell thc = new TableHeaderCell
            {
                Text = "Field Name"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Source Name"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Data Type"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Filter Type"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Filter Text"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Alias"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell
            {
                Text = "Include in Output"
            };
            thr.Cells.Add(thc);
            thc = new TableHeaderCell();
            thr.Cells.Add(thc);
            this.filterTable.Rows.Add(thr);
            for (int i = 0; i < working.Conditions.Count; i++)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                if (working.Conditions[i].SourceField == null)
                {
                    DropDownList dl = new DropDownList();
                    foreach (Field f in working.SourceData.Header)
                    {
                        dl.Items.Add(new ListItem(f.Name, f.ID.ToString()));
                    }
                    dl.ID = "filtersourcefield_" + working.Conditions[i].ID.ToString();
                    dl.Attributes.Add("onchange", "updateFilterSourceField('" + working.Conditions[i].ID.ToString() + "')");
                    tc.Controls.Add(dl);
                }
                else
                {
                    tc.Text = working.Conditions[i].SourceField.Name;
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField == null)
                {
                    tc.Text = string.Empty;
                }
                else
                {
                    tc.Text = "<a href='javascript:addField(\"" + working.Conditions[i].SourceField.SQLColumnName + "\")'>" + working.Conditions[i].SourceField.SQLColumnName + "</a>";
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField == null)
                {
                    tc.Text = string.Empty;
                }
                else
                {
                    tc.Text = working.Conditions[i].SourceField.FieldMetric.Name;
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField != null)
                {
                    DropDownList filterType = new DropDownList
                    {
                        ID = "filtertype_" + working.Conditions[i].ID.ToString()
                    };
                    filterType.Items.Add(new ListItem(string.Empty, "0"));
                    filterType.Items.Add(new ListItem("Filter", "1"));
                    filterType.Items.Add(new ListItem("Sort Ascending", "2"));
                    filterType.Items.Add(new ListItem("Sort Decending", "3"));
                    filterType.Items.Add(new ListItem("Formula", "5"));
                    filterType.Items.Add(new ListItem("Conversion", "6"));
                    filterType.Attributes.Add("onchange", "updateFilterType('" + working.Conditions[i].ID.ToString() + "')");
                    if (working.Conditions[i].Type == ViewCondition.ConditionType.Exclude)
                    {
                        filterType.Enabled = false;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.Filter)
                    {
                        filterType.SelectedIndex = 1;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.None)
                    {
                        filterType.SelectedIndex = 0;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.SortAscending)
                    {
                        filterType.SelectedIndex = 2;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.SortDescending)
                    {
                        filterType.SelectedIndex = 3;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.Formula)
                    {
                        filterType.SelectedIndex = 4;
                    }
                    else if (working.Conditions[i].Type == ViewCondition.ConditionType.Conversion)
                    {
                        filterType.SelectedIndex = 5;
                    }
                    tc.Controls.Add(filterType);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField != null)
                {
                    TextBox txtFilter = new TextBox
                    {
                        ID = "filtertext_" + working.Conditions[i].ID.ToString()
                    };
                    txtFilter.Attributes.Add("onchange", "updateFilterText('" + working.Conditions[i].ID.ToString() + "')");
                    txtFilter.Text = working.Conditions[i].Condition;
                    if ((working.Conditions[i].Type == ViewCondition.ConditionType.Filter) || (working.Conditions[i].Type == ViewCondition.ConditionType.Formula))
                    {
                        txtFilter.Style["display"] = "";
                    }
                    else
                    {
                        txtFilter.Style["display"] = "none";
                    }
                    tc.Controls.Add(txtFilter);
                    DropDownList ddl = new DropDownList
                    {
                        ID = "filterconversion_" + working.Conditions[i].ID.ToString()
                    };
                    ddl.Attributes.Add("onchange", "updateFilterConversion('" + working.Conditions[i].ID.ToString() + "')");
                    ddl.Items.Add(new ListItem(string.Empty, string.Empty));
                    foreach (Conversion c in base.Conversions)
                    {
                        if (c.SourceMetric.ID == working.Conditions[i].SourceField.FieldMetric.ID)
                        {
                            ddl.Items.Add(new ListItem(c.SourceMetric.Name + "->" + c.DestinationMetric.Name, c.ID.ToString()));
                            if ((working.Conditions[i].CondConversion != null) && (c.ID == working.Conditions[i].CondConversion.ID))
                            {
                                ddl.SelectedIndex = ddl.Items.Count - 1;
                            }
                        }
                    }
                    if (working.Conditions[i].CondConversion == null)
                    {
                        ddl.Style["display"] = "none";
                    }
                    tc.Controls.Add(ddl);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField != null)
                {
                    TextBox tb1 = new TextBox
                    {
                        ID = "filteralias_" + working.Conditions[i].ID.ToString()
                    };
                    tb1.Attributes.Add("onchange", "updateFilterAlias('" + working.Conditions[i].ID.ToString() + "')");
                    tb1.Text = working.Conditions[i].SQLName;
                    tc.Controls.Add(tb1);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (working.Conditions[i].SourceField != null)
                {
                    CheckBox cb = new CheckBox();
                    if (working.Conditions[i].Type == ViewCondition.ConditionType.Exclude)
                    {
                        cb.Checked = false;
                    }
                    else
                    {
                        cb.Checked = true;
                    }
                    cb.ID = "filterinclude_" + working.Conditions[i].ID.ToString();
                    cb.Attributes.Add("onchange", "updateFilterInclude('" + working.Conditions[i].ID.ToString() + "')");
                    tc.Controls.Add(cb);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                string metadata_string = string.Empty;
                if (working.Conditions[i].SourceField != null)
                {
                    metadata_string = working.Conditions[i].SourceField.SourceColumnName + "|" + working.Conditions[i].SourceField.Metadata.observation_methodology + "|" + working.Conditions[i].SourceField.Metadata.instrument + "|" + working.Conditions[i].SourceField.Metadata.analysis_methodology + "|" + working.Conditions[i].SourceField.Metadata.processing_methodology + "|" + working.Conditions[i].SourceField.Metadata.citations + "|" + working.Conditions[i].SourceField.Metadata.description;
                    tc.Text = "<a id='metadata_" + i.ToString() + "' href='javascript:showMetadata(\"metadata_" + i.ToString() + "\"," + i.ToString() + ")'>Show Metadata</a>";
                }
                else
                {
                    metadata_string = "|||||";
                }
                tr.Cells.Add(tc);
                if (string.IsNullOrEmpty(metadata_table))
                {
                    metadata_table = metadata_string;
                }
                else
                {
                    metadata_table = metadata_table + "~" + metadata_string;
                }
                this.filterTable.Rows.Add(tr);
            }
            this.fieldMetadata.Value = metadata_table;
        }

        protected void PopulateLinks(ESMERALDAClasses.View working)
        {
            this.spanDownloadCSV.InnerHtml = "<table border='0px'><tr><td><a href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "'>Download data as CSV</a></td>";
            this.spanDownloadCSV.InnerHtml = this.spanDownloadCSV.InnerHtml + "<td><a href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&METADATA=1'>Download metadata as XML</a></td></tr></table>";
            base.SetSessionValue("View-" + working.ID.ToString(), working);
        }

        protected void PopulateMetadata(ESMERALDAClasses.View working)
        {
            this.txtBriefDescription.Text = working.BriefDescription;
            this.txtDescription.Text = working.Description;
            this.txtViewName.Text = working.Name;
            this.lblViewSQLName.Text = working.ViewSQLName;
        }

        protected void PopulatePreviewData(SqlConnection conn, ESMERALDAClasses.View working)
        {
            TableHeaderRow thr;
            SqlDataReader reader;
            int i;
            TableHeaderCell thc;
            TableRow tr;
            TableCell tc;
            this.errormessage.InnerHtml = string.Empty;
            if (!string.IsNullOrEmpty(working.SQLQuery))
            {
                this.tblPreviewData.Rows.Clear();
                thr = null;
                SqlCommand query = new SqlCommand
                {
                    Connection = conn,
                    CommandTimeout = 60,
                    CommandType = CommandType.Text,
                    CommandText = working.SQLQuery
                };
                reader = null;
                try
                {
                    reader = query.ExecuteReader();
                }
                catch (Exception ex)
                {
                    this.errormessage.InnerHtml = "<strong>" + ex.Message + ":</strong> " + ex.StackTrace;
                    return;
                }
                while (reader.Read())
                {
                    if (thr == null)
                    {
                        thr = new TableHeaderRow();
                        i = 0;
                        while (i < reader.FieldCount)
                        {
                            thc = new TableHeaderCell
                            {
                                Text = reader.GetName(i)
                            };
                            thr.Cells.Add(thc);
                            i++;
                        }
                        this.tblPreviewData.Rows.Add(thr);
                    }
                    tr = new TableRow();
                    for (i = 0; i < reader.FieldCount; i++)
                    {
                        tc = new TableCell
                        {
                            Text = reader[i].ToString()
                        };
                        tr.Cells.Add(tc);
                    }
                    this.tblPreviewData.Rows.Add(tr);
                }
                reader.Close();
            }
            else
            {
                int numrows = -1;
                if (!string.IsNullOrEmpty(this.txtRowsToRetrieve.Text))
                {
                    numrows = int.Parse(this.txtRowsToRetrieve.Text);
                }
                string cmd = working.GetQuery(numrows);
                this.txtQuery.Text = cmd;
                this.tblPreviewData.Rows.Clear();
                thr = new TableHeaderRow();
                i = 0;
                while (i < working.Conditions.Count)
                {
                    if ((working.Conditions[i].SourceField != null) && (working.Conditions[i].Type != ViewCondition.ConditionType.Exclude))
                    {
                        thc = new TableHeaderCell
                        {
                            Text = working.Conditions[i].SQLName
                        };
                        thr.Cells.Add(thc);
                    }
                    i++;
                }
                this.tblPreviewData.Rows.Add(thr);
                if (!string.IsNullOrEmpty(cmd))
                {
                    reader = new SqlCommand { Connection = conn, CommandTimeout = 60, CommandType = CommandType.Text, CommandText = cmd }.ExecuteReader();
                    while (reader.Read())
                    {
                        tr = new TableRow();
                        for (i = 0; i < working.Conditions.Count; i++)
                        {
                            if ((working.Conditions[i].SourceField != null) && (working.Conditions[i].Type != ViewCondition.ConditionType.Exclude))
                            {
                                tc = new TableCell();
                                if (!reader.IsDBNull(reader.GetOrdinal(working.Conditions[i].SQLName)))
                                {
                                    if (working.Conditions[i].CondConversion != null)
                                    {
                                        tc.Text = working.Conditions[i].CondConversion.DestinationMetric.Format(reader[working.Conditions[i].SQLName].ToString());
                                    }
                                    else
                                    {
                                        tc.Text = working.Conditions[i].SourceField.FieldMetric.Format(reader[working.Conditions[i].SQLName].ToString());
                                    }
                                }
                                if ((working.Conditions[i].Type == ViewCondition.ConditionType.Conversion) || (working.Conditions[i].Type == ViewCondition.ConditionType.Formula))
                                {
                                    tc.CssClass = "modifiedHeaderCell";
                                }
                                tr.Cells.Add(tc);
                            }
                        }
                        this.tblPreviewData.Rows.Add(tr);
                    }
                    reader.Close();
                }
            }
        }

        protected void txtRowsToRetrieve_TextChanged(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            this.PopulateData(working);
            this.BuildValueString(working);
        }
    }
}