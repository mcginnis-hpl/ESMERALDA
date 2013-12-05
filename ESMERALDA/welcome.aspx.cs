using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ESMERALDA
{
    public partial class welcome : ESMERALDAPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = GetAppString("appstring_shortname") + " - Welcome";
            pagecontent.InnerHtml = GetAppString("appstring_welcome");
            if (!base.UserIsAdministrator)
            {
                this.adminLink.InnerHtml = string.Empty;
            }
        }
    }
}