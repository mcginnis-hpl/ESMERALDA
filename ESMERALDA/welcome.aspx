<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="welcome.aspx.cs" Inherits="ESMERALDA.welcome" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
     <title>Welcome to ESMERALDA</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script> 
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">    
    <div id="page_wrapper">
    <div id="pagecontent" class="clearfix">
    <p>If you are looking for the Weather data, <a href='http://10.1.13.205:88/HPLWeatherData.aspx' target="_blank">click here.</a></p>
    <h3>Welcome!</h3>
    <p>Welcome to ESMERALDA, the Environmental Science Metadata-Enhanced Repository for Analysis, Lookup, and Data Archiving.  This application is intended to help you find the data you're looking for,
    share data with collaborators, and safely backup your datasets.</p>
    <p>To begin finding data, click the "Search" tab to search for data by keyword, or the "Browse" tab to see all of the programs available in the database.  You do not need a login to view or download data.</p>
    <p>To upload data, you will require a login.  If you have one, please click the "Login" tab at the top of the page.  If you do not, <a href='mailto:smcginnis@umces.edu??subject=ESMERALDA Account Request'>please click here to request one.</a>  To save your data, select one of the current programs from the menu on the left, or create a new program for your data.</p>
    <div id="adminLink" runat="server"><a href="AdminPage.aspx" class='squarebutton'><span>Admin Page</span></a></div>
    </div>
    </div>
    </form>
</body>
</html>
