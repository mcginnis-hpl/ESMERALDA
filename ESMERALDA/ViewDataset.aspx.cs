using ESMERALDAClasses;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace ESMERALDA
{
    public partial class ViewDataset : ESMERALDAPage
    {
        protected void btnAddColumn_Click(object sender, EventArgs e)
        {
            ESMERALDAClasses.View view = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            this.ParseValueString(this.viewValues.Value, view);
            ViewCondition cond = new ViewCondition(null, ViewCondition.ConditionType.None, view);
            cond.Parent = view;
            view.Header.Add(cond);
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
            for (int i = 0; i < working.Header.Count; i++)
            {
                work = working.Header[i].ID.ToString();
                if (((ViewCondition)working.Header[i]).SourceField == null)
                {
                    work = work + "|";
                }
                else
                {
                    work = work + "|" + ((ViewCondition)working.Header[i]).SourceField.ID.ToString();
                }
                work = ((work + "|" + ((int)((ViewCondition)working.Header[i]).Type).ToString()) + "|" + ((ViewCondition)working.Header[i]).Condition) + "|" + ((ViewCondition)working.Header[i]).SQLColumnName;
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
                working.SetMetadataValue("title", this.txtViewName.Text);
                working.SetMetadataValue("abstract", this.txtDescription.Text);
                working.SetMetadataValue("purpose", this.txtBriefDescription.Text);
                working.IsPublic = chkIsPublic.Checked;
                working.IsVisible = true;
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
            if (!IsAuthenticated)
            {                
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
                    ESMERALDAClasses.View sourceview = new ESMERALDAClasses.View();
                    sourceview.Load(conn, viewid, base.Conversions, base.Metrics);
                    view = new ESMERALDAClasses.View(sourceview);
                    view.AutopopulateConditions();
                }
                else
                {
                    Dataset data = new Dataset();
                    data.Load(conn, datasetid, base.Conversions, base.Metrics);
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
                if(view.SourceData != null)
                {
                    this.PopulateSourceMetadataField(conn, view.SourceData);
                }
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

        protected void PopulateSourceMetadataField(SqlConnection conn, QuerySet working)
        {
            int i;
            DateTime starttime = DateTime.Now;
            Guid projectid = Guid.Empty;
            for (i = 0; i < base.Request.Params.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Request.Params[i]) && (base.Request.Params.GetKey(i).ToUpper() == "PROJECTID"))
                {
                    projectid = new Guid(base.Request.Params[i]);
                }
            }
            this.txtMetadata_Acquisition.InnerText = working.GetMetadataValue("acqdesc");
            this.txtMetadata_Description.InnerText = working.GetMetadataValue("abstract");
            this.txtMetadata_Name.InnerText = working.GetMetadataValue("title");
            this.txtMetadata_Processing.InnerText = working.GetMetadataValue("procdesc");
            this.txtMetadata_ShortDescription.InnerText = working.GetMetadataValue("purpose");
            string url = working.GetMetadataValue("url");
            if (string.IsNullOrEmpty(url))
            {
                this.txtMetadata_URL.InnerText = string.Empty;
            }
            else
            {
                this.txtMetadata_URL.InnerHtml = "<a href='" + url + "' target='_blank'>" + url + "</a>";
            }           
            string directlink = "<a href=\"http://hpldata.hpl.umces.edu/Default.aspx?ENTITYID=" + working.ID.ToString() + "\">" + working.ID.ToString() + "</a>";
            this.lblMetadata_DatasetID.Text = directlink;
            string keywordstring = string.Empty;
            List<string> keywords = working.GetMetadataValueArray("keyword");
            if (keywords.Count > 0)
            {
                keywordstring = keywords[0];
                for (i = 1; i < keywords.Count; i++)
                {
                    keywordstring = keywordstring + ", " + keywords[i];
                }
            }
            this.txtKeywords.InnerText = keywordstring;
            additional_metadata.InnerHtml = working.GetAdditionalMetadataTable();
            chkSourceIsPublic.Checked = working.IsPublic;
            if (working.ParentContainer != null)
            {
                lblMetadata_Project.Text = working.ParentContainer.GetMetadataValue("title");
            }            
            chooser.PopulateChooser(conn, working);
        }

        protected void PopulateBreadcrumb(ESMERALDAClasses.View working)
        {
            string ds_link = string.Empty;
            if(working.SourceData == null)
                return;
            ds_link = working.SourceData.GetMetadataValue("title");
            string proj_link = string.Empty;
            Container c = working.SourceData.ParentContainer;
            while(c != null)
            {
                proj_link = "<a href='EditContainer.aspx?CONTAINERID=" + c.ID.ToString() + "'>" + c.GetMetadataValue("title") + "</a>";
                ds_link = proj_link + ": " + ds_link;
                c = c.parentContainer;
            }

            breadcrumb.InnerHtml = ds_link;
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
                    foreach (ViewCondition cond in working.Header)
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
                            cond.SQLColumnName = tokens[4];
                            if (string.IsNullOrEmpty(cond.SQLColumnName))
                            {
                                cond.SQLColumnName = cond.SourceField.SQLColumnName;
                                int field_count = 0;
                                foreach (ViewCondition cond1 in working.Header)
                                {
                                    if ((cond1.SQLColumnName == cond.SQLColumnName) && (cond1.ID != cond.ID))
                                    {
                                        field_count++;
                                    }
                                }
                                if (field_count > 0)
                                {
                                    cond.SQLColumnName = cond.SQLColumnName + field_count.ToString();
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
                                cond.SQLColumnName = tokens[4];
                            }
                            break;
                        }
                    }
                }
            }
        }

        protected void PopulateCommonControls(ESMERALDAClasses.View view)
        {            
            if (view.SourceData != null)
            {
                HtmlGenericControl span = null;

                TableCell td = null;
                if (IsAuthenticated)
                {
                    if (view.SourceData.GetType() == typeof(Dataset))
                    {
                        span = new HtmlGenericControl("span")
                        {
                            InnerHtml = "<a class='squarebutton' href='EditDataset.aspx?DATASETID=" + view.SourceData.ID + "'><span>Edit Dataset</span></a><br/>"
                        };
                        td = new TableCell();
                        td.Controls.Add(span);
                        controlmenu.Rows[0].Cells.Add(td);
                    }

                    LinkButton lb = new LinkButton
                    {
                        Text = "<span>Save View</span>"
                    };
                    lb.Click += new EventHandler(this.lb_Click);
                    lb.CssClass = "squarebutton";
                    td = new TableCell();
                    td.Controls.Add(lb);
                    controlmenu.Rows[0].Cells.Add(td);
                }

                span = new HtmlGenericControl("span")
                {
                    InnerHtml = "<a class='squarebutton' href='javascript:showSaveDialog()' id='saveanchor'><span>Download this data</span></a>"
                };
                td = new TableCell();
                td.Controls.Add(span);
                controlmenu.Rows[0].Cells.Add(td);

                span = new HtmlGenericControl("span")
                {
                    InnerHtml = "<a class='squarebutton' href='VisualizeView.aspx?VIEWID=" + view.ID.ToString() + "' target='_blank'><span>Visualize this data</span></a>"
                };
                td = new TableCell();
                td.Controls.Add(span);
                controlmenu.Rows[0].Cells.Add(td);
                PopulateBreadcrumb(view);                    
            }
            if(IsAuthenticated)
            {
                metadata.Visible = true;
            }
            else
            {
                metadata.Visible = false;
            }
        }

        protected void PopulateData(ESMERALDAClasses.View working)
        {
            string dbname = working.SourceData.ParentContainer.database_name;
            string tablename = working.SourceData.SQLName;
            SqlConnection dataconn = base.ConnectToDatabaseReadOnly(dbname);
            this.PopulateFilters(working);
            /*DateTime start = DateTime.Now;
            this.PopulatePreviewData(dataconn, working);
            System.Diagnostics.Debug.WriteLine("Old way: " + (DateTime.Now - start).TotalMilliseconds.ToString() + "ms");*/
            ExperimentalPreviewData(working);
            this.PopulateLinks(working);
            dataconn.Close();
            this.querytag.InnerHtml = "Custom SQL Query (Source Data SQL Name: <a href='javascript:addField(\"" + working.SourceData.SQLName + "\")'>" + working.SourceData.SQLName + "</a>)";
        }

        protected void ExperimentalPreviewData(ESMERALDAClasses.View working)
        {
            // datapreview.Visible = false;
            Guid uid = Guid.NewGuid();
            SetSessionValueCrossPage(uid.ToString(), working);
            string url = "StreamData.aspx?VIEWID=" + uid.ToString();
            int numrows = -1;
            if (!string.IsNullOrEmpty(this.txtRowsToRetrieve.Text))
            {
                numrows = int.Parse(this.txtRowsToRetrieve.Text);
                url += "&NUMROWS=" + numrows.ToString();
            }

            txtQuery.Text = working.GetQuery(numrows);
            testdatapreview.Attributes["src"] = url;
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
            for (int i = 0; i < working.Header.Count; i++)
            {
                TableRow tr = new TableRow();
                TableCell tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField == null)
                {
                    DropDownList dl = new DropDownList();
                    foreach (Field f in working.SourceData.Header)
                    {
                        dl.Items.Add(new ListItem(f.Name, f.ID.ToString()));
                    }
                    dl.ID = "filtersourcefield_" + working.Header[i].ID.ToString();
                    dl.Attributes.Add("onchange", "updateFilterSourceField('" + working.Header[i].ID.ToString() + "')");
                    tc.Controls.Add(dl);
                }
                else
                {
                    tc.Text = ((ViewCondition)working.Header[i]).SourceField.Name;
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField == null)
                {
                    tc.Text = string.Empty;
                }
                else
                {
                    tc.Text = "<a href='javascript:addField(\"" + ((ViewCondition)working.Header[i]).SourceField.SQLColumnName + "\")'>" + ((ViewCondition)working.Header[i]).SourceField.SQLColumnName + "</a>";
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField == null)
                {
                    tc.Text = string.Empty;
                }
                else
                {
                    tc.Text = ((ViewCondition)working.Header[i]).SourceField.FieldMetric.Name;
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField != null)
                {
                    DropDownList filterType = new DropDownList
                    {
                        ID = "filtertype_" + working.Header[i].ID.ToString()
                    };
                    filterType.Items.Add(new ListItem(string.Empty, "0"));
                    filterType.Items.Add(new ListItem("Filter", "1"));
                    filterType.Items.Add(new ListItem("Sort Ascending", "2"));
                    filterType.Items.Add(new ListItem("Sort Decending", "3"));
                    filterType.Items.Add(new ListItem("Formula", "5"));
                    filterType.Items.Add(new ListItem("Conversion", "6"));
                    filterType.Attributes.Add("onchange", "updateFilterType('" + working.Header[i].ID.ToString() + "')");
                    if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Exclude)
                    {
                        filterType.Enabled = false;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Filter)
                    {
                        filterType.SelectedIndex = 1;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.None)
                    {
                        filterType.SelectedIndex = 0;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.SortAscending)
                    {
                        filterType.SelectedIndex = 2;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.SortDescending)
                    {
                        filterType.SelectedIndex = 3;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Formula)
                    {
                        filterType.SelectedIndex = 4;
                    }
                    else if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Conversion)
                    {
                        filterType.SelectedIndex = 5;
                    }
                    tc.Controls.Add(filterType);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField != null)
                {
                    TextBox txtFilter = new TextBox
                    {
                        ID = "filtertext_" + working.Header[i].ID.ToString()
                    };
                    txtFilter.Attributes.Add("onchange", "updateFilterText('" + working.Header[i].ID.ToString() + "')");
                    txtFilter.Text = ((ViewCondition)working.Header[i]).Condition;
                    if ((((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Filter) || (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Formula))
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
                        ID = "filterconversion_" + working.Header[i].ID.ToString()
                    };
                    ddl.Attributes.Add("onchange", "updateFilterConversion('" + working.Header[i].ID.ToString() + "')");
                    ddl.Items.Add(new ListItem(string.Empty, string.Empty));
                    foreach (Conversion c in base.Conversions)
                    {
                        if (c.SourceMetric.ID == ((ViewCondition)working.Header[i]).SourceField.FieldMetric.ID)
                        {
                            ddl.Items.Add(new ListItem(c.SourceMetric.Name + "->" + c.DestinationMetric.Name, c.ID.ToString()));
                            if ((((ViewCondition)working.Header[i]).CondConversion != null) && (c.ID == ((ViewCondition)working.Header[i]).CondConversion.ID))
                            {
                                ddl.SelectedIndex = ddl.Items.Count - 1;
                            }
                        }
                    }
                    if (((ViewCondition)working.Header[i]).CondConversion == null)
                    {
                        ddl.Style["display"] = "none";
                    }
                    tc.Controls.Add(ddl);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField != null)
                {
                    TextBox tb1 = new TextBox
                    {
                        ID = "filteralias_" + working.Header[i].ID.ToString()
                    };
                    tb1.Attributes.Add("onchange", "updateFilterAlias('" + working.Header[i].ID.ToString() + "')");
                    tb1.Text = working.Header[i].SQLColumnName;
                    tc.Controls.Add(tb1);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                if (((ViewCondition)working.Header[i]).SourceField != null)
                {
                    CheckBox cb = new CheckBox();
                    if (((ViewCondition)working.Header[i]).Type == ViewCondition.ConditionType.Exclude)
                    {
                        cb.Checked = false;
                    }
                    else
                    {
                        cb.Checked = true;
                    }
                    cb.ID = "filterinclude_" + working.Header[i].ID.ToString();
                    cb.Attributes.Add("onchange", "updateFilterInclude('" + working.Header[i].ID.ToString() + "')");
                    tc.Controls.Add(cb);
                }
                tr.Cells.Add(tc);
                tc = new TableCell();
                string metadata_string = string.Empty;
                if (((ViewCondition)working.Header[i]).SourceField != null)
                {
                    metadata_string = ((ViewCondition)working.Header[i]).SourceField.SQLColumnName + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("observation_methodology") + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("instrument") + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("analysis_methodology") + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("processing_methodology") + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("citations") + "|" + ((ViewCondition)working.Header[i]).SourceField.GetMetadataValue("description");
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

        /// <summary>
        /// Populate the list of attachments for this current request
        /// </summary>
        /// <param name="working">The current request.</param>
        protected void PopulateAttachments(ESMERALDAClasses.View working)
        {
            string linkurl = "<table border='0'>";
            if (working.attachments.Count > 0)
            {                
                foreach (AttachedFile f in working.attachments)
                {
                    string download_link = "<a href='DownloadAttachedFile.aspx?ATTACHMENTID=" + f.ID.ToString() + "' target='_blank'>Download " + f.Filename + "</a>";
                    // It's just a table; add a download link in a table cell for each row.
                    linkurl += "<tr><td>" + download_link + "</td></tr>";
                }                                
            }
            if (working.SourceData != null && working.SourceData.attachments.Count > 0)
            {
                foreach (AttachedFile f in working.SourceData.attachments)
                {
                    string download_link = "<a href='DownloadAttachedFile.aspx?ATTACHMENTID=" + f.ID.ToString() + "' target='_blank'>Download " + f.Filename + "</a>";
                    // It's just a table; add a download link in a table cell for each row.
                    linkurl += "<tr><td>" + download_link + "</td></tr>";
                }
            }
            linkurl += "</table>";
            filedownloadlink.InnerHtml = linkurl;
        }

        protected void PopulateLinks(ESMERALDAClasses.View working)
        {
            this.spanDownloadCSV.InnerHtml = "<table class='inlinemenu'><tr><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "'>Download data as comma-delimited text</a></td><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&COMPRESS=1'>Compressed</a></td></tr>";
            this.spanDownloadCSV.InnerHtml += "<tr><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&DELIM=TAB'>Download data as tab-delimited text</a></td><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&DELIM=TAB&COMPRESS=1'>Compressed</a></td></tr>";
            this.spanDownloadCSV.InnerHtml += "<tr><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&METADATA=XML'>Download metadata as XML</a></td><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&METADATA=BCODMO'>Download metadata for BCO-DMO</a></td></tr>";
            this.spanDownloadCSV.InnerHtml += "<tr><td><a target='_blank' href='DownloadViewAsCSV.aspx?VIEWID=" + working.ID.ToString() + "&METADATA=FGDC'>Download metadata for FGDC</a></td><td></td></tr>";
            if (working.SourceData.GetType() == typeof(Dataset))
            {
                this.spanDownloadCSV.InnerHtml += "<tr><td colspan='2'><a href='http://hpldata.hpl.umces.edu/Default.aspx?ENTITYID=" + working.SourceData.ID.ToString() + "'>Direct Link to Dataset</a></td></tr>";
            }
            this.spanDownloadCSV.InnerHtml += "<tr><td colspan='2'><a class='squarebutton' href='javascript:hideSaveDialog()'><span>Close this dialog</span></a></td></tr>";
            this.spanDownloadCSV.InnerHtml += "</table>";
            this.downloadcontrols.Style["display"] = "none";
            PopulateAttachments(working);
            base.SetSessionValueCrossPage("View-" + working.ID.ToString(), working);
        }

        protected void PopulateMetadata(ESMERALDAClasses.View working)
        {
            this.txtBriefDescription.Text = working.GetMetadataValue("purpose");
            this.txtDescription.Text = working.GetMetadataValue("description");
            this.txtViewName.Text = working.GetMetadataValue("title");
            if (working.IsPublic)
                chkIsPublic.Checked = true;
            else
                chkIsPublic.Checked = false;
            this.lblViewSQLName.Text = working.SQLName;
        }
        
        protected void txtRowsToRetrieve_TextChanged(object sender, EventArgs e)
        {
            ESMERALDAClasses.View working = (ESMERALDAClasses.View)base.GetSessionValue("WorkingView");
            this.PopulateData(working);
            this.BuildValueString(working);
        }
    }
}