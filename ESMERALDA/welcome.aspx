<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="welcome.aspx.cs" Inherits="ESMERALDA.welcome" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">     
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script> 
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">    
    <div id="page_wrapper">
    <div id="pagecontent" class="clearfix" runat="server">    
    </div>
    <div id="adminLink" runat="server"><a href="AdminPage.aspx" class='squarebutton'><span>Admin Page</span></a></div>
    </div>
    </form>
</body>
</html>
