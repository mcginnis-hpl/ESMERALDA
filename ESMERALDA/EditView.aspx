<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditView.aspx.cs" Inherits="ESMERALDA.EditView" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <title>Edit a View</title>
    <script runat="server">
        void PopulateNode(Object sender, TreeNodeEventArgs evt)
        {
            SqlConnection conn = base.ConnectToConfigString("RepositoryConnection");
            char[] delim = { '~' };
            string[] tokens = evt.Node.Value.Split(delim);
            string query = "SELECT DISTINCT field_id, dataset_id, field_name, sql_column_name FROM field_metadata WHERE dataset_id='" + tokens[0] + "' ORDER BY field_name";
            SqlCommand cmd = new SqlCommand()
            {
                Connection = conn,
                CommandType = CommandType.Text,
                CommandText = query
            };
            SqlDataReader reader = cmd.ExecuteReader();
            List<EntityHolder> fields = new List<EntityHolder>();
            while (reader.Read())
            {
                EntityHolder e = new EntityHolder();
                e.Name = reader["field_name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("dataset_id")))
                {
                    e.ParentID = new Guid(reader["dataset_id"].ToString());
                }
                e.EntityID = new Guid(reader["field_id"].ToString());
                if (!reader.IsDBNull(reader.GetOrdinal("sql_column_name")))
                    e.SQLName = reader["sql_column_name"].ToString();
                e.SQLType = "field";
                fields.Add(e);
            }
            reader.Close();
            conn.Close();
            foreach (EntityHolder h in fields)
            {
                TreeNode n = new TreeNode();
                string SQLName = h.SQLName;
                if (tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1]))
                {
                    SQLName = tokens[1] + ".[" + SQLName + "]";
                }
                n.Text = "<a href='javascript:addField(\"" + SQLName + "\")'>" + h.Name + "</a>";
                n.Value = h.EntityID.ToString();
                evt.Node.ChildNodes.Add(n);
            }
        }            
    </script>
    <script type="text/javascript" language="javascript">
        function addField(myValue) {
            var myField = document.getElementById("txtQueryText");
            //IE support
            if (document.selection) {
                myField.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
            }
            //MOZILLA/NETSCAPE support
            else if (myField.selectionStart || myField.selectionStart == '0') {
                var startPos = myField.selectionStart;
                var endPos = myField.selectionEnd;
                myField.value = myField.value.substring(0, startPos) + myValue + myField.value.substring(endPos, myField.value.length);
            } else {
                myField.value += myValue;
            }
        }

        function getNewMetric(column, row, controlID) {
            newMetricColumn = column;
            newMetricRow = row;
            newMetricControlID = controlID;
            var element = document.getElementById(controlID);
            var position = getOffset(element);

            var metricdiv = document.getElementById("newMetricWindow");

            var ap = document.getElementById(controlID);
            if (ap == null) { return; }
            var name_field = document.getElementById("<%=txtNewMetricName.ClientID%>");
            var abbrev_field = document.getElementById("<%=txtNewMetricAbbrev.ClientID%>");
            name_field.value = "";
            abbrev_field.value = "";
            var typeval = document.getElementById("header_unit" + row);
            var type_field = document.getElementById("<%=comboMetricType.ClientID%>");
            if (typeval && typeval.value) {
                for (var i = 0; i < type_field.options.length; i++) {
                    if (type_field.options[i].value == typeval.value) {
                        type_field.selectedIndex = i;
                        break;
                    }
                }
            }
            metricdiv.style.display = '';
            metricdiv.style.top = (getOffset(ap).top + 20) + "px";
            metricdiv.style.left = (getOffset(ap).left + 20) + "px";
        }

        var xmlhttp;
        function getURL(url, callback) {
            xmlhttp = null;
            if (window.XMLHttpRequest) {// code for all new browsers
                xmlhttp = new XMLHttpRequest();
            }
            else if (window.ActiveXObject) {// code for IE5 and IE6
                xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
            if (xmlhttp != null) {
                xmlhttp.onreadystatechange = callback;
                xmlhttp.open("GET", url, true);
                xmlhttp.send(null);
            }
            else {
                alert("Your browser does not support XMLHTTP.");
            }
        }

        function getNewMetricIDDone() {
            if (xmlhttp.readyState == 4) {// 4 = "loaded"
                if (xmlhttp.status == 200) {// 200 = OK
                    //xmlhttp.data and shtuff
                    var newid = "";
                    if (xmlhttp.data) {
                        newid = xmlhttp.data;
                    }
                    else if (xmlhttp.responseText) {
                        newid = xmlhttp.responseText;
                    }

                    replaceValue(newMetricColumn, newMetricRow, newid);

                    var metricdiv = document.getElementById("newMetricWindow");
                    var name_field = document.getElementById("<%=txtNewMetricName.ClientID%>");
                    var abbrev_field = document.getElementById("<%=txtNewMetricAbbrev.ClientID%>");
                    var type_field = document.getElementById("<%=comboMetricType.ClientID%>");
                    var newMetrics = document.getElementById("newMetrics");
                    if (newMetrics.value && newMetrics.value.length > 0) {
                        newMetrics.value = newMetrics.value + ";";
                    }
                    newMetrics.value = newMetrics.value + name_field.value + "|" + abbrev_field.value + "|" + newid + "|" + type_field.value + "|" + "1";

                    var i = 0;
                    var metric_dropdown = document.getElementById("header_metric" + i);
                    while (metric_dropdown) {
                        populateMetric("header_unit" + i, "header_metric" + i, newid);
                        i += 1;
                        metric_dropdown = document.getElementById("header_metric" + i);
                    }
                    metricdiv.style.display = "none";
                }
                else {
                    alert("Problem retrieving data");
                }
            }
        }

        function commitNewMetric() {
            var url = "GuidGenerator.aspx?berandom=" + Math.random();
            getURL(url, getNewMetricIDDone);
        }

        function getOffset(el) {
            var _x = 0;
            var _y = 0;
            while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
                _x += el.offsetLeft - el.scrollLeft;
                _y += el.offsetTop - el.scrollTop;
                el = el.offsetParent;
            }
            return { top: _y, left: _x };
        }

        function cancelNewMetric() {
            var metricdiv = document.getElementById("newMetricWindow");
            metricdiv.style.display = "none";
            var newcontrol = document.getElementById(newMetricControlID);
            newcontrol.selectedIndex = 0;
            newMetricControlID = null;
        }

        function replaceValue(column, row, value) {
            var spec = document.getElementById("tableSpecification");
            var rows = spec.value.split(";");
            var cols = rows[row].split("|");
            cols[column] = value;
            var newrow = cols[0];
            var i = 0;
            for (i = 1; i < cols.length; i++) {
                newrow = newrow + "|" + cols[i];
            }
            var newtable = "";
            if (row == 0) {
                newtable = newrow;
            }
            else {
                newtable = rows[0];
            }

            for (i = 1; i < rows.length; i++) {
                if (i == row) {
                    newtable = newtable + ";" + newrow;
                }
                else {
                    newtable = newtable + ";" + rows[i];
                }
            }
            spec.value = newtable;
        }

        function updateHeaderField(column, row, controlID) {
            var spec = document.getElementById("tableSpecification");
            var field = document.getElementById(controlID);
            var newvalue;
            if (column == 1) {
                replaceValue(column, row, field.value);
            }
            else if (column == 2) {
                var target_metric = "header_metric" + row;
                populateMetric(controlID, target_metric, null);
                replaceValue(column, row, field.options[field.selectedIndex].value);
                populateTimeFields();
            }
            else if (column == 3) {
                newvalue = field.options[field.selectedIndex].value;
                if (newvalue == "NEW") {
                    getNewMetric(column, row, controlID);
                }
                else {
                    replaceValue(column, row, newvalue);
                }
            }
            else if (column == 4) {
                if (field.checked) {
                    replaceValue(column, row, "1");
                }
                else {
                    replaceValue(column, row, "0");
                }
            }
            else if (column == 5) {
                newvalue = field.options[field.selectedIndex].value;
                replaceValue(column, row, newvalue);
            }
            else if (column == 6) {
                if (field.checked) {
                    replaceValue(column, row, "1");
                }
                else {
                    replaceValue(column, row, "0");
                }
            }
        }

        function editAddMetadata(controlID, column_name) {
            var targ = document.getElementById('addMetadataTarget');
            targ.value = column_name;
            var meta = document.getElementById("fieldMetadata");
            var rows = meta.value.split("~");
            var i = 0;
            for (i = 0; i < rows.length; i++) {
                var cols = rows[i].split("|");
                if (cols[0] == column_name) {
                    var control = document.getElementById('txt_meta_observation');
                    control.value = cols[1];
                    control = document.getElementById('txt_meta_instrument');
                    control.value = cols[2];
                    control = document.getElementById('txt_meta_analysis');
                    control.value = cols[3];
                    control = document.getElementById('txt_meta_processing');
                    control.value = cols[4];
                    control = document.getElementById('txt_meta_citations');
                    control.value = cols[5];
                    control = document.getElementById('txt_meta_description');
                    control.value = cols[6];
                    break;
                }
            }
            var ap = document.getElementById(controlID);
            var popup = document.getElementById('fieldMetadataWindow');
            popup.style.display = '';
            popup.style.top = (getOffset(ap).top + 20) + "px";
            popup.style.left = (getOffset(ap).left + 20) + "px";
        }

        function commitMetadata() {
            var targ = document.getElementById('addMetadataTarget');
            var column_name = targ.value;
            var spec = document.getElementById("fieldMetadata");
            var rows = spec.value.split("~");
            var i = 0;
            for (i = 0; i < rows.length; i++) {
                var cols = rows[i].split("|");
                if (cols[0] == column_name) {
                    var control = document.getElementById('txt_meta_observation');
                    replaceMetadataValue(1, i, control.value);
                    control = document.getElementById('txt_meta_instrument');
                    replaceMetadataValue(2, i, control.value);
                    control = document.getElementById('txt_meta_analysis');
                    replaceMetadataValue(3, i, control.value);
                    control = document.getElementById('txt_meta_processing');
                    replaceMetadataValue(4, i, control.value);
                    control = document.getElementById('txt_meta_citations');
                    replaceMetadataValue(5, i, control.value);
                    control = document.getElementById('txt_meta_description');
                    replaceMetadataValue(6, i, control.value);
                }
            }
            var metricdiv = document.getElementById("fieldMetadataWindow");
            metricdiv.style.display = "none";
        }

        function cancelMetadata() {
            var metricdiv = document.getElementById("fieldMetadataWindow");
            metricdiv.style.display = "none";
        }

        function toggleFieldMetadata() {
            var el = document.getElementById("field_metadata");
            if (el.style.display == "none") {
                el.style.display = "";
            }
            else {
                el.style.display = "none";
            }
        }
    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <h4>
                Edit a View</h4>
            <div id="metadata">
                <table border="0">
                    <tr>
                        <td>
                            View Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtViewName" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Brief Description:
                        </td>
                        <td>
                            <asp:TextBox ID="txtBriefDescription" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description:
                        </td>
                        <td>
                            <asp:TextBox ID="txtDescription" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            SQL View Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtViewSQLName" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            View is Public:
                        </td>
                        <td>
                            <asp:CheckBox ID="chkIsPublic" runat="server" />
                        </td>
                    </tr>
                </table>
            </div>
            <div style="width: 100%">
                <div style="float: left; width: 50%">
                    <asp:TreeView ID="treeSystemView" runat="server" OnTreeNodePopulate="PopulateNode"
                        PopulateNodesFromClient="true" EnableClientScript="true">
                    </asp:TreeView>
                </div>
                <div style="vertical-align: top; float: left; width: 50%">
                    <p>
                        Query Text</p>
                    <asp:TextBox ID="txtQueryText" runat="server" Width="100%" TextMode="MultiLine" Rows="8"></asp:TextBox>
                </div>
            </div>
            <div>
            <a href='javascript:toggleFieldMetadata()'>Edit Field Metadata</a>
            </div>
            <div id="field_metadata" style="display: none">
                <asp:Table ID="tblFieldMetadata" runat="server">
                    <asp:TableHeaderRow>
                        <asp:TableHeaderCell>Source Column</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Name</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Data Type</asp:TableHeaderCell>
                        <asp:TableHeaderCell ToolTip="The unit of measure for this value.">Units</asp:TableHeaderCell>
                        <asp:TableHeaderCell></asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                </asp:Table>
            </div>
            <div>
                <table border="0">
                    <tr>
                        <td>
                            <asp:LinkButton ID="btnSubmitView" runat="server" OnClick="btnSubmitView_Click" CssClass="squarebutton"><span>Submit</span></asp:LinkButton>
                        </td>
                        <td style="padding-left: 20px">
                            <asp:LinkButton ID="btnSaveView" runat="server" OnClick="btnSaveView_Click" CssClass="squarebutton"><span>Save View</span></asp:LinkButton>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="preview" runat="server">
                <asp:Table ID="tblPreviewData" runat="server">
                </asp:Table>
            </div>
            <asp:HiddenField ID="tableSpecification" runat="server" />
            <asp:HiddenField ID="fieldMetadata" runat="server" />
            <asp:HiddenField ID="newMetrics" runat="server" />
            <asp:HiddenField ID="addMetadataTarget" runat="server" />
            <div id="fieldMetadataWindow" style="display: none; position: absolute; left: 0px;
                top: 0px; padding: 16px; background: #FFFFFF; border: 2px solid #2266AA; z-index: 100;">
                <table border="0">
                    <tr>
                        <td>
                            Instrument:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_instrument" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Observation Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_observation" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Analysis Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_analysis" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Processing Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_processing" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Citations:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_citations" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_description" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <a class="squarebutton" href="javascript:commitMetadata()"><span>Save Metadata</span></a>
                        </td>
                        <td>
                            <a class="squarebutton" href="javascript:cancelMetadata()"><span>Cancel</span></a>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="newMetricWindow" style="display: none; position: absolute; left: 0px; top: 0px;
                        padding: 16px; background: #FFFFFF; border: 2px solid #2266AA; z-index: 100;">
                        <table border="0">
                            <tr>
                                <td>
                                    Metric Name:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtNewMetricName" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Metric Abbreviation:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtNewMetricAbbrev" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Metric Type:
                                </td>
                                <td>
                                    <asp:DropDownList ID="comboMetricType" runat="server">
                                        <asp:ListItem Text="Integer" Value="1" />
                                        <asp:ListItem Text="Decimal" Value="2" />
                                        <asp:ListItem Text="Text" Value="3" />
                                        <asp:ListItem Text="Datetime" Value="4" />
                                        <asp:ListItem Text="Time" Value="6" />
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <a class="squarebutton" href="javascript:commitNewMetric()"><span>Save Metric</span></a>
                                </td>
                                <td>
                                    <a class="squarebutton" href="javascript:cancelNewMetric()"><span>Cancel</span></a>
                                </td>
                            </tr>
                        </table>
                    </div>
        </div>
    </div>
    </form>
</body>
</html>
