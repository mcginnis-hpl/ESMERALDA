<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditLinkedFile.aspx.cs"
    Inherits="ESMERALDA.EditLinkedFile" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Link a File</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript" language="javascript"></script>
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
</head>
<body>
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
                        <div id="loadingGraphic" runat="server" style="border-style: none; display: none;">
                            <center>
                                <h3>
                                    Loading file...</h3>
                            </center>
                            <center>
                                <img src="img/loading.gif" width="50px" alt="Loading..." /></center>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    </form>
</body>
</html>
