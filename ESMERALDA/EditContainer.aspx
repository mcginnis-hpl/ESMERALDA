<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditContainer.aspx.cs"
    Inherits="ESMERALDA.EditContainer" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="SlimeeLibrary" Namespace="SlimeeLibrary" TagPrefix="cc1" %>
<%@ Register TagPrefix="pc" TagName="PersonChooser" Src="~/PersonChooser.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript" language="javascript"></script>
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <script type="text/javascript" language="javascript">
        function togglePeople() {
            var el = document.getElementById("choose_container");
            var link = document.getElementById("person_link");

            if (el.style.display == 'none') {
                el.style.display = '';
                link.innerHTML = "<a href='javascript:togglePeople()'>Hide People</a>";
            }
            else {
                el.style.display = 'none';
                link.innerHTML = "<a href='javascript:togglePeople()'>Show People</a>";
            }
        }

        function reloadForm() {
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
    </script>
    <title>Edit a Folder</title>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="6000">
    </asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div id="page_wrapper">
                <div id="pagecontent">
                    <div id="metadata" runat="server" class="clearfix">
                        <h4>
                            Folder metadata:</h4>
                        <span id="breadcrumb" runat="server"></span>
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
                                    <asp:TextBox ID="txtMetadata_Description" runat="server" Width="500px"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    URL:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="txtMetadata_URL" runat="server" Width="500px"></asp:TextBox>
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
                                <td colspan="2">
                                </td>
                                <td colspan="2">
                                    <table border="0">
                                        <tr>
                                            <td>
                                                Database Name:
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txtMetadata_DatabaseName" runat="server"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                Is a separate database:
                                            </td>
                                            <td>
                                                <asp:CheckBox ID="chkIsSeparateDatabase" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    People:<span id='person_link'><a href='javascript:togglePeople()'>Show People</a></span><br />
                                    <div id="choose_container" style="display: none">
                                        <pc:PersonChooser ID="chooser" runat="server" />
                                    </div>
                                </td>
                                <td colspan="2">
                                    <table border="0">
                                        <tr>
                                            <td>
                                                <asp:CheckBox ID="chkIsPublic" runat="server" Text="Folder is Public" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                Folder ID:
                                                <asp:Label ID="lblMetadata_projectid" runat="server" Text="Label"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr id="attachments" runat="server">
                                <td>
                                    Attachments:
                                </td>
                                <td colspan="3">
                                    <table border="0px">
                                        <tr runat="server" id="rowUpload">
                                            <td>
                                                <asp:AsyncFileUpload ID="uploadAttachment" runat="server" OnUploadedComplete="doUpload"
                                                    OnClientUploadComplete="reloadForm" ThrobberID="loadingGraphic" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <span id="filedownloadlink" runat="server"></span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                        <asp:LinkButton ID="btn_SaveMetadata" runat="server" OnClick="btn_SaveMetadata_Click"
                            CssClass="squarebutton"><span>Save Metadata</span></asp:LinkButton>
                    </div>
                    <br />
                    <div id="dataSets" runat="server" class="clearfix">
                        <div id="subContainers" runat="server">
                        </div>
                        <div id="currentDatasets" runat="server">
                        </div>
                        <div id="addDatasetControl" runat="server">
                        </div>
                        <div id="currentViews" runat="server">
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanelAnimationExtender ID="UpdatePanelAnimationExtender1" TargetControlID="UpdatePanel1"
        runat="server">
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
    <asp:HiddenField ID="hiddenCommands" runat="server" />
    </form>
</body>
</html>
