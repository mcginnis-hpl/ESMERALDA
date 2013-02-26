<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewDataset.aspx.cs" Inherits="ESMERALDA.ViewDataset" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <title>View a Dataset</title>
    <script type="text/javascript" language="javascript">
        function updateFilterType(id) {
            var rownum = getRow(id);
            var controlname = "filtertype_" + id;
            var control = document.getElementById(controlname);
            if (control) {
                var newval = control.value;
                replaceValue(2, rownum, newval);
            }
            if (control.value == "2" || control.value == "3") {
                controlname = "filtertext_" + id;
                control = document.getElementById(controlname);
                control.style.display = 'none';

                controlname = "filterconversion_" + id;
                control = document.getElementById(controlname);
                control.style.display = 'none';
            }
            else if (control.value == "1" || control.value == "5") {
                controlname = "filtertext_" + id;
                control = document.getElementById(controlname);
                control.style.display = '';

                controlname = "filterconversion_" + id;
                control = document.getElementById(controlname);
                control.style.display = 'none';
            }
            else if (control.value == "6") {
                controlname = "filtertext_" + id;
                control = document.getElementById(controlname);
                control.style.display = 'none';

                controlname = "filterconversion_" + id;
                control = document.getElementById(controlname);
                control.style.display = '';
            }
        }

        function updateFilterText(id) {
            var rownum = getRow(id);
            var controlname = "filtertext_" + id;
            var control = document.getElementById(controlname);
            if (control) {
                var newval = control.value;
                replaceValue(3, rownum, newval);
            }
        }

        function updateFilterConversion(id) {
            var rownum = getRow(id);
            var controlname = "filterconversion_" + id;
            var control = document.getElementById(controlname);
            if (control) {
                var newval = control.value;
                replaceValue(3, rownum, newval);
            }
        }

        function updateFilterAlias(id) {
            var controlname = "filteralias_" + id;
            validateFieldName(controlname);
            var rownum = getRow(id);
            var control = document.getElementById(controlname);
            if (control) {
                var newval = control.value;
                replaceValue(4, rownum, newval);
            }
        }

        function validateFieldName(fieldname) {
            var control = document.getElementById(fieldname);
            if (!control) {
                return;
            }
            var regex = "^[a-zA-Z][a-zA-Z0-9_]*$";
            var tester = new RegExp(regex);
            if (!tester.test(control.value)) {
                alert(control.value + " is not a valid column name.");
                control.focus();
            }
        }

        function updateFilterInclude(id) {
            var rownum = getRow(id);
            var controlname = "filterinclude_" + id;
            var control = document.getElementById(controlname);
            controlname = "filtertype_" + id;
            var type_element = document.getElementById(controlname);
            if (control.checked) {
                replaceValue(2, rownum, 0);
            }
            else {
                replaceValue(2, rownum, 4);
            }
            controlname = "filtertext_" + id;
            control = document.getElementById(controlname);
            control.style.display = 'none';

            controlname = "filterconversion_" + id;
            control = document.getElementById(controlname);
            control.style.display = 'none';
        }

        function getRow(id) {
            var spec = document.getElementById("viewValues");
            var rows = spec.value.split(";");
            for (var i = 0; i < rows.length; i++) {
                var cols = rows[i].split("|");
                if (cols[0] == id)
                    return i;
            }
            return -1;
        }

        function replaceValue(column, row, value) {
            var spec = document.getElementById("viewValues");
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

        function updateFilterSourceField(id) {
            var rownum = getRow(id);
            var controlname = "filtersourcefield_" + id;
            var control = document.getElementById(controlname);
            if (control) {
                var newval = control.value;
                replaceValue(1, rownum, newval);
                var btn = document.getElementById("btnUpdate");
                if (btn) btn.click();
            }
        }

        function cancelMetadata() {
            var metricdiv = document.getElementById("fieldMetadataWindow");
            metricdiv.style.display = "none";
        }

        function showMetadata(controlID, index) {
            var meta = document.getElementById("fieldMetadata");
            var rows = meta.value.split("~");
            var i = 0;
            var cols = rows[index].split("|");
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
            var ap = document.getElementById(controlID);
            var popup = document.getElementById('fieldMetadataWindow');
            popup.style.display = '';
            popup.style.top = (getOffset(ap).top + 20) + "px";
            popup.style.left = (getOffset(ap).left + 20) + "px";
        }

        function addField(newfield) {
            var query = document.getElementById("txtQuery");
            if (!query.value || query.value.length == 0) {
                query.value = newfield;
            }
            else {
                if (query.value.substr(query.value.length - 1, 1) != " ") {
                    query.value = query.value + " ";
                }
                query.value = query.value + newfield;
            }
            query.focus();
        }

        function showHelp() {
            var el = document.getElementById("help");
            el.style.display = "";
            var linkel = document.getElementById("showhelplink");
            linkel.innerHTML = "<a class='squarebutton' href='javascript:hideHelp()'><span>Hide help.</span></a>";
        }

        function hideHelp() {
            var el = document.getElementById("help");
            el.style.display = "none";
            var linkel = document.getElementById("showhelplink");
            linkel.innerHTML = "<a class='squarebutton' href='javascript:showHelp()'><span>Show help.</span></a>";
        }

        function showSaveDialog() {
            var el = document.getElementById("downloadcontrols");
            el.style.display = "inherit";
        }

        function getOffset(el) {
            var _x = 0;
            var _y = 0;
            while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
                _x += el.offsetLeft - el.scrollLeft;
                _y += el.offsetTop - el.scrollTop;
                el = el.parentNode;
            }
            return { top: _y, left: _x };
        }

        function hideSaveDialog() {
            var el = document.getElementById("downloadcontrols");
            el.style.display = "none";
        }

    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <h4>
                View a Dataset</h4>
            <div class="clearfix">
                <span id="showhelplink"><a class='squarebutton' href='javascript:showHelp()'><span>Show
                    help.</span></a></span>
                <div id="help" style="display: none">
                    <p>
                        On this page, you can do the following:</p>
                    <ul>
                        <li>On this page, you can preview and filter a dataset. You do not need to give the
                            view a name or a description if you intend to save the view (this functionality
                            is for authenticated users only).</li>
                        <li>To sort your data, select "Sort Ascending" or "Sort Descending" from the Filter
                            Type dropdown beside the column you wish to sort.</li>
                        <li>To filter your data, select "Filter" from the Filter Type dropdown, and enter your
                            filter criteria in the "Filter Text" text box. Filter statements can use constants
                            (e.g., "< 10") or dynamic values (e.g., "< [Dissolved Oxygen]"). You can also combine
                            filter statements using "AND" or "OR" (e.g., "< 10 AND > 2"). To learn more about
                            filter statements, <a href='FilterStatements.htm' target='_blank'>click here</a>.</li>
                        <li>To apply a formula, select "Formula" from the Filter Type dropdown. You may then
                            enter the formula in the "Filter Text" box. The formula should use the field name
                            in brackets, e.g. "SIN([Frequency]) * COS([Amplitude]) + 1" where Amplitude and
                            Frequency are field names.</li>
                        <li>If you are familiar with SQL, you can also type your SQL query in the "Execute Query"
                            box.</li>
                        <li>Finally, you may change the number of rows to preview in the "Number of Rows to
                            Preview" text box.</li>
                        <li>To download you query, selected "Download as CSV" from the bottom of the page. Other
                            data formats are coming!</li>
                    </ul>
                </div>
            </div>
            <div id="metadata" runat="server">
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
                            <asp:Label ID="lblViewSQLName" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="filters">
                <asp:Table ID="filterTable" runat="server">
                </asp:Table>
                <asp:Table ID="controlmenu" runat="server" border="0px" CssClass="inlinemenu">
                    <asp:TableRow>
                        <asp:TableCell>
                            <asp:LinkButton ID="btnAddColumn" runat="server" OnClick="btnAddColumn_Click" CssClass="squarebutton"><span>Add Derived Column</span></asp:LinkButton>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:LinkButton ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" CssClass="squarebutton"><span>Update View</span></asp:LinkButton>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </div>
            <asp:HiddenField ID="fieldMetadata" runat="server" />
            <div id="fieldMetadataWindow" style="display: none; position: absolute; left: 0px;
                top: 0px; padding: 16px; background: #FFFFFF; border: 2px solid #2266AA; z-index: 100;">
                <table border="0">
                    <tr>
                        <td>
                            Instrument:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_instrument" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Observation Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_observation" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Analysis Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_analysis" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Processing Methodology:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_processing" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Citations:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_citations" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description:
                        </td>
                        <td>
                            <asp:TextBox ID="txt_meta_description" runat="server" ReadOnly="true"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <a class='squarebutton' href="javascript:cancelMetadata()"><span>Close</span></a>
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="query">
                <table border="0">
                    <tr>
                        <td colspan="2">
                            <span id="querytag" runat="server"></span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:TextBox ID="txtQuery" runat="server" Rows="6" TextMode="MultiLine" Width="619px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Table ID="querycontrols" runat="server" CssClass="inlinemenu">
                                <asp:TableRow>
                                    <asp:TableCell>
                                        Number of Rows to Preview:
                                        <asp:TextBox ID="txtRowsToRetrieve" runat="server" OnTextChanged="txtRowsToRetrieve_TextChanged"></asp:TextBox></asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </td>
                        <td>
                            <asp:LinkButton ID="btnExecuteQuery" runat="server" OnClick="btnExecuteQuery_Click"
                                CssClass="squarebutton"><span>Execute Query</span></asp:LinkButton>
                        </td>
                    </tr>
                </table>
            </div>
            <iframe id="testdatapreview" runat="server" style="width: 100%; height: 480px; overflow: scroll">
            </iframe>
            <span id="errormessage" runat="server"></span>
            <div id="downloadcontrols" class="downloadmenu">
                <span id="spanDownloadCSV" runat="server"></span>
            </div>
            <asp:HiddenField ID="viewValues" runat="server" />
        </div>
    </div>
    </form>
</body>
</html>
