<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditJoin.aspx.cs" Inherits="ESMERALDA.EditJoin"
    ValidateRequest="false" EnableEventValidation="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <title>Edit a Join</title>
    <script type="text/javascript" language="javascript">
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

        function updateProject1() {
            var el = document.getElementById("comboSource1Project");
            if (!el || el.value.length == 0) {
                populateDataset1("");
            }
            else {
                populateDataset1(el.value);
            }
        }

        function populateDataset1(projid) {
            var url = "GetEntityList.aspx?TYPE=DATASET&PARENTID=" + projid;
            getURL(url, getDataset1Done);
        }

        function getDataset1Done() {
            if (xmlhttp.readyState == 4) {// 4 = "loaded"
                if (xmlhttp.status == 200) {// 200 = OK
                    var dropdown = document.getElementById("comboSource1Dataset");
                    while (dropdown.options.length > 0) {
                        dropdown.options.remove(0);
                    }
                    var opt = document.createElement("option");
                    opt.text = "";
                    opt.value = "";
                    dropdown.options.add(opt);

                    //xmlhttp.data and shtuff
                    var ds_list = "";
                    if (xmlhttp.data) {
                        ds_list = xmlhttp.data;
                    }
                    else if (xmlhttp.responseText) {
                        ds_list = xmlhttp.responseText;
                    }
                    var items = ds_list.split(";");
                    if (!items)
                        return;
                    var set_value = false;
                    for (var i = 0; i < items.length; i++) {
                        var tokens = items[i].split(",");
                        opt = document.createElement("option");
                        opt.text = tokens[1];
                        opt.value = tokens[0];
                        dropdown.options.add(opt);
                    }
                }
                else {
                    alert("Problem retrieving data");
                }
            }
        }

        function updateProject2() {
            var el = document.getElementById("comboSource2Project");
            if (!el || el.value.length == 0) {
                populateDataset2("");
            }
            else {
                populateDataset2(el.value);
            }
        }

        function populateDataset2(projid) {
            var url = "GetEntityList.aspx?TYPE=DATASET&PARENTID=" + projid;
            getURL(url, getDataset2Done);
        }

        function getDataset2Done() {
            if (xmlhttp.readyState == 4) {// 4 = "loaded"
                if (xmlhttp.status == 200) {// 200 = OK
                    var dropdown = document.getElementById("comboSource2Dataset");
                    while (dropdown.options.length > 0) {
                        dropdown.options.remove(0);
                    }
                    var opt = document.createElement("option");
                    opt.text = "";
                    opt.value = "";
                    dropdown.options.add(opt);

                    //xmlhttp.data and shtuff
                    var ds_list = "";
                    if (xmlhttp.data) {
                        ds_list = xmlhttp.data;
                    }
                    else if (xmlhttp.responseText) {
                        ds_list = xmlhttp.responseText;
                    }
                    var items = ds_list.split(";");
                    if (!items)
                        return;
                    var set_value = false;
                    for (var i = 0; i < items.length; i++) {
                        var tokens = items[i].split(",");
                        opt = document.createElement("option");
                        opt.text = tokens[1];
                        opt.value = tokens[0];
                        dropdown.options.add(opt);
                    }
                }
                else {
                    alert("Problem retrieving data");
                }
            }
        }

        function updateDataset1() {
            var el = document.getElementById("comboSource1Dataset");
            if (!el || el.value.length == 0) {
                populateField1("");
            }
            else {
                populateField1(el.value);
                var el2 = document.getElementById("selectedDS1");
                el2.value = el.value;
            }
        }

        function populateField1(dsid) {
            var url = "GetEntityList.aspx?TYPE=FIELD&PARENTID=" + dsid;
            getURL(url, getField1Done);
        }

        function getField1Done() {
            if (xmlhttp.readyState == 4) {// 4 = "loaded"
                if (xmlhttp.status == 200) {// 200 = OK
                    var dropdown = document.getElementById("comboSource1LinkingField");
                    while (dropdown.options.length > 0) {
                        dropdown.options.remove(0);
                    }
                    var opt = document.createElement("option");
                    opt.text = "";
                    opt.value = "";
                    dropdown.options.add(opt);

                    //xmlhttp.data and shtuff
                    var ds_list = "";
                    if (xmlhttp.data) {
                        ds_list = xmlhttp.data;
                    }
                    else if (xmlhttp.responseText) {
                        ds_list = xmlhttp.responseText;
                    }
                    var items = ds_list.split(";");
                    if (!items)
                        return;
                    var set_value = false;
                    for (var i = 0; i < items.length; i++) {
                        var tokens = items[i].split(",");
                        opt = document.createElement("option");
                        opt.text = tokens[1];
                        opt.value = tokens[0];
                        dropdown.options.add(opt);
                    }
                }
                else {
                    alert("Problem retrieving data");
                }
            }
        }

        function updateDataset2() {
            var el = document.getElementById("comboSource2Dataset");
            if (!el || el.value.length == 0) {
                populateField2("");
            }
            else {
                populateField2(el.value);
                var el2 = document.getElementById("selectedDS2");
                el2.value = el.value;
            }
        }

        function populateField2(dsid) {
            var url = "GetEntityList.aspx?TYPE=FIELD&PARENTID=" + dsid;
            getURL(url, getField2Done);
        }

        function getField2Done() {
            if (xmlhttp.readyState == 4) {// 4 = "loaded"
                if (xmlhttp.status == 200) {// 200 = OK
                    var dropdown = document.getElementById("comboSource2LinkingField");
                    while (dropdown.options.length > 0) {
                        dropdown.options.remove(0);
                    }
                    var opt = document.createElement("option");
                    opt.text = "";
                    opt.value = "";
                    dropdown.options.add(opt);

                    //xmlhttp.data and shtuff
                    var ds_list = "";
                    if (xmlhttp.data) {
                        ds_list = xmlhttp.data;
                    }
                    else if (xmlhttp.responseText) {
                        ds_list = xmlhttp.responseText;
                    }
                    var items = ds_list.split(";");
                    if (!items)
                        return;
                    var set_value = false;
                    for (var i = 0; i < items.length; i++) {
                        var tokens = items[i].split(",");
                        opt = document.createElement("option");
                        opt.text = tokens[1];
                        opt.value = tokens[0];
                        dropdown.options.add(opt);
                    }
                }
                else {
                    alert("Problem retrieving data");
                }
            }
        }

        function updateField1() {
            var el = document.getElementById("comboSource1LinkingField");
            if (!el || el.value.length == 0) {
                return;
            }
            else {
                var el2 = document.getElementById("selectedField1");
                el2.value = el.value;
            }
        }

        function updateField2() {
            var el = document.getElementById("comboSource2LinkingField");
            if (!el || el.value.length == 0) {
                return;
            }
            else {
                var el2 = document.getElementById("selectedField2");
                el2.value = el.value;
            }
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
            <div id="joininfo">
                <table border="0">
                    <tr>
                        <td>
                            Source 1 Project
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource1Project" runat="server" onchange="updateProject1()">
                            </asp:DropDownList>
                        </td>
                        <td>
                            Source 1 Dataset
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource1Dataset" runat="server" onchange="updateDataset1()">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Source 2 Project
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource2Project" runat="server" onchange="updateProject2()">
                            </asp:DropDownList>
                        </td>
                        <td>
                            Source 2 Dataset
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource2Dataset" runat="server" onchange="updateDataset2()">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Source 1 Linking Field
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource1LinkingField" runat="server" onchange="updateField1()">
                            </asp:DropDownList>
                        </td>
                        <td>
                            Source 2 Linking Field
                        </td>
                        <td>
                            <asp:DropDownList ID="comboSource2LinkingField" runat="server" onchange="updateField2()">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>Join Type:</td>
                        <td>
                            <asp:DropDownList ID="comboJoinType" runat="server">
                                <asp:ListItem Text="Matches Only" Value="0"></asp:ListItem>
                                <asp:ListItem Text="All Fields from Source 1" Value="1"></asp:ListItem>
                                <asp:ListItem Text="All Fields from Source 2" Value="2"></asp:ListItem>
                                <asp:ListItem Text="All Fields from Both Sources" Value="3"></asp:ListItem>
                            </asp:DropDownList>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                </table>
                <table border="0">
                    <tr>
                        <td>
                            <asp:LinkButton ID="btnSubmitJoin" runat="server" OnClick="btnSubmitJoin_Click">Submit</asp:LinkButton>
                        </td>
                        <td style="padding-left: 20px">
                            <asp:LinkButton ID="btnSaveJoin" runat="server" onclick="btnSaveJoin_Click">Save Join</asp:LinkButton>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="preview" runat="server">
                <asp:Table ID="tblPreviewData" runat="server">
                </asp:Table>
            </div>
            <asp:HiddenField ID="selectedDS1" runat="server" />
            <asp:HiddenField ID="selectedDS2" runat="server" />
            <asp:HiddenField ID="selectedField1" runat="server" />
            <asp:HiddenField ID="selectedField2" runat="server" />
        </div>
    </div>
    </form>
</body>
</html>
