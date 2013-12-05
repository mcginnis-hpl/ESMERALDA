<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditPerson.aspx.cs" Inherits="ESMERALDA.EditPerson" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>HPL Data Repository</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <div id="login" runat="server">
                <h3>
                    Existing Users</h3>
                    <p>Type in your assigned username and password below.  If you do not have a username and password, <a href='mailto:smcginnis@umces.edu??subject=Account Request'>please click here to request one.</a>  Keep in mind that you do not need a login to view or download data.</p>
                <table border="0px">
                    <tr>
                        <td>
                            Email Address:
                        </td>
                        <td>
                            <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Password:
                        </td>
                        <td>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" OnTextChanged="txtPassword_TextChanged"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Stay logged in:
                        </td>
                        <td>
                            <asp:CheckBox ID="chkPersistCookie" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:LinkButton ID="btnLogon" runat="server" OnClick="btnLogon_Click" CssClass="squarebutton"><span>Sign In</span></asp:LinkButton>
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
            </div>
            <div id="persondata" runat="server">
                <h3>User Information</h3>
                <table border="1px">
                    <tr>
                        <td>
                            Email Address:
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtEmail" runat="server" Width="363px"></asp:TextBox>
                        </td>
                        <td>
                            User ID:
                        </td>
                        <td>
                            <asp:Label ID="lblUserID" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Password:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtPasswordNew" TextMode="Password" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Re-enter Password:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtPasswordConfirm" TextMode="Password" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            First Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
                        </td>                        
                        <td>
                            Last Name:
                        </td>
                        <td>
                            <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
                        </td>
                        <td></td><td></td>
                    </tr>
                    <tr>
                        <td>
                            Honorific:
                        </td>
                        <td>
                            <asp:TextBox ID="txtHonorific" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Affiliation
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtAffiliation" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Address 1:
                        </td>
                        <td colspan="5">
                            <asp:TextBox ID="txtAddress1" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Address 2:
                        </td>
                        <td colspan="5">
                            <asp:TextBox ID="txtAddress2" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            City:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtCity" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            State:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtState" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            ZIP:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtZIP" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Country:
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtCountry" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Phone:
                        </td>
                        <td>
                            <asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            Fax:
                        </td>
                        <td>
                            <asp:TextBox ID="txtFax" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Comments:
                        </td>
                        <td colspan="5">
                        </td>
                    </tr>
                    <tr>
                        <td colspan="6">
                            <asp:TextBox ID="txtComments" runat="server" Height="67px" TextMode="MultiLine" Width="700px"></asp:TextBox>
                        </td>
                    </tr>                    
                    
                </table>
                    <table class="inlinemenu">
                    <tr>
                        <td id="celllogout" runat="server">
                            <asp:LinkButton ID="btnLogout" runat="server" CssClass="squarebutton" 
                                onclick="btnLogout_Click"><span>Log out</span></asp:LinkButton>
                        </td>
                        <td>
                            <asp:LinkButton ID="btnSave" runat="server" OnClick="btnSave_Click" CssClass="squarebutton"><span>Save Information</span></asp:LinkButton>
                        </td>
                        <td>
                            <asp:LinkButton ID="btnNewUser" runat="server" onclick="btnNewUser_Click" CssClass="squarebutton"><span>Create a new User</span></asp:LinkButton>
                        </td>
                        <td>
                        Edit an existing user: 
                            <asp:DropDownList ID="comboUserList" AutoPostBack="true" runat="server" 
                                onselectedindexchanged="comboUserList_SelectedIndexChanged">
                            </asp:DropDownList>
                        </td>
                        <td>
                        </td>
                    </tr>
                    </table>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
