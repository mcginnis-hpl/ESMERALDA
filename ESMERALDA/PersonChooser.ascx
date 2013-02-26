<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PersonChooser.ascx.cs" Inherits="ESMERALDA.PersonChooser" %>
<script language="javascript" type="text/javascript">
    function getOffset(el) {
        var _x = 0;
        var _y = 0;
        while (el && !isNaN(el.offsetLeft) && !isNaN(el.offsetTop)) {
            _x += el.offsetLeft - el.scrollLeft;
            _y += el.offsetTop - el.scrollTop;
            el = el.offsetParent;
        }
        return { top: _y, left: _x };
    }

    function addUser() {
        var reldiv = document.getElementById("relationshipdiv");
        var ap = document.getElementById('<%=listAvailableUsers.ClientID %>');
        document.getElementById('<%=txtRelationship.ClientID %>').value = "";
        reldiv.style.display = "inherit";

        reldiv.style.top = (getOffset(ap).top) + "px";
        document.getElementById('<%=txtRelationship.ClientID %>').focus();
    }

    function cancelAddUser() {
        var reldiv = document.getElementById("relationshipdiv");
        reldiv.style.display = "none";
    }

    function commitAddUser() {
        var list = document.getElementById('<%=listAvailableUsers.ClientID %>');
        var rel = document.getElementById('<%=txtRelationship.ClientID %>').value;
        var new_user = "";
        var new_rel = "";
        var user_id = new Array();
        var user_name = new Array();
        var i = 0;
        while (i < list.options.length) {
            var item = list.options[i];
            if (item.selected) {
                user_id.push(item.value);
                user_name.push(item.text);

                if (new_user.length <= 0) {
                    new_user = item.value;
                    new_rel = rel;
                }
                else {
                    new_user = new_user + "|" + item.value;
                    new_rel = new_rel + "|" + rel;
                }
            }
            i += 1;
        }
        if (document.getElementById('<%=userValues.ClientID %>').value.length == 0) {
            document.getElementById('<%=userValues.ClientID %>').value = new_user;
            document.getElementById('<%=relationshipValues.ClientID %>').value = new_rel;
        }
        else {
            document.getElementById('<%=userValues.ClientID %>').value = document.getElementById('<%=userValues.ClientID %>').value + "|" + new_user;
            document.getElementById('<%=relationshipValues.ClientID %>').value = document.getElementById('<%=relationshipValues.ClientID %>').value + "|" + new_rel;
        }
        var dest_list = document.getElementById('<%=listSelectedUsers.ClientID %>');
        var myOption;
        for (i = user_id.length - 1; i >= 0; i--) {
            myOption = document.createElement("Option");
            myOption.text = user_name[i] + ": " + rel;
            myOption.value = user_id[i];
            dest_list.add(myOption);
        }

        var reldiv = document.getElementById("relationshipdiv");
        reldiv.style.display = "none";
    }

    function removeUser() {
        var list = document.getElementById('<%=listSelectedUsers.ClientID %>');
        var i = 0;
        var user_list = document.getElementById('<%=userValues.ClientID %>').value.split("|");
        var rel_list = document.getElementById('<%=relationshipValues.ClientID %>').value.split("|");

        while (i < list.options.length) {
            var item = list.options[i];
            if (item.selected) {
                list.remove(i);
                user_list.splice(i, 1);
                rel_list.splice(i, 1);
            }
            else {
                i = i + 1;
            }
        }
        var new_user_list = "";
        var new_rel_list = "";
        if (user_list.length > 0) {
            new_user_list = user_list[0];
            new_rel_list = rel_list[0];

            for (i = 1; i < user_list.length; i++) {
                new_user_list = new_user_list + "|" + user_list[i];
                new_rel_list = new_rel_list + "|" + rel_list[i];
            }
        }
        document.getElementById('<%=userValues.ClientID %>').value = new_user_list;
        document.getElementById('<%=relationshipValues.ClientID %>').value = new_rel_list;
    }

</script>
<table border="0">
    <tr><th>Available People</th><th></th><th>Selected People</th></tr>
    <tr><td id="available" runat="server"><asp:ListBox ID="listAvailableUsers" runat="server"></asp:ListBox></td><td id="controls" runat="server"><center><a class='squarebutton' href='javascript:addUser()'><span>Add User</span></a></center><br /><center><a class='squarebutton' href='javascript:removeUser()'><span>Remove User</span></a></center></td>
    <td><asp:ListBox ID="listSelectedUsers" runat="server"></asp:ListBox></td></tr>
</table>
<div id="relationshipdiv" style="border: 1px solid #000; width:250px; position:absolute; margin:0 auto; background-color: #FFFFFF; display:none;">
    Relationship: <asp:TextBox ID="txtRelationship" runat="server" 
        Width="240px"></asp:TextBox>    
    <table border="0">
        <tr><td><a class='squarebutton' href='javascript:commitAddUser()'><span>Commit</span></a></td><td><a class='squarebutton' href='javascript:cancelAddUser()'><span>Cancel</span></a></td></tr>
    </table>        
</div>
<asp:HiddenField ID="userValues" runat="server" />
<asp:HiddenField ID="relationshipValues" runat="server" />
