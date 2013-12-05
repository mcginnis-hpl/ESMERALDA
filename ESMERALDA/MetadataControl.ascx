<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MetadataControl.ascx.cs"
    Inherits="ESMERALDA.MetadataControl" %>
<script language="javascript" type="text/javascript">
    var edit_holder = -1;

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

    function addMetadata() {
        var reldiv = document.getElementById("metadataDiv");
        var ap = document.getElementById('<%=listTags.ClientID %>');
        document.getElementById('<%=txtTag.ClientID %>').value = "";
        document.getElementById('<%=txtValue.ClientID %>').value = "";
        reldiv.style.display = "inherit";

        reldiv.style.top = (getOffset(ap).top) + "px";
        document.getElementById('<%=txtTag.ClientID %>').focus();
        edit_holder = -1;
    }

    function editMetadata() {
        var tag = "";
        var val = "";
        var i = 0;
        var tag_list = document.getElementById('<%=listTags.ClientID %>');
        var val_list = document.getElementById('<%=listValues.ClientID %>');
        while (i < tag_list.options.length) {
            var item = tag_list.options[i];
            if (item.selected) {
                tag = item.text;
                val = val_list.options[i].text;
                edit_holder = i;
                break;
            }
            else {
                i = i + 1;
            }
        }
        var reldiv = document.getElementById("metadataDiv");
        var ap = document.getElementById('<%=listTags.ClientID %>');
        document.getElementById('<%=txtTag.ClientID %>').value = tag;
        document.getElementById('<%=txtValue.ClientID %>').value = val;
        reldiv.style.display = "inherit";

        reldiv.style.top = (getOffset(ap).top) + "px";
        document.getElementById('<%=txtTag.ClientID %>').focus();
    }

    function cancelAddMetadata() {
        var reldiv = document.getElementById("metadataDiv");
        reldiv.style.display = "none";
        edit_holder = -1;
    }

    function commitAddMetadata() {
        var tag = document.getElementById('<%=txtTag.ClientID %>').value;
        var val = document.getElementById('<%=txtValue.ClientID %>').value;
        if (tag.length <= 0) {
            alert("You must enter a tag to commit this metadata.");
            return;
        }
        if (val.length <= 0) {
            alert("You must enter a value to commit this metadata.");
            return;
        }
        if (edit_holder >= 0) {
            var tag_list = document.getElementById('<%=listTags.ClientID %>');
            var val_list = document.getElementById('<%=listValues.ClientID %>');

            tag_list.options[edit_holder].text = tag;
            tag_list.options[edit_holder].value = tag;
            val_list.options[edit_holder].text = val;
            val_list.options[edit_holder].value = val;
        }
        else {
            var dest_list = document.getElementById('<%=listTags.ClientID %>');
            var myOption;
            myOption = document.createElement("Option");
            myOption.text = tag;
            myOption.value = tag;
            dest_list.add(myOption);

            dest_list = document.getElementById('<%=listValues.ClientID %>');
            myOption = document.createElement("Option");
            myOption.text = val;
            myOption.value = val;
            dest_list.add(myOption);
        }
        var reldiv = document.getElementById("metadataDiv");
        reldiv.style.display = "none";
        edit_holder = -1;
    }

    function removeMetadata() {
        var i = 0;
        var tag_list = document.getElementById('<%=listTags.ClientID %>');
        var val_list = document.getElementById('<%=listValues.ClientID %>');

        while (i < tag_list.options.length) {
            var item = tag_list.options[i];
            if (item.selected) {
                tag_list.remove(i);
                val_list.remove(i);
                break;
            }
            else {
                i = i + 1;
            }
        }
    }
</script>
<table border="0">
    <tr>
        <th>
            Tag
        </th>
        <th colspan="2">
            Value
        </th>
    </tr>
    <tr>
        <td colspan="2">
                <asp:ListBox ID="listTags" runat="server" Width="100%" Height="100%"></asp:ListBox>      
        </td>
        <td colspan="2">
                <asp:ListBox ID="listValues" runat="server" Width="100%" Height="100%"></asp:ListBox>
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <table border="0" width="100%">
                <tr>
                    <td>
                        <a class='squarebutton' href='javascript:addMetadata()'><span>Add</span></a>
                    </td>
                    <td>
                        <a class='squarebutton' href='javascript:editMetadata()'><span>Edit</span></a>
                    </td>
                    <td>
                        <a class='squarebutton' href='javascript:removeMetadata()'><span>Remove</span></a>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<div id="metadataDiv" style="border: 1px solid #000; width: 310px; position: absolute;
    margin: 0 auto; background-color: #FFFFFF; display: none;">
    <table border="0">
        <tr>
            <td>
                Tag
            </td>
            <td>
                <asp:TextBox ID="txtTag" runat="server" Width="240px"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td>
                Value
            </td>
            <td>
                <asp:TextBox ID="txtValue" runat="server" Width="240px"></asp:TextBox>
            </td>
        </tr>
        <tr id="controls" runat="server">
            <td>
                <a class='squarebutton' href='javascript:commitAddMetadata()'><span>Commit</span></a>
            </td>
            <td>
                <a class='squarebutton' href='javascript:cancelAddMetadata()'><span>Cancel</span></a>
            </td>
        </tr>
    </table>
</div>
