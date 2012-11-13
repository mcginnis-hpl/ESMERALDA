<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditProject.aspx.cs" Inherits="ESMERALDA.EditProject" %>

<%@ Register Assembly="SlimeeLibrary" Namespace="SlimeeLibrary" TagPrefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript" language="javascript"></script> 
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script> 
    <title>Edit a Project</title>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">    
    <div id="pagecontent">
    <div id="metadata" runat="server">
    <h4>Project metadata:</h4>
    <table border="1px">            
        <tr>
            <td>Name:</td>
            <td>
                <asp:TextBox ID="txtMetadata_Name" runat="server"></asp:TextBox></td>
            <td>Acronym:</td>
            <td><asp:TextBox ID="txtMetadata_Acronym" runat="server"></asp:TextBox></td>
        </tr>
        <tr><td>Description</td><td colspan="3">
            <asp:TextBox ID="txtMetadata_Description" runat="server"></asp:TextBox></td></tr>
            <tr><td>URL:</td><td colspan="3">
                <asp:TextBox ID="txtMetadata_URL" runat="server"></asp:TextBox></td></tr>
                <tr><td>Start Date:</td><td>
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
                </td><td>End Date:</td><td>
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
                </td></tr>
                <tr><td>Logo URL:</td><td>
                    <asp:TextBox ID="txtMetadata_LogoURL" runat="server"></asp:TextBox></td><td>Small Logo URL:</td><td>
                        <asp:TextBox ID="txtMetadata_SmallLogoURL" runat="server"></asp:TextBox></td></tr>
                        <tr><td>Parent Program:</td><td>
                            <asp:DropDownList ID="comboParentProgram" runat="server">
                            </asp:DropDownList>
                        </td><td>Database Name:</td><td>
                            <asp:Label ID="txtMetadata_DatabaseName" runat="server" Text="Label"></asp:Label>
                            </td></tr>
    </table>        
    </div>
    <asp:LinkButton ID="btn_SaveMetadata" runat="server" 
            onclick="btn_SaveMetadata_Click">Save Metadata</asp:LinkButton>
            <br />
    <div id="dataSets" runat="server">
        <div id="currentDatasets" runat="server">
        </div>        
        <div id="addDatasetControl" runat="server"></div>
        <div id="currentViews" runat="server">
        </div>
    </div>
    </div>
    </div>
    </form>
</body>
</html>