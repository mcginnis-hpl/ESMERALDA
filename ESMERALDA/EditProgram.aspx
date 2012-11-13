<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditProgram.aspx.cs" Inherits="ESMERALDA.EditProgram" %>

<%@ Register Assembly="SlimeeLibrary" Namespace="SlimeeLibrary" TagPrefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="jquery-1.4.2.min.js" type="text/javascript" language="javascript"></script>
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script> 
    <title>Edit a Program</title>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">    
    <div id="pagecontent">
    <div id="metadata" runat="server">
        <h4>
            Program metadata:</h4>
        <table border="1px">
            <tr>
                <td>
                    Name:
                </td>
                <td>
                    <asp:TextBox ID="txtMetadata_Name" runat="server"></asp:TextBox>
                </td>
                <td>
                    Acronym:
                </td>
                <td>
                    <asp:TextBox ID="txtMetadata_Acronym" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Description
                </td>
                <td colspan="3">
                    <asp:TextBox ID="txtMetadata_Description" runat="server" Width="600px"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    URL:
                </td>
                <td colspan="3">
                    <asp:TextBox ID="txtMetadata_URL" runat="server" Width="597px"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Start Date:
                </td>
                <td>
                    <cc1:DatePicker ID="controlStartDate" runat="server" AutoPostBack="true" Width="100px" PaneWidth="150px">
                        <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                        <PaneHeaderStyle BackColor="#0099FF" />
                        <TitleStyle ForeColor="White" Font-Bold="true" />
                        <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                        <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                        <DayHeaderStyle BackColor="#E8E8E8" />
                        <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99"/>
                        <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false"/>
                        <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false"/>
                    </cc1:DatePicker>
                </td>
                <td>
                    End Date:
                </td>
                <td>
                    <cc1:DatePicker ID="controlEndDate" runat="server" AutoPostBack="true" Width="100px" PaneWidth="150px">
                        <PaneTableStyle BorderColor="#707070" BorderWidth="1px" BorderStyle="Solid" />
                        <PaneHeaderStyle BackColor="#0099FF" />
                        <TitleStyle ForeColor="White" Font-Bold="true" />
                        <NextPrevMonthStyle ForeColor="White" Font-Bold="true" />
                        <NextPrevYearStyle ForeColor="#E0E0E0" Font-Bold="true" />
                        <DayHeaderStyle BackColor="#E8E8E8" />
                        <TodayStyle BackColor="#FFFFCC" ForeColor="#000000" Font-Underline="false" BorderColor="#FFCC99"/>
                        <AlternateMonthStyle BackColor="#F0F0F0" ForeColor="#707070" Font-Underline="false"/>
                        <MonthStyle BackColor="" ForeColor="#000000" Font-Underline="false"/>
                    </cc1:DatePicker>
                </td>
            </tr>
            <tr>
                <td>
                    Logo URL:
                </td>
                <td>
                    <asp:TextBox ID="txtMetadata_LogoURL" runat="server"></asp:TextBox>
                </td>
                <td>
                    Small Logo URL:
                </td>
                <td>
                    <asp:TextBox ID="txtMetadata_SmallLogoURL" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>Program ID:</td>
                <td>
                    <asp:Label ID="lblProgramID" runat="server" Text=""></asp:Label></td>
                <td>Database Name:</td>
                <td><asp:Label ID="lblMetadata_DatabaseName" runat="server" Text="Label"></asp:Label></td>
            </tr>
        </table>
        <asp:LinkButton ID="btnSave" runat="server" onclick="btnSave_Click">Save Metadata</asp:LinkButton>
    </div>
    <div id="projects" runat="server">
    </div>
    <div id="addProjectControl" runat="server">
    </div>
    </div>
    </div>
    </form>
</body>
</html>
