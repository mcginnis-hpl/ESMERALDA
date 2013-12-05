<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ESMERALDA.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script type="text/javascript" language="javascript">
        function resizeIframe(newHeight) {
            document.getElementById('_contentframe').style.height = parseInt(newHeight, 10) + 20 + 'px';
        }

        function hasClass(ele, cls) {
            return ele.className.match(new RegExp('(\\s|^)' + cls + '(\\s|$)'));
        }
        function addClass(ele, cls) {
            if (!this.hasClass(ele, cls)) ele.className += " " + cls;
        }
        function removeClass(ele, cls) {
            if (hasClass(ele, cls)) {
                var reg = new RegExp('(\\s|^)' + cls + '(\\s|$)');
                ele.className = ele.className.replace(reg, ' ');
            }
        }

        function setActiveTab(tabname, url) {
            removeClass(document.getElementById("Welcomelink"), "current");
            removeClass(document.getElementById("Welcomeitem"), "current");
            removeClass(document.getElementById("Browselink"), "current");
            removeClass(document.getElementById("Browseitem"), "current");
            removeClass(document.getElementById("Searchlink"), "current");
            removeClass(document.getElementById("Searchitem"), "current");

            setContentSource(url);
            var activelinkname = tabname + "link";
            var activeitemname = tabname + "item";
            addClass(document.getElementById(activelinkname), "current");
            addClass(document.getElementById(activeitemname), "current");
        }

        function setContentSource(url) {
            document.getElementById('_contentframe').src = url;
        }
    </script>
</head>
<body>
<form id="form1" runat="server">
<div id="page_wrapper">

<div id="header_wrapper">

<div id="header">

<h1><span id="pagestring_shortname" runat="server"></span></h1>
<h2><span id="pagestring_fullname" runat="server"></span></h2>
<h3><span id="versionNumber" runat="server"></span></h3>
</div>

<div id="helplink">
<a href="mailto:smcginnis@umces.edu?Subject=ESMERALDA%20Issue">Report a problem.</a>
</div>

<div id="navcontainer">

<ul id="navlist">
<li id="Welcomeitem" class="current"><a id="Welcomelink" href="javascript:setActiveTab('Welcome', 'Welcome.aspx')" class="current">Welcome</a></li>
<li id="Browseitem"><a id="Browselink" href="javascript:setActiveTab('Browse', 'Browse.aspx')">Browse</a></li>
<li id="Searchitem"><a id="Searchlink" href="javascript:setActiveTab('Search', 'Search.aspx')">Search</a></li>
<li id="EditPersonitem"><a id="EditPersonlink" href="javascript:setActiveTab('EditPerson', 'EditPerson.aspx?MODE=LOGIN')">Log In</a></li>
</ul>
</div>
</div>

<div id="left_side" runat="server">

</div>

<div id="right_side">
&nbsp;
</div>

<div id="content">

<iframe id="_contentframe" src="welcome.aspx" style="width:100%; border-style:none"></iframe>

</div>

</div>
</form>
</body>
</html>
