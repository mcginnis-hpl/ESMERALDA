<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminPage.aspx.cs" Inherits="ESMERALDA.AdminPage" EnableEventValidation="false" %>

<%@ Register Assembly="SlimeeLibrary" Namespace="SlimeeLibrary" TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Admin Page</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <script type="text/javascript" language="javascript">
        function populateMetrics(input_value) {
            var type_control = document.getElementById("comboRowType");
            var selected_type = type_control.value;

            var dropdown = document.getElementById("comboRowMetric");
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
                if (opt.value == input_value) {
                    dropdown.selectedIndex = dropdown.options.length - 1;
                    selectedMetric = input_value;
                }
            }
        }

        function copyMetric() {
            var source_control = document.getElementById("comboRowMetric");
            var dest_control = document.getElementById("selectedMetric");
            dest_control.value = source_control.options[source_control.selectedIndex].value;
        }

        function RemoveRow(i) {
            var commands = document.getElementById("fieldCommands");
            commands.value = "DELETE:" + i;
            form1.submit();
        }

        function EditRow(i) {
            var commands = document.getElementById("fieldCommands");
            commands.value = "EDIT:" + i;
            form1.submit();
        }
    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <div id="program">
                <h4>
                    Program metadata:</h4>
                <table border="1px">
                    <tr>
                        <td>
                            Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_Name" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Acronym:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_Acronym" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtProgram_Description" runat="server" Width="600px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            URL:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtProgram_URL" runat="server" Width="597px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Start Date:
                        </td>
                        <td>
                            <cc1:DatePicker ID="controlStartDate" runat="server" AutoPostBack="true" Width="100px"
                                PaneWidth="150px">
                                <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                                <PaneHeaderStyle BackColor="#0099FF" />
                                <TitleStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                                <DayHeaderStyle BackColor="#E8E8E8" />
                                <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99" />
                                <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false" />
                                <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false" />
                            </cc1:DatePicker>
                        </td>
                        <td>
                            End Date:
                        </td>
                        <td>
                            <cc1:DatePicker ID="controlEndDate" runat="server" AutoPostBack="true" Width="100px"
                                PaneWidth="150px">
                                <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                                <PaneHeaderStyle BackColor="#0099FF" />
                                <TitleStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                                <DayHeaderStyle BackColor="#E8E8E8" />
                                <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99" />
                                <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false" />
                                <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false" />
                            </cc1:DatePicker>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Logo URL:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_LogoURL" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Small Logo URL:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_SmallLogoURL" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Program ID:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_ID" runat="server" OnTextChanged="txtProgram_ID_TextChanged" AutoPostBack="true"></asp:TextBox>
                        </td>
                        <td>
                            Database Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProgram_DatabaseName" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <asp:LinkButton ID="btnSaveProgram" runat="server" OnClick="btnSaveProgram_Click">Save Metadata</asp:LinkButton>
            </div>
            <div id="project">
                <h4>
                    Project metadata:</h4>
                <table border="1px">
                    <tr>
                        <td>
                            Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_Name" runat="server" Width="249px"></asp:TextBox>
                        </td>
                        <td>
                            Acronym:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_Acronym" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtProject_Description" runat="server" Width="476px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            URL:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtProject_URL" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Start Date:
                        </td>
                        <td>
                            <cc1:DatePicker ID="projectStartDate" runat="server" AutoPostBack="true" Width="100px"
                                PaneWidth="150px">
                                <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                                <PaneHeaderStyle BackColor="#0099FF" />
                                <TitleStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                                <DayHeaderStyle BackColor="#E8E8E8" />
                                <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99" />
                                <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false" />
                                <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false" />
                            </cc1:DatePicker>
                        </td>
                        <td>
                            End Date:
                        </td>
                        <td>
                            <cc1:DatePicker ID="projectEndDate" runat="server" AutoPostBack="true" Width="100px"
                                PaneWidth="150px">
                                <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                                <PaneHeaderStyle BackColor="#0099FF" />
                                <TitleStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                                <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                                <DayHeaderStyle BackColor="#E8E8E8" />
                                <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99" />
                                <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false" />
                                <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false" />
                            </cc1:DatePicker>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Logo URL:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_LogoURL" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Small Logo URL:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_SmallLogoURL" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Project ID:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_ID" runat="server" OnTextChanged="txtProject_ID_TextChanged" AutoPostBack="true"></asp:TextBox>
                        </td>
                        <td>
                            Database Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtProject_DatabaseName" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <asp:LinkButton ID="btn_SavProject" runat="server" OnClick="btn_SavProject_Click">Save Metadata</asp:LinkButton>
            </div>
            <div id="dataset">
                <h4>
                    Dataset metadata:</h4>
                <table border="1px">
                    <tr>
                        <td>
                            Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtDataset_Name" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Short Description:
                        </td>
                        <td>
                            <asp:TextBox ID="txtDataset_ShortDescription" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Description:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtDataset_Description" runat="server" Width="390px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Dataset URL:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtDataset_URL" runat="server" Width="390px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Acquisition Description:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtDataset_Acquisition" runat="server" Height="60px" TextMode="MultiLine"
                                Width="390px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Processing Description:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtDataset_Processing" runat="server" Height="60px" TextMode="MultiLine"
                                Width="390px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Keywords:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtDataset_Keywords" runat="server" Width="387px"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Dataset ID:
                        </td>
                        <td>
                            <asp:TextBox ID="txtDataset_ID" runat="server" 
                                ontextchanged="txtDataset_ID_TextChanged" AutoPostBack="true"></asp:TextBox>
                        </td>
                        <td>
                            Table name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtDataset_TableName" runat="server" 
                                ontextchanged="txtDataset_TableName_TextChanged" AutoPostBack="true"></asp:TextBox>
                        </td>
                    </tr>
                </table>
                <asp:Table ID="tblSpecification" runat="server">
                    <asp:TableHeaderRow ID="headerRow">
                        <asp:TableHeaderCell>Field Name</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Type</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Metric</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Source Column</asp:TableHeaderCell>                        
                        <asp:TableHeaderCell>SQL Column Name</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Instrument</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Observation Methodology</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Analysis Methodology</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Processing Methodology</asp:TableHeaderCell>
                        <asp:TableHeaderCell>Description</asp:TableHeaderCell>
                        <asp:TableHeaderCell></asp:TableHeaderCell>
                    </asp:TableHeaderRow>
                    <asp:TableRow ID="EditRow" runat="server">
                        <asp:TableCell>
                            <asp:TextBox ID="txtRowName" runat="server"></asp:TextBox></asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList ID="comboRowType" runat="server" onchange="populateMetrics(-1)">
                                <asp:ListItem Text="" Value="0"></asp:ListItem>
                                <asp:ListItem Text="Integer" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Decimal" Value="2"></asp:ListItem>
                                <asp:ListItem Text="Text" Value="3"></asp:ListItem>
                                <asp:ListItem Text="DateTime" Value="4"></asp:ListItem>
                                <asp:ListItem Text="Time" Value="6"></asp:ListItem>
                            </asp:DropDownList>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList ID="comboRowMetric" runat="server" onchange="copyMetric()"></asp:DropDownList>
                        </asp:TableCell>                        
                        <asp:TableCell>
                            <asp:TextBox ID="txtRowSourceColumn" runat="server"></asp:TextBox></asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="txtRowSQLColumn" runat="server"></asp:TextBox></asp:TableCell>
                            <asp:TableCell>
                            <asp:TextBox ID="txtRowInstrument" runat="server"></asp:TextBox></asp:TableCell>
                            <asp:TableCell>
                            <asp:TextBox ID="txtRowObservation" runat="server"></asp:TextBox></asp:TableCell>
                            <asp:TableCell>
                            <asp:TextBox ID="txtRowAnalysis" runat="server"></asp:TextBox></asp:TableCell>
                            <asp:TableCell>
                            <asp:TextBox ID="txtRowProcessing" runat="server"></asp:TextBox></asp:TableCell>
                            <asp:TableCell>
                            <asp:TextBox ID="txtRowCitations" runat="server"></asp:TextBox></asp:TableCell>
                        <asp:TableCell>
                            <asp:LinkButton ID="btnAddRow" runat="server" OnClick="btnAddRow_Click">Add Row</asp:LinkButton></asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <asp:HiddenField ID="newMetrics" runat="server" />
                <asp:HiddenField ID="selectedMetric" runat="server" />
                <asp:HiddenField ID="fieldCommands" runat="server" />
                <asp:LinkButton ID="btnSaveDataset" runat="server" 
                    onclick="btnSaveDataset_Click">Save Dataset</asp:LinkButton>
            </div>
        </div>        
    </div>
    </form>
</body>
</html>
