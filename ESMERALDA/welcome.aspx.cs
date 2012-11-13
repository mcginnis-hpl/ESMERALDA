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
            if (!base.UserIsAdministrator)
            {
                this.adminLink.InnerHtml = string.Empty;
            }
        }
    }
}