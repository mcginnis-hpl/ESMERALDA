<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewDataset.aspx.cs" Inherits="ESMERALDA.ViewDataset" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="jquery-1.4.2.min.js" type="text/javascript" language="javascript"></script>
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
    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <h4>
                View a Dataset</h4>
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
                            <asp:Label ID="lblViewSQLName" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="filters">
                <asp:Table ID="filterTable" runat="server">
                </asp:Table>
                <table border="0px">
                    <tr><td><asp:LinkButton ID="btnAddColumn" runat="server" OnClick="btnAddColumn_Click">Add Derived Column</asp:LinkButton></td><td><asp:LinkButton ID="btnUpdate" runat="server" OnClick="btnUpdate_Click">Update View</asp:LinkButton></td></tr>
                </table>                                
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
                            <a href="javascript:cancelMetadata()">Close</a>
                        </td>
                        <td>                            
                        </td>
                    </tr>
                </table>
            </div>
            <div id="query">
                <span id="querytag" runat="server"></span><br />
                <asp:TextBox ID="txtQuery" runat="server" Rows="6" TextMode="MultiLine" 
                    Width="619px"></asp:TextBox>
                    <br />
                <asp:LinkButton ID="btnExecuteQuery" runat="server" 
                    onclick="btnExecuteQuery_Click">Execute Query</asp:LinkButton>
            </div>
            <div id="commoncontrols" runat="server">
            </div>
            <div id="datapreview">
                <div>
                    Number of Rows to Preview:
                    <asp:TextBox ID="txtRowsToRetrieve" runat="server" OnTextChanged="txtRowsToRetrieve_TextChanged"></asp:TextBox></div>
                <asp:Table ID="tblPreviewData" runat="server">
                </asp:Table>
                <span id="errormessage" runat="server"></span>
            </div>
            <div id="downloadcontrols">
                <span id="spanDownloadCSV" runat="server"></span>
            </div>
            <asp:HiddenField ID="viewValues" runat="server" />
        </div>
    </div>
    </form>
</body>
</html>