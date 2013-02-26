<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VisualizeView.aspx.cs" Inherits="ESMERALDA.VisualizeView" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
        <title>Visualize a View</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
    <script type="text/javascript">

        // Load the Visualization API and the piechart package.
        google.load('visualization', '1.0', { 'packages': ['corechart'] });

        // Set a callback to run when the Google Visualization API is loaded.
        google.setOnLoadCallback(drawChart);

        // Callback that creates and populates a data table,
        // instantiates the pie chart, passes in the data and
        // draws it.
        function drawChart() {
            var chart_type = document.getElementById("chartType").value;

            var label_control = document.getElementById("labels");
            var label_tokens = label_control.value.split("~");
            var type_control = document.getElementById("types");
            var type_tokens = type_control.value.split("~");

            var data_string = document.getElementById("points").value;
            if (!data_string || data_string.length == 0)
                return;
            var data = new google.visualization.DataTable();

            var i = 0;
            for (i = 0; i < label_tokens.length; i++) {
                if (type_tokens[i] == "Integer" || type_tokens[i] == "Decimal") {
                    data.addColumn('number', label_tokens[i]);
                }
                else if (type_tokens[i] == "DateTime" || type_tokens[i] == "Time") {
                    if (chart_type == "BAR" || chart_type == "COLUMN") {
                        data.addColumn('string', label_tokens[i]);
                    }
                    else {
                        data.addColumn('date', label_tokens[i]);
                    }
                }
                else {
                    data.addColumn('string', label_tokens[i]);
                }
            }
                        
            var rows = data_string.split(";");
            if (!rows)
                return;
            for (var i = 0; i < rows.length; i++) {
                new_row = new Array();
                var points = rows[i].split(",");
                for (var j = 0; j < points.length; j++) {
                    if (type_tokens[j] == "Integer" || type_tokens[j] == "Decimal") {
                        new_row.push(Number(points[j]));
                    }
                    else if (type_tokens[j] == "DateTime" || type_tokens[j] == "Time") {
                        if (chart_type == "BAR" || chart_type == "COLUMN") {
                            new_row.push(points[j]);
                        }
                        else {
                            new_row.push(new Date(Date.parse(points[j])));
                        }
                    }
                    else {
                        new_row.push(points[j]);
                    }
                }                
                data.addRow(new_row);
            }

            if (chart_type == "SCATTER") {
                var options = {
                    title: label_tokens[0] + ' vs. ' + label_tokens[1],
                    hAxis: { title: label_tokens[0] },
                    vAxis: { title: label_tokens[1] },
                    legend: 'none'
                };
                var chart = new google.visualization.ScatterChart(document.getElementById('chart_div'));
                chart.draw(data, options);
            }
            else if (chart_type == "LINE") {
                var options = {
                    title: label_tokens[0] + ' vs. ' + label_tokens[1],
                    hAxis: { title: label_tokens[0] },
                    vAxis: { title: label_tokens[1] },
                    legend: 'none'
                };
                var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
                chart.draw(data, options);
            }
            else if (chart_type == "COLUMN") {
                var options = {
                    title: label_tokens[0] + ' vs. ' + label_tokens[1],
                    hAxis: { title: label_tokens[0], titleTextStyle: { color: 'red'} }
                };

                var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
                chart.draw(data, options);
            }
            else if (chart_type == "BAR") {
                var options = {
                    title: label_tokens[0] + ' vs. ' + label_tokens[1],
                    vAxis: { title: label_tokens[0], titleTextStyle: { color: 'red'} }
                };

                var chart = new google.visualization.BarChart(document.getElementById('chart_div'));
                chart.draw(data, options);
            }
        }

        function handleTypeChange() {
            var chart_type = document.getElementById("comboGraphType").value;
            if (chart_type == "SCATTER" || chart_type == "LINE") {
                var el = document.getElementById("control_xAxis");
                el.style.display = 'inherit';
                el = document.getElementById("control_yAxis");
                el.style.display = 'inherit';
                el = document.getElementById("control_field1");
                el.style.display = 'none';
                el = document.getElementById("control_field2");
                el.style.display = 'none';
                el = document.getElementById("control_field3");
                el.style.display = 'none';
                el = document.getElementById("control_field4");
                el.style.display = 'none';
            }
            else if (chart_type == "COLUMN" || chart_type == "BAR") {
                var el = document.getElementById("control_xAxis");
                el.style.display = 'none';
                el = document.getElementById("control_yAxis");
                el.style.display = 'none';
                el = document.getElementById("control_field1");
                el.style.display = 'inherit';
                el = document.getElementById("control_field2");
                el.style.display = 'inherit';
                el = document.getElementById("control_field3");
                el.style.display = 'inherit';
                el = document.getElementById("control_field4");
                el.style.display = 'inherit';
            }
        }
    </script>
</head>
<body onload="handleTypeChange()">
    <form id="form1" runat="server">
        <div id="page_wrapper">
            <div id="pagecontent">
                <div id="controls">
                    <h4>Create a Data Visualization</h4>
                    <table border="0px">
                        <tr>
                            <td>Graph Type</td>
                            <td>
                                <asp:DropDownList ID="comboGraphType" runat="server" onchange="handleTypeChange()">
                                    <asp:ListItem Text="Scatter" Value="SCATTER"></asp:ListItem>
                                    <asp:ListItem Text="Line" Value="LINE"></asp:ListItem>
                                    <asp:ListItem Text="Column" Value="COLUMN"></asp:ListItem>
                                    <asp:ListItem Text="Bar" Value="BAR"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_xAxis">
                            <td>X Axis:</td>
                            <td>
                                <asp:DropDownList ID="comboXAxis" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_yAxis">
                            <td>Y Axis:</td>
                            <td>
                                <asp:DropDownList ID="comboYAxis" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_field1">
                            <td>Field 1:</td>
                            <td>
                                <asp:DropDownList ID="comboField1" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_field2">
                            <td>Field 2:</td>
                            <td>
                                <asp:DropDownList ID="comboField2" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_field3">
                            <td>Field 3:</td>
                            <td>
                                <asp:DropDownList ID="comboField3" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="control_field4">
                            <td>Field 4:</td>
                            <td>
                                <asp:DropDownList ID="comboField4" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" align="center">
                                <asp:LinkButton ID="btnCreateGraph" runat="server" 
                                    onclick="btnCreateGraph_Click" CssClass="squarebutton"><span>Create Graph</span></asp:LinkButton></td>
                        </tr>
                    </table>
                </div>
                <div id="chart_div">
                </div>
            </div>
        </div>
        <asp:HiddenField ID="points" runat="server" />
        <asp:HiddenField ID="labels" runat="server" />
        <asp:HiddenField ID="types" runat="server" />
        <asp:HiddenField ID="chartType" runat="server" />
    </form>
</body>
</html>
