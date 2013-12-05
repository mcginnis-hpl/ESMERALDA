<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Browse.aspx.cs" Inherits="ESMERALDA.Browse" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script> 
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <h3>Top-Level Folders</h3>
                <p>Click on one of the folders below to browse that folder's sub-folders and datasets.</p>
            <div id="programList">
                <asp:Table ID="tblPrograms" runat="server">
                </asp:Table>
                <a href='EditContainer.aspx' class="squarebutton"><span>Create a New Folder</span></a>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
