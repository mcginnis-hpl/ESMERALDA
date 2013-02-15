<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditDataSet.aspx.cs" Inherits="ESMERALDA.EditDataSet" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Upload a Dataset</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript" language="javascript"></script>
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <script type="text/javascript" language="javascript">
        var newMetricColumn;
        var newMetricRow;
        var newMetricControlID;

        function populateMetric(typeControlID, targetControlID, newValueID) {
            var type_control = document.getElementById(typeControlID);
            var selected_type = type_control.value;

            var dropdown = document.getElementById(targetControlID);
            var selected_value = dropdown.value;

            while (dropdown.options.length > 0) {
                dropdown.options.remove(0);
            }

            var opt = document.createElement("option");
            opt.text = "";
            opt.value = "";
            dropdown.options.add(opt);

            if (!selected_type) {
                dropdown.selectedIndex = 0;
                return;
            }

            opt = document.createElement("option");
            opt.text = "New Metric";
            opt.value = "NEW";
            dropdown.options.add(opt);

            var newMetrics = document.getElementById("newMetrics");
            var rows = newMetrics.value.split(";");
            if (!rows)
                return;
            for (var i = 0; i < rows.length; i++) {
                var cols = rows[i].split("|");
                if (!cols) {
                    continue;
                }
                if (cols.length < 4) {
                    continue;
                }
                if (cols[3] != selected_type)
                    continue;
                opt = document.createElement("option");
                opt.text = cols[0] + "(" + cols[1] + ")";
                opt.value = cols[2];
                dropdown.options.add(opt);
                if (opt.value == selected_value) {
                    dropdown.selectedIndex = dropdown.options.length - 1;
                }
                if (newMetricControlID && newValueID) {
                    if (newMetricControlID == targetControlID && opt.value == newValueID) {
                        dropdown.selectedIndex = dropdown.options.length - 1;
                        newMetricControlID = null;
                    }
                }
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
        }

        function populateTimeFields() {
            var row = 0;
            while (true) {
                var dropdown = document.getElementById("header_subfield" + row);
                if (!dropdown) {
                    break;
                }
                var selected_value = dropdown.value;
                while (dropdown.options.length > 0) {
                    dropdown.options.remove(0);
                }
                var type_field = document.getElementById("header_unit" + row);
                if (type_field.value != "4") {
                    dropdown.style.display = 'none';
                    return;
                }
                dropdown.style.display = '';
                var opt = document.createElement("option");
                opt.text = "";
                opt.value = "";
                dropdown.options.add(opt);

                var row2 = 0;
                var found = false;
                while (true) {
                    var element = document.getElementById("header_unit" + row2);
                    if (!element) {
                        break;
                    }
                    if (element.value == "6") {
                        found = true;
                        opt = document.createElement("option");
                        var name_el = document.getElementById("header_name" + row2);
                        var source_el = document.getElementById("header_sourcename" + row2);
                        opt.text = name_el.value;
                        opt.value = source_el.innerText;
                        dropdown.options.add(opt);

                        if (opt.value == selected_value) {
                            dropdown.selectedIndex = dropdown.options.length - 1;
                        }
                    }
                    row2 += 1;
                }
                if (!found) {
                    dropdown.style.display = 'none';
                } else {
                    dropdown.style.display = '';
                }
                row += 1;
            }
        }

        function validateControls() {
            var creatdb_control = document.getElementById("btnCreateDataset");
            var name_field = document.getElementById("txtMetadata_Name");
            var valid = true;
            if (!name_field) {
                valid = false;
            }
            if (!name_field.value || name_field.value.length == 0) {
                valid = false;
            }
            var row = 0;
            var compare_value;
            var compare_control;
            while (true) {
                compare_value = "";
                compare_control = document.getElementById("header_sourcename" + row);
                if (!compare_control) {
                    break;
                }
                compare_control = document.getElementById("header_name" + row);
                compare_value = compare_control.value;
                if (!compare_value || compare_value.length <= 0) {
                    valid = false;
                    break;
                }
                compare_control = document.getElementById("header_unit" + row);
                compare_value = compare_control.value;
                if (!compare_value || compare_value.length <= 0) {
                    valid = false;
                    break;
                }
                compare_control = document.getElementById("header_metric" + row);
                compare_value = compare_control.value;
                if (!compare_value || compare_value.length <= 0) {
                    valid = false;
                    break;
                }
                row += 1;
            }
            if (valid) {
                creatdb_control.style.display = "";
            }
            else {
                creatdb_control.style.display = "none";
            }
        }

        function reloadForm() {
            var fu = document.getElementById("uploadFiles2");
            document.getElementById("uploadFiles2").innerHTML = fu.innerHTML;
            // document.getElementById('btnRefreshField').click();
            var element = document.getElementById("hiddenCommands");
            element.value = "REFRESH";
            __doPostBack('UpdatePanel1', '');
        }

        function onUpdating() {
            // get the divImage
            var panelProg = $get('divCreateSpinner');
            // set it to visible
            panelProg.style.display = '';
        }

        function onUpdated() {
            // get the divImage
            var panelProg = $get('divCreateSpinner');
            // set it to invisible
            panelProg.style.display = 'none';
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

        function showSheetSelector() {
            var popup = document.getElementById('spreadsheetSheetPicker');
            popup.style.display = '';
        }

        function replaceMetadataValue(column, row, value) {
            var spec = document.getElementById("fieldMetadata");
            var rows = spec.value.split("~");
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
                    newtable = newtable + "~" + newrow;
                }
                else {
                    newtable = newtable + "~" + rows[i];
                }
            }
            spec.value = newtable;
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
    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="6000">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div id="page_wrapper">
                <div id="pagecontent">
                    <div id="upload" runat="server">
                        <div id="uploadPrompt" runat="server">
                            <h4>
                                Upload a file:</h4>
                            Select a file to upload data from:
                        </div>
                        <asp:AsyncFileUpload ID="uploadFiles2" runat="server" OnUploadedComplete="ProcessUpload"
                            OnClientUploadComplete="reloadForm" ThrobberID="loadingGraphic" />
                        <asp:Button ID="btnRefreshField" runat="server" Text="Button" OnClick="btnRefreshField_Click"
                            Style="display: none" />
                        <div id="loadingGraphic" runat="server" style="border-style: none; display: none;">
                            <center>
                                <h3>
                                    Loading file...</h3>
                            </center>
                            <center>
                                <img src="img/loading.gif" width="50px" alt="Loading..." /></center>
                        </div>
                        <div id="deleteDataControl" runat="server">
                            <asp:LinkButton ID="btnDeleteExistingData" runat="server" OnClick="btnDeleteExistingData_Click">Delete Existing Data</asp:LinkButton>
                        </div>
                        <div id="divuploadedFiles">
                            <asp:Table ID="uploadedFiles" runat="server">
                            </asp:Table>
                        </div>
                    </div>
                    <div id="metadata" runat="server">
                        <h4>
                            Dataset metadata:</h4>
                        <table border="1px">
                            <tr>
                                <td>
                                    Name:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtMetadata_Name" runat="server"></asp:TextBox>
                                </td>
                                <td>
                                    Short Description:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtMetadata_ShortDescription" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Description:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtMetadata_Description" runat="server" Width="390px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Dataset URL:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtMetadata_URL" runat="server" Width="390px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Acquisition Description:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtMetadata_Acquisition" runat="server" Height="60px" TextMode="MultiLine"
                                        Width="390px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Processing Description:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtMetadata_Processing" runat="server" Height="60px" TextMode="MultiLine"
                                        Width="390px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Keywords (comma-separated):
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtKeywords" runat="server" Width="387px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Dataset ID:
                                </td>
                                <td>
                                    <asp:Label ID="lblMetadata_DatasetID" runat="server" Text=""></asp:Label>
                                </td>
                                <td>
                                    Project:
                                </td>
                                <td>
                                    <asp:DropDownList ID="comboProject" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Dataset is Public:
                                </td>
                                <td>
                                    <asp:DropDownList ID="comboMetadata_IsPublic" runat="server">
                                        <asp:ListItem Text="Yes" Value="true"></asp:ListItem>
                                        <asp:ListItem Text="No" Value="false"></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td>
                                </td>
                                <td>                                    
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="datafields" runat="server">
                        <h4>
                            Configure dataset fields:</h4>
                        <asp:Table ID="tblDataField" runat="server">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell>Source Column</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Name</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Data Type</asp:TableHeaderCell>
                                <asp:TableHeaderCell ToolTip="The unit of measure for this value.">Units</asp:TableHeaderCell>
                                <asp:TableHeaderCell ToolTip="Used to join Date and Time fields together.">Linked Field</asp:TableHeaderCell>
                                <asp:TableHeaderCell ToolTip="Include this column in the saved data set.">Include</asp:TableHeaderCell>
                                <asp:TableHeaderCell></asp:TableHeaderCell>
                            </asp:TableHeaderRow>
                        </asp:Table>
                        <asp:LinkButton ID="btnSaveTableConfig" runat="server" OnClick="btnSaveTableConfig_Click">Save Data Specification</asp:LinkButton>
                    </div>
                    <div id="saveControl" runat="server">
                        <asp:LinkButton ID="btnCreateDataset" runat="server" OnClick="btnCreateDataset_Click">Create Dataset</asp:LinkButton>
                    </div>
                    <div id="divCreateSpinner" class="centered" runat="server" style="border-width: 1; display: none;">
                            <center>
                                <h3>
                                    Creating Dataset...</h3>
                            </center>
                            <center>
                                <img src="img/loading.gif" width="50px" alt="Loading..." /></center>
                        </div>
                    <div id="preview" runat="server">
                        <asp:Table ID="previewTable" runat="server" CssClass="preview">
                        </asp:Table>
                    </div>
                    <asp:HiddenField ID="tableSpecification" runat="server" />
                    <asp:HiddenField ID="newMetrics" runat="server" />
                    <asp:HiddenField ID="hiddenCommands" runat="server" />
                    <asp:HiddenField ID="fieldMetadata" runat="server" />
                    <asp:HiddenField ID="addMetadataTarget" runat="server" />
                    <div id="fieldMetadataWindow" style="display: none; position: absolute; left: 0px; top: 0px;
                        padding: 16px; background: #FFFFFF; border: 2px solid #2266AA; z-index: 100;">
                        <table border="0">
                            <tr><td>Instrument:</td><td>
                                <asp:TextBox ID="txt_meta_instrument" runat="server"></asp:TextBox></td></tr>
                            <tr><td>Observation Methodology:</td><td>
                                <asp:TextBox ID="txt_meta_observation" runat="server"></asp:TextBox></td></tr>                            
                            <tr><td>Analysis Methodology:</td><td>
                                <asp:TextBox ID="txt_meta_analysis" runat="server"></asp:TextBox></td></tr>
                            <tr><td>Processing Methodology:</td><td>
                                <asp:TextBox ID="txt_meta_processing" runat="server"></asp:TextBox></td></tr>
                            <tr><td>Citations:</td><td>
                                <asp:TextBox ID="txt_meta_citations" runat="server"></asp:TextBox></td></tr>
                                <tr><td>Description:</td><td>
                                <asp:TextBox ID="txt_meta_description" runat="server"></asp:TextBox></td></tr>
                                <tr>
                                <td>
                                    <a href="javascript:commitMetadata()">Save Metadata</a>
                                </td>
                                <td>
                                    <a href="javascript:cancelMetadata()">Cancel</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="spreadsheetSheetPicker" style="display:none; position: absolute; left: 50%; top: 50%;
                        padding: 16px; background: #FFFFFF; border: 2px solid #2266AA; z-index: 100; width:400px; height:50px; margin-left:-200px; margin-top:-25px;">
                        <table border="0">
                            <tr><td>Please select the sheet to use:</td><td>
                                <asp:DropDownList ID="comboSpreadsheetSheets" runat="server">
                                </asp:DropDownList>
                                <asp:Button ID="btnSelectSheet" runat="server" Text="OK" 
                                    onclick="btnSelectSheet_Click" />
                               </td></tr>
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
                                    <a href="javascript:commitNewMetric()">Save Metric</a>
                                </td>
                                <td>
                                    <a href="javascript:cancelNewMetric()">Cancel</a>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanelAnimationExtender ID="UpdatePanelAnimationExtender1"
        TargetControlID="UpdatePanel1" runat="server">
        <Animations>
            <OnUpdating>
               <Parallel duration="0">
                    <ScriptAction Script="onUpdating();" />
                    <EnableAction AnimationTarget="btnCreateDataset" Enabled="false" />                   
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration="0">
                    <ScriptAction Script="onUpdated();" />
                    <EnableAction AnimationTarget="btnCreateDataset" Enabled="true" />
                </Parallel>
            </OnUpdated>
        </Animations>
        </asp:UpdatePanelAnimationExtender>       
    </form>
</body>
</html>