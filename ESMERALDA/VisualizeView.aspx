<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VisualizeView.aspx.cs"
    Inherits="ESMERALDA.VisualizeView" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Visualize a View</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script type="text/javascript">
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

        function addField() {
            var reldiv = document.getElementById("colordiv");
            var ap = document.getElementById('addlink');
            document.getElementById('<%=txtColor.ClientID %>').value = "";
            reldiv.style.display = "inherit";

            reldiv.style.top = (getOffset(ap).top) + "px";
            document.getElementById('<%=txtColor.ClientID %>').focus();
        }

        function cancelAddUser() {
            document.getElementById('<%=txtColor.ClientID %>').value = "";
            series.checked = false;
            var reldiv = document.getElementById("colordiv");
            reldiv.style.display = "none";
        }

        function removeField() {
            var list = document.getElementById('<%=listSelectedFields.ClientID %>');
            var i = 0;
            var user_list = document.getElementById('<%=fieldValues.ClientID %>').value.split("|");
            var rel_list = document.getElementById('<%=colorValues.ClientID %>').value.split("|");
            var series_list = document.getElementById('<%=seriesValues.ClientID %>').value.split("|");

            while (i < list.options.length) {
                var item = list.options[i];
                if (item.selected) {
                    list.remove(i);
                    user_list.splice(i, 1);
                    rel_list.splice(i, 1);
                    series_list.splice(i, 1);
                }
                else {
                    i = i + 1;
                }
            }
            var new_field_list = "";
            var new_color_list = "";
            var new_series_list = "";
            if (user_list.length > 0) {
                new_field_list = user_list[0];
                new_color_list = rel_list[0];
                new_series_list = series_list[0];

                for (i = 1; i < user_list.length; i++) {
                    new_field_list = new_field_list + "|" + user_list[i];
                    new_color_list = new_color_list + "|" + rel_list[i];
                    new_series_list = new_series_list + "|" + series_list[i];
                }
            }
            document.getElementById('<%=fieldValues.ClientID %>').value = new_field_list;
            document.getElementById('<%=colorValues.ClientID %>').value = new_color_list;
            document.getElementById('<%=seriesValues.ClientID %>').value = new_series_list;
        }

        function commitAddField() {
            var list = document.getElementById('<%=listAvailableFields.ClientID %>');
            var rel = document.getElementById('<%=txtColor.ClientID %>').value;
            var series = document.getElementById('<%=chkSeries.ClientID %>');

            var new_field = "";
            var new_color = "";
            var new_series = "";

            var field_id = new Array();
            var field_name = new Array();
            var i = 0;
            while (i < list.options.length) {
                var item = list.options[i];
                if (item.selected) {
                    field_id.push(item.value);
                    field_name.push(item.text);

                    if (new_field.length <= 0) {
                        new_field = item.value;
                        new_color = rel;
                        if (series.checked) {
                            new_series = "1";
                        }
                        else {
                            new_series = "0";
                        }                      
                    }
                    else {
                        new_field = new_field + "|" + item.value;
                        new_color = new_color + "|" + rel;
                        if (series.checked) {
                            new_series = new_series + "|1";
                        }
                        else {
                            new_series = new_series + "|0";
                        }
                    }
                }
                i += 1;
            }
            if (document.getElementById('<%=fieldValues.ClientID %>').value.length == 0) {
                document.getElementById('<%=fieldValues.ClientID %>').value = new_field;
                document.getElementById('<%=colorValues.ClientID %>').value = new_color;
                document.getElementById('<%=seriesValues.ClientID %>').value = new_series;
            }
            else {
                document.getElementById('<%=fieldValues.ClientID %>').value = document.getElementById('<%=fieldValues.ClientID %>').value + "|" + new_field;
                document.getElementById('<%=colorValues.ClientID %>').value = document.getElementById('<%=colorValues.ClientID %>').value + "|" + new_color;
                document.getElementById('<%=seriesValues.ClientID %>').value = document.getElementById('<%=seriesValues.ClientID %>').value + "|" + new_series;
            }
            var dest_list = document.getElementById('<%=listSelectedFields.ClientID %>');
            var myOption;
            for (i = field_id.length - 1; i >= 0; i--) {
                myOption = document.createElement("Option");
                myOption.text = field_name[i] + ": " + rel;
                myOption.value = field_id[i];
                dest_list.add(myOption);
            }

            var reldiv = document.getElementById("colordiv");
            reldiv.style.display = "none";
            document.getElementById('<%=txtColor.ClientID %>').value = "";
            series.checked = false;
        } 
        
        function wndsize() {
            var w = 0; var h = 0;
            //IE
            if (!window.innerWidth) {
                if (!(document.documentElement.clientWidth == 0)) {
                    //strict mode
                    w = document.documentElement.clientWidth; h = document.documentElement.clientHeight;
                } else {
                    //quirks mode
                    w = document.body.clientWidth; h = document.body.clientHeight;
                }
            } else {
                //w3c
                w = window.innerWidth; h = window.innerHeight;
            }
            var el = document.getElementById("pageWidth");
            el.value = w.toString() + "," + h.toString();
        }
    </script>
</head>
<body onload="wndsize()">
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent" style="width:100%">
            <div id="controls">
                <h4>
                    Create a Data Visualization</h4>
                    <p>Use the controls below to build a visualization.  To select a field from the dataset, pick the field's name in the left list and click the "->" button.  Enter a color in HTML format (#RRGGBB), or leave the color blank to let ESMERALDA pick the color of the dataset for you.  Click "Commit" to add the field.</p>
                    <p>The first field in the Selected list will be the "X" value for any graph (or the category, for bar and column graphs).</p>
                    <p>When you have entered all of your values, click "Create Graph" to view your visualization.</p>
                <table border="0px">
                    <tr>
                        <td colspan="1">
                            Graph Type
                        </td>
                        <td colspan="3">
                            <asp:DropDownList ID="comboGraphType" runat="server">
                            </asp:DropDownList>
                        </td>                        
                    </tr>
                    <tr>
                        <td>
                            Available Fields
                        </td>
                        <td></td>
                        <td>
                            Selected Fields
                        </td>
                        <td>
                            Colors
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:ListBox ID="listAvailableFields" runat="server" Height="180px"></asp:ListBox>
                        </td>
                        <td>
                            <table border='0'>
                                <tr><td><a id="addlink" href='javascript:addField()'>-></a></td></tr>
                                <tr><td><a href='javascript:removeField()'><-</a></td></tr>
                            </table>                            
                        </td>
                        <td>
                            <asp:ListBox ID="listSelectedFields" runat="server" Height="180px"></asp:ListBox>                            
                        </td>
                        <td>
                            <asp:ListBox ID="listColors" runat="server" Height="180px"></asp:ListBox>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" align="center">
                            <asp:LinkButton ID="btnCreateGraph" runat="server" OnClick="btnCreateGraph_Click"
                                CssClass="squarebutton"><span>Create Graph</span></asp:LinkButton>
                        </td>
                    </tr>
                </table>
            </div>
            <asp:Chart ID="msChart" runat="server">                    
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1">
                        <Area3DStyle />
                    </asp:ChartArea>
                </ChartAreas>
            </asp:Chart>
        </div>
    </div>
    <div id="colordiv" style="border: 1px solid #000; width:250px; position:absolute; margin:0 auto; background-color: #FFFFFF; display:none;">
        Color: <asp:TextBox ID="txtColor" runat="server" 
            Width="240px"></asp:TextBox>    
        <asp:CheckBox ID="chkSeries" runat="server" Text="This value is a series key." />
        <table border="0">
            <tr><td><a class='squarebutton' href='javascript:commitAddField()'><span>Commit</span></a></td><td><a class='squarebutton' href='javascript:cancelAddField()'><span>Cancel</span></a></td></tr>
        </table>        
    </div>
    <asp:HiddenField ID="fieldValues" runat="server" />
    <asp:HiddenField ID="colorValues" runat="server" />
    <asp:HiddenField ID="seriesValues" runat="server" />
    <asp:HiddenField ID="pageWidth" runat="server" />
    </form>
</body>
</html>
